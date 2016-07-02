using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Configuration.Client.Providers;
using Couchbase.Core;
using Couchbase.IO;
using Newtonsoft.Json;
using Orleans.Providers;
using Orleans.Runtime;
using GF.Common;

namespace Orleans.Storage.Couchbase
{
    // A Couchbase storage provider.
    // The storage provider should be included in a deployment by adding this line to the Orleans server configuration file:
    // 
    //     <Provider Type="Orleans.Storage.Couchbase.CouchbaseStorage" Name="CouchbaseStore" ConfigSectionName="couchbaseClients/couchbaseDataStore" /> 
    // and this line to any grain that uses it:
    // 
    //     [StorageProvider(ProviderName = "CouchbaseStore")]
    // 
    // The name 'CouchbaseStore' is an arbitrary choice.
    public class OrleansCouchbaseStorage : IStorageProvider
    {
        //---------------------------------------------------------------------
        public Logger Log { get; protected set; }
        public string ConfigSectionName { get; set; }
        public string Name { get; protected set; }
        public bool UseGuidAsStorageKey { get; protected set; }
        GrainStateCouchbaseDataManager DataManager { get; set; }

        //---------------------------------------------------------------------
        // Initializes the storage provider.
        // <param name="name">The name of this provider instance.</param>
        // <param name="providerRuntime">A Orleans runtime object managing all storage providers.</param>
        // <param name="config">Configuration info for this provider instance.</param>
        // <returns>Completion promise for this operation.</returns> 
        public async Task Init(string name, IProviderRuntime provider_runtime, IProviderConfiguration config)
        {
            this.Name = name;
            this.ConfigSectionName = config.Properties["ConfigSectionName"];
            string useGuidAsStorageKeyString;
            config.Properties.TryGetValue("UseGuidAsStorageKey", out useGuidAsStorageKeyString);
            var useGuidAsStorageKey = true;//default is true

            if (!string.IsNullOrWhiteSpace(useGuidAsStorageKeyString))
            {
                Boolean.TryParse(useGuidAsStorageKeyString, out useGuidAsStorageKey);
            }

            this.UseGuidAsStorageKey = useGuidAsStorageKey;

            if (string.IsNullOrWhiteSpace(ConfigSectionName)) throw new ArgumentException("ConfigSectionName property not set");
            var configSection = ReadConfig(ConfigSectionName);
            DataManager = await GrainStateCouchbaseDataManager.Initialize(configSection);
            Log = provider_runtime.GetLogger(this.GetType().FullName);
        }

        //---------------------------------------------------------------------
        public Task Close()
        {
            if (DataManager != null)
            {
                DataManager.Dispose();
                DataManager = null;
            }

            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        // Reads persisted state from the backing store and deserializes it into the the target
        // grain state object.
        // <param name="grainType">A string holding the name of the grain class.</param>
        // <param name="grainReference">Represents the long-lived identity of the grain.</param>
        // <param name="grainState">A reference to an object to hold the persisted state of the grain.</param>
        // <returns>Completion promise for this operation.</returns>
        public async Task ReadStateAsync(string grain_type, GrainReference grain_reference, IGrainState grain_state)
        {
            if (DataManager == null) throw new ArgumentException("DataManager property not initialized");

            string key = _genKey(grain_type, grain_reference);
            var entity_data = await DataManager.ReadAsync(key);

            grain_state.State = entity_data;
        }

        //---------------------------------------------------------------------
        // Writes the persisted state from a grain state object into its backing store.
        // <param name="grainType">A string holding the name of the grain class.</param>
        // <param name="grainReference">Represents the long-lived identity of the grain.</param>
        // <param name="grainState">A reference to an object holding the persisted state of the grain.</param>
        // <returns>Completion promise for this operation.</returns>
        public Task WriteStateAsync(string grain_type, GrainReference grain_reference, IGrainState grain_state)
        {
            if (DataManager == null) throw new ArgumentException("DataManager property not initialized");

            string key = _genKey(grain_type, grain_reference);
            var entity_data = JsonConvert.SerializeObject(grain_state.State);

            return DataManager.WriteAsync(key, entity_data);
        }

        //---------------------------------------------------------------------
        // Removes grain state from its backing store, if found.
        // <param name="grainType">A string holding the name of the grain class.</param>
        // <param name="grainReference">Represents the long-lived identity of the grain.</param>
        // <param name="grainState">An object holding the persisted state of the grain.</param>
        public Task ClearStateAsync(string grain_type, GrainReference grain_reference, IGrainState grain_state)
        {
            if (DataManager == null) throw new ArgumentException("DataManager property not initialized");

            string key = _genKey(grain_type, grain_reference);
            return DataManager.DeleteAsync(key);
        }

        //---------------------------------------------------------------------
        string _genKey(string grain_type, GrainReference grain_reference)
        {
            var key = grain_type + "_";
            string key_extend;
            var key_guid = grain_reference.GetPrimaryKey(out key_extend);
            if (string.IsNullOrEmpty(key_extend))
            {
                key += key_guid.ToString();
            }
            else
            {
                key += key_extend;
            }
            return key;
        }

        //---------------------------------------------------------------------
        private CouchbaseClientSection ReadConfig(string section_name)
        {
            var section = (CouchbaseClientSection)ConfigurationManager.GetSection(section_name);
            if (section.Servers.Count == 0) throw new ArgumentException("Couchbase servers not set");

            return section;
        }
    }

    // Interfaces with a Couchbase database driver.
    public class GrainStateCouchbaseDataManager
    {
        //---------------------------------------------------------------------
        public static GrainStateCouchbaseDataManager Instance { get; private set; }
        public static Cluster Cluster { get; set; }
        public IBucket Bucket { get; set; }

        //---------------------------------------------------------------------
        private GrainStateCouchbaseDataManager()
        {
            Instance = this;
        }

        //---------------------------------------------------------------------
        public static async Task<GrainStateCouchbaseDataManager> Initialize(CouchbaseClientSection configSection)
        {
            var instance = new GrainStateCouchbaseDataManager();
            var config = new ClientConfiguration(configSection);
            Cluster = new Cluster(config);

            var tcs = new TaskCompletionSource<IBucket>();
            Action initAction;
            if (configSection.Buckets.Count > 0)
            {
                var buckets = new BucketElement[configSection.Buckets.Count];
                configSection.Buckets.CopyTo(buckets, 0);

                var bucketSetting = buckets.First();
                initAction = () => { tcs.SetResult(Cluster.OpenBucket(bucketSetting.Name, bucketSetting.Password)); };
            }
            else
            {
                initAction = () => { tcs.SetResult(Cluster.OpenBucket()); };
            }

            WaitCallback initBucket = (state) =>
            {
                try { initAction(); }
                catch (Exception ex) { tcs.SetException(new Exception("GrainStateCouchbaseDataManager initialize exception", ex)); }
            };

            ThreadPool.QueueUserWorkItem(initBucket, null);

            instance.Bucket = await tcs.Task;

            return instance;
        }

        //---------------------------------------------------------------------
        public async Task DeleteAsync(string key)
        {
            var opResult = await this.Bucket.RemoveAsync(key);

            if (!opResult.Success && opResult.Status != ResponseStatus.KeyNotFound)
            {
                throw new Exception("Delete data from couchbase error", opResult.Exception);
            }
        }

        //---------------------------------------------------------------------
        public async Task<EntityData> ReadAsync(string key)
        {
            var op_result = await this.Bucket.GetAsync<string>(key);

            EntityData result = null;

            if (op_result.Status == ResponseStatus.Success)
            {
                var setting = new JsonSerializerSettings();
                result = (EntityData)JsonConvert.DeserializeObject(op_result.Value, typeof(EntityData));
            }
            else if (op_result.Status == ResponseStatus.KeyNotFound)
            {
            }
            else
            {
                throw new Exception("Read from couchbase server error", op_result.Exception);
            }

            return result;
        }

        //---------------------------------------------------------------------
        public async Task WriteAsync(string key, string entity_data)
        {
            await this.Bucket.UpsertAsync(key, entity_data);
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            if (this.Bucket != null)
            {
                this.Bucket.Dispose();
            }
        }
    }
}

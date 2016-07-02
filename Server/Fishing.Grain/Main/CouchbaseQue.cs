using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Configuration.Client.Providers;
using Couchbase.Core;
using Couchbase.IO;
using Couchbase.N1QL;
using Newtonsoft.Json;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using GF.Common;

public class CouchbaseQueData
{
    public string que_id;
    public ulong seq;
    public int type;
    public Dictionary<string, string> map_data;
}

public class QueryCouchbaseQueData : Document<QueryCouchbaseQueData>
{
    public string que_id;
    public ulong seq;
    public int type;
    public Dictionary<string, string> map_data;
}

public class CouchbaseQue
{
    //-------------------------------------------------------------------------
    public int Count { get { return Que.Count; } }
    Queue<CouchbaseQueData> Que { get; set; }
    string QueId { get; set; }
    string QueKeyPrefix { get; set; }
    bool Quering { get; set; }

    //-------------------------------------------------------------------------
    public CouchbaseQue(string que_type, string que_id)
    {
        Que = new Queue<CouchbaseQueData>();
        QueId = que_id;
        QueKeyPrefix = string.Format("Que{0}_{1}", que_type, que_id);
        Quering = false;
    }

    //-------------------------------------------------------------------------
    public async Task<CouchbaseQueData> popData()
    {
        CouchbaseQueData que_data = new CouchbaseQueData();
        que_data.que_id = "";
        que_data.seq = 0;
        que_data.type = (int)CouchbaseQueDataType.None;
        que_data.map_data = null;

        if (Count == 0) return que_data;

        CouchbaseQueData que_data1 = Que.Dequeue();
        que_data.que_id = que_data1.que_id;
        que_data.seq = que_data1.seq;
        que_data.type = que_data1.type;
        que_data.map_data = que_data1.map_data;

        string key = string.Format("{0}_{1}", QueKeyPrefix, que_data.seq);
        await DbClientCouchbase.Instance.asyncRemove(key);

        return que_data;
    }

    //-------------------------------------------------------------------------
    public async Task pushData(CouchbaseQueData que_data)
    {
        ulong seq = await DbClientCouchbase.Instance.IncrementAsync(QueKeyPrefix);
        que_data.seq = seq;
        que_data.que_id = QueId;

        string key = string.Format("{0}_{1}", QueKeyPrefix, seq);
        string data = EbTool.jsonSerialize(que_data);

        await DbClientCouchbase.Instance.asyncSave(key, data);
    }

    //-------------------------------------------------------------------------
    public async Task queryThenCacheAllData()
    {
        if (Que.Count > 0) return;

        if (Quering) return;
        Quering = true;

        string query = string.Format(@"SELECT que_id, seq, type, map_data FROM Fishing
                WHERE que_id=$1
                ORDER BY seq LIMIT 10;");

        var query_request = QueryRequest.Create(query)
               .ScanConsistency(ScanConsistency.RequestPlus)
               .AddPositionalParameter(QueId);
        var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<QueryCouchbaseQueData>(query_request);
        if (result.Success && result.Rows.Count > 0)
        {
            foreach (var i in result.Rows)
            {
                QueryCouchbaseQueData q = i;
                if (q != null && !string.IsNullOrEmpty(q.que_id))
                {
                    CouchbaseQueData que_data = new CouchbaseQueData();
                    que_data.que_id = q.que_id;
                    que_data.seq = q.seq;
                    que_data.type = q.type;
                    que_data.map_data = q.map_data;

                    Que.Enqueue(que_data);
                }
            }
        }

        Quering = false;
    }
}

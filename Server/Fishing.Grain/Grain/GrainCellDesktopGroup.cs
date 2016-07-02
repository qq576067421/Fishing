using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Providers;
using Orleans.Streams;
using GF.Common;

namespace Ps
{
    // 牌桌组Grain，有状态，可重入
    [Reentrant]
    [StorageProvider(ProviderName = "CouchbaseStore")]
    public class GrainCellDesktopGroup : Grain<EntityData>, ICellDesktopGroup
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }
        public IGrainFactory GF { get { return this.GrainFactory; } }
        public IAsyncStream<StreamData> AsyncStream { get; set; }
        //Entity EtDesktop { get; set; }
        IDisposable TimerHandleSave { get; set; }
        IDisposable TimerHandleUpdate { get; set; }
        Stopwatch StopwatchUpdate { get; set; }

        //---------------------------------------------------------------------
        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromHours(24));

            //if (this.State == null || string.IsNullOrEmpty(this.State.entity_guid))
            //{
            //    this.State = new EntityData();
            //    this.State.entity_type = typeof(EtDesktop).Name;
            //    this.State.entity_guid = this.GetPrimaryKey().ToString();
            //    this.State.list_component = new List<ComponentData>();
            //}

            IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
            AsyncStream = stream_provider.GetStream<StreamData>(this.GetPrimaryKey(), null);

            //EtDesktop = EntityMgr.Instance.genEntity<EtDesktop, GrainCellDesktop>(this.State, this);

            StopwatchUpdate = new Stopwatch();
            TimerHandleUpdate = RegisterTimer((_) => _update(), null, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200));
            TimerHandleSave = RegisterTimer((_) => _save(), null, TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(10000));

            return base.OnActivateAsync();
        }

        //---------------------------------------------------------------------
        public override Task OnDeactivateAsync()
        {
            TimerHandleUpdate.Dispose();
            TimerHandleSave.Dispose();

            //EtDesktop.close();

            Logger.Info("GrainCellDesktopGroup.OnDeactivateAsync()");

            return base.OnDeactivateAsync();
        }

        //---------------------------------------------------------------------
        // 初始化桌子信息
        Task ICellDesktopGroup.s2sSetupDesktop(DesktopInfo desktop_info)
        {
            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        public IStreamProvider getStreamProvider()
        {
            IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
            return stream_provider;
        }

        //---------------------------------------------------------------------
        Task _update()
        {
            //if (EtDesktop != null)
            //{
            //    EtDesktop.update((float)StopwatchUpdate.Elapsed.TotalSeconds);
            //    StopwatchUpdate.Restart();
            //}

            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        Task _save()
        {
            //if (EtDesktop != null)
            //{
            //    var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            //    return co_desktop.save();
            //}

            return TaskDone.Done;
        }
    }
}

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
    // 牌桌Grain，有状态，可重入
    [Reentrant]
    [StorageProvider(ProviderName = "CouchbaseStore")]
    public class GrainCellDesktop : Grain<EntityData>, ICellDesktop
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }
        public IGrainFactory GF { get { return this.GrainFactory; } }
        public IAsyncStream<StreamData> AsyncStream { get; set; }
        Entity EtDesktop { get; set; }
        IDisposable TimerHandleSave { get; set; }
        IDisposable TimerHandleUpdate { get; set; }
        Stopwatch StopwatchUpdate { get; set; }

        //---------------------------------------------------------------------
        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromHours(24));

            if (this.State == null || string.IsNullOrEmpty(this.State.entity_guid))
            {
                this.State = new EntityData();
                this.State.entity_type = typeof(EtDesktop).Name;
                this.State.entity_guid = this.GetPrimaryKey().ToString();
                this.State.list_component = new List<ComponentData>();
            }

            IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
            AsyncStream = stream_provider.GetStream<StreamData>(this.GetPrimaryKey(), null);

            EtDesktop = EntityMgr.Instance.genEntity<EtDesktop, GrainCellDesktop>(this.State, this);

            StopwatchUpdate = new Stopwatch();
            TimerHandleUpdate = RegisterTimer((_) => _update(), null,
                TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200));
            TimerHandleSave = RegisterTimer((_) => _save(), null,
                TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(10000));

            return base.OnActivateAsync();
        }

        //---------------------------------------------------------------------
        public override Task OnDeactivateAsync()
        {
            TimerHandleUpdate.Dispose();

            EtDesktop.close();

            Logger.Info("OnDeactivateAsync()");

            return base.OnDeactivateAsync();
        }

        //---------------------------------------------------------------------
        // 初始化桌子信息
        Task ICellDesktop.s2sSetupDesktop(DesktopInfo desktop_info)
        {
            var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            return co_desktop.s2sSetupDesktop(desktop_info);
        }

        //---------------------------------------------------------------------
        // 获取桌子信息
        Task<DesktopInfo> ICellDesktop.s2sGetDesktopInfo()
        {
            var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            return co_desktop.s2sGetDesktopInfo();
        }

        //---------------------------------------------------------------------
        // 玩家进入桌子
        Task<DesktopData> ICellDesktop.s2sPlayerEnter(DesktopRequestPlayerEnter request_enter,
            EntityData etdata_playermirror)
        {
            var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            return co_desktop.s2sPlayerEnter(request_enter, etdata_playermirror);
        }

        //---------------------------------------------------------------------
        // 玩家离开桌子
        Task<DesktopPlayerLeaveInfo> ICellDesktop.s2sPlayerLeave(string player_etguid)
        {
            var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            return co_desktop.s2sPlayerLeave(player_etguid);
        }

        //---------------------------------------------------------------------
        // 桌子内聊天广播
        Task ICellDesktop.s2sDesktopChat(ChatMsgRecv msg)
        {
            var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            return co_desktop.s2sDesktopChat(msg);
        }

        //---------------------------------------------------------------------
        // 玩家本轮操作取消托管
        Task ICellDesktop.s2sPlayerCancelAutoAction(string player_etguid)
        {
            var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            return co_desktop.s2sPlayerCancelAutoAction(player_etguid);
        }

        //---------------------------------------------------------------------
        // 玩家操作请求
        Task ICellDesktop.s2sPlayerActionRequest(string player_etguid, List<string> vec_param)
        {
            var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
            return co_desktop.s2sPlayerActionRequest(player_etguid, vec_param);
        }

        //---------------------------------------------------------------------
        public IStreamProvider getStreamProvider()
        {
            IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
            return stream_provider;
        }

        //---------------------------------------------------------------------
        public void signDestroy()
        {
            this.DeactivateOnIdle();
        }

        //---------------------------------------------------------------------
        Task _update()
        {
            if (EtDesktop != null)
            {
                EtDesktop.update((float)StopwatchUpdate.Elapsed.TotalSeconds);
                StopwatchUpdate.Restart();
            }

            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        Task _save()
        {
            if (EtDesktop != null)
            {
                var co_desktop = EtDesktop.getComponent<CellDesktop<DefDesktop>>();
                return co_desktop.save();
            }

            return TaskDone.Done;
        }
    }
}

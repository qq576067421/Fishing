using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using GF.Common;

namespace Ps
{
    // 玩家Grain，有状态，可重入
    [Reentrant]
    [StorageProvider(ProviderName = "CouchbaseStore")]
    public class GrainCellPlayer : Grain<EntityData>, ICellPlayer
    {
        //---------------------------------------------------------------------
        public CouchbaseQue CouchbaseQue { get; private set; }
        public Logger Logger { get { return GetLogger(); } }
        public IGrainFactory GF { get { return this.GrainFactory; } }
        public IAsyncStream<StreamData> AsyncStream { get; set; }
        public ObserverSubscriptionManager<ICellPlayerObserver> Subscribers { get; private set; }
        IDisposable TimerHandleUpdate { get; set; }
        IDisposable TimerHandleSave { get; set; }
        Stopwatch StopwatchUpdate { get; set; }
        Entity EtPlayer { get; set; }
        float Tm4CouchbaseQue { get; set; }

        //---------------------------------------------------------------------
        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromMinutes(60));

            Tm4CouchbaseQue = 0f;
            Subscribers = new ObserverSubscriptionManager<ICellPlayerObserver>();

            if (this.State != null && !string.IsNullOrEmpty(this.State.entity_guid))
            {
                if (AsyncStream == null)
                {
                    IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
                    AsyncStream = stream_provider.GetStream<StreamData>(this.GetPrimaryKey(), "Friend");
                }

                if (CouchbaseQue == null)
                {
                    CouchbaseQue = new CouchbaseQue("EtPlayer", this.State.entity_guid);
                }

                EtPlayer = EntityMgr.Instance.genEntity<EtPlayer, GrainCellPlayer>(this.State, this);
            }

            StopwatchUpdate = new Stopwatch();
            TimerHandleUpdate = RegisterTimer((_) => _update(), null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(50));
            TimerHandleSave = RegisterTimer((_) => _save(), null, TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(10000));

            return base.OnActivateAsync();
        }

        //---------------------------------------------------------------------
        public override Task OnDeactivateAsync()
        {
            TimerHandleUpdate.Dispose();
            TimerHandleSave.Dispose();

            if (EtPlayer != null)
            {
                EtPlayer.close();
                EtPlayer = null;
            }

            Subscribers.Clear();

            Logger.Info("OnDeactivateAsync()");

            return base.OnDeactivateAsync();
        }

        //---------------------------------------------------------------------
        Task ICellPlayer.subPlayer(ICellPlayerObserver sub, IPEndPoint remote_endpoint)
        {
            if (Subscribers.Count > 0)
            {
                Logger.Info("subPlayer() 重复登陆，踢出前一帐号 Subscribers.Count={0}", Subscribers.Count);

                AccountNotify account_notify;
                account_notify.id = AccountNotifyId.Logout;
                account_notify.data = EbTool.protobufSerialize<ProtocolResult>(ProtocolResult.LogoutNewLogin);

                MethodData notify_data = new MethodData();
                notify_data.method_id = MethodType.s2cAccountNotify;
                notify_data.param1 = EbTool.protobufSerialize<AccountNotify>(account_notify);
                Subscribers.Notify((s) => s.s2cNotify(notify_data));
            }

            Subscribers.Subscribe(sub);

            var co_player = EtPlayer.getComponent<CellPlayer<DefPlayer>>();
            co_player.c2sClientAttach(remote_endpoint);

            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        async Task ICellPlayer.unsubPlayer(ICellPlayerObserver sub)
        {
            bool is_sub = Subscribers.IsSubscribed(sub);
            if (is_sub) Subscribers.Unsubscribe(sub);

            // 通知EtPlayer服务端：客户端断开了
            if (Subscribers.Count == 0 && EtPlayer != null)
            {
                var co_player = EtPlayer.getComponent<CellPlayer<DefPlayer>>();
                await co_player.c2sClientDeattach();

                DeactivateOnIdle();
            }
        }

        //---------------------------------------------------------------------
        // Client->Cell的请求
        async Task<EntityData> ICellPlayer.c2sEnterWorld(NewPlayerInfo new_player_info)
        {
            Logger.Info("c2sEnterWorld() GrainId={0}", this.GetPrimaryKey().ToString());

            // 新建角色
            if (EtPlayer == null)
            {
                if (AsyncStream == null)
                {
                    IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
                    AsyncStream = stream_provider.GetStream<StreamData>(this.GetPrimaryKey(), "Friend");
                }

                if (CouchbaseQue == null)
                {
                    CouchbaseQue = new CouchbaseQue("EtPlayer", new_player_info.et_player_guid);
                }

                bool exist = true;
                ulong player_id = 100;
                //do
                //{
                //    player_id = (ulong)CellApp.Instance.Rd.next(1000000, 9999999);
                //    var grain_playerservice = GF.GetGrain<ICellPlayerService>(0);
                //    exist = await grain_playerservice.playerIdExist(player_id);
                //} while (exist);

                EbLog.Note("-------------------------------");
                EbLog.Note("新创建玩家");
                EbLog.Note("AccountName=" + new_player_info.account_name);
                EbLog.Note("PlayerId=" + player_id);
                EbLog.Note("-------------------------------");

                var et_player_data = new EntityData();
                et_player_data.entity_type = typeof(EtPlayer).Name;
                et_player_data.entity_guid = new_player_info.et_player_guid;
                et_player_data.list_component = new List<ComponentData>();
                et_player_data.cache_data = new Dictionary<string, object>();
                et_player_data.cache_data["NewPlayer"] = true;
                et_player_data.cache_data["NewPlayerInfo"] = new_player_info;
                et_player_data.cache_data["NewPlayerId"] = player_id;
                EtPlayer = EntityMgr.Instance.genEntity<EtPlayer, GrainCellPlayer>(et_player_data, this);
                State = EtPlayer.genEntityData4SaveDb();
                await WriteStateAsync();
            }

            return EtPlayer.genEntityData4NetSync((byte)NodeType.Client);
        }

        //---------------------------------------------------------------------
        // Client->Cell的请求
        async Task<MethodData> ICellPlayer.c2sRequest(MethodData method_data)
        {
            var co_player = EtPlayer.getComponent<CellPlayer<DefPlayer>>();
            MethodData result = await co_player.c2sRequest(method_data);
            return result;
        }

        //---------------------------------------------------------------------
        // Cell->Client的通知
        Task ICellPlayer.s2sNotify(MethodData method_data)
        {
            Subscribers.Notify((s) => s.s2cNotify(method_data));

            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        // Cell->Cell的请求
        async Task ICellPlayer.s2sBotEnterWorld(string player_etguid)
        {
            NewPlayerInfo new_player_info = new NewPlayerInfo();
            new_player_info.account_id = "";
            new_player_info.account_name = "";
            new_player_info.gender = true;
            new_player_info.is_bot = true;
            new_player_info.et_player_guid = player_etguid;

            var grain = GF.GetGrain<ICellPlayer>(new Guid(player_etguid));
            await grain.c2sEnterWorld(new_player_info);
        }

        //---------------------------------------------------------------------
        // Cell->Cell的请求
        Task ICellPlayer.s2sDesktop2Player(List<string> vec_param)
        {
            if (EtPlayer == null) return TaskDone.Done;

            var co_playerdesktop = EtPlayer.getComponent<CellPlayerDesktop<DefPlayerDesktop>>();
            return co_playerdesktop.s2sDesktop2Player(vec_param);
        }

        //---------------------------------------------------------------------
        async Task _update()
        {
            float tm = (float)StopwatchUpdate.Elapsed.TotalSeconds;
            StopwatchUpdate.Restart();

            if (EtPlayer != null)
            {
                EtPlayer.update(tm);

                if (CouchbaseQue != null)
                {
                    Tm4CouchbaseQue += tm;
                    if (Tm4CouchbaseQue > 1f)
                    {
                        Tm4CouchbaseQue = 0f;
                        await CouchbaseQue.queryThenCacheAllData();
                    }

                    if (CouchbaseQue.Count > 0)
                    {
                        var que_data = await CouchbaseQue.popData();
                        if (que_data.type > 0)
                        {
                            var co_player = EtPlayer.getComponent<CellPlayer<DefPlayer>>();
                            await co_player.recvCouchbaseQueData(que_data);
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        Task _save()
        {
            if (EtPlayer == null) return TaskDone.Done;

            State = EtPlayer.genEntityData4SaveDb();
            WriteStateAsync();

            var co_player = EtPlayer.getComponent<CellPlayer<DefPlayer>>();
            if (co_player.CachePlayerData.player_server_state != PlayerServerState.Offline)
            {
                string data = EbTool.jsonSerialize(co_player.CachePlayerData);
                DbClientCouchbase.Instance.asyncTouch(co_player.CachePlayerKey, TimeSpan.FromSeconds(15.0));
            }

            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        public IStreamProvider getStreamProvider()
        {
            IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
            return stream_provider;
        }

        //---------------------------------------------------------------------
        public Task asyncSaveState()
        {
            if (EtPlayer == null) return TaskDone.Done;

            State = EtPlayer.genEntityData4SaveDb();
            WriteStateAsync();

            return TaskDone.Done;
        }
    }
}

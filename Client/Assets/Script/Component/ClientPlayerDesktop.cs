using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public enum PlayerOperateType
    {
        None = 0,
    }

    public class RenderListener : IRenderListener
    {
        //-------------------------------------------------------------------------
        public ClientPlayerDesktop<DefPlayerDesktop> CoDesktop { get; private set; }

        //-------------------------------------------------------------------------
        public RenderListener(IComponent co_desktop)
        {
            CoDesktop = (ClientPlayerDesktop<DefPlayerDesktop>)co_desktop;
        }

        //-------------------------------------------------------------------------
        public int onGetPlayerGold(uint et_player_rpcid)
        {
            return 1000;
        }

        //-------------------------------------------------------------------------
        public void onSceneCreated()
        {
            EbLog.Note("RenderListener.onSceneCreated()");

            // 向服务器请求获取场景初始化数据
            CoDesktop.Scene.c2sSnapshotScene();
        }

        //-------------------------------------------------------------------------
        public void onSceneFire(uint et_player_rpcid, int player_gold)
        {
            EbLog.Note("RenderListener.onSceneFire()");
        }

        //-------------------------------------------------------------------------
        public void onSceneFishDie(uint et_player_rpcid, int player_gold, int fish_vibid, int fish_gold)
        {
            EbLog.Note("RenderListener.onSceneFishDie()");
        }

        //-------------------------------------------------------------------------
        public void onSceneNoBullet(uint et_player_rpcid)
        {
            EbLog.Note("RenderListener.onSceneNoBullet()");
        }

        //-------------------------------------------------------------------------
        public void onScenePlayerChange2Ob()
        {
            EbLog.Note("RenderListener.onScenePlayerChange2Ob()");
        }

        //-------------------------------------------------------------------------
        // Render给Logic发送场景请求
        public void onSceneRender2Logic(List<string> vec_param)
        {
            CoDesktop.requestSceneRender2Logic(vec_param);
        }

        //-------------------------------------------------------------------------
        public void onSceneShowMessageBox(string content, bool is_ok, string btn_text,
            int count_down, int info_level, bool is_colider, bool is_change_sit)
        {
        }

        //-------------------------------------------------------------------------
        public void onSceneSnapshot()
        {
            EbLog.Note("RenderListener.onSceneSnapshot()");

            CoDesktop.Init = true;
            CoDesktop.Scene.displaySceneStateInfo();
        }

        //-------------------------------------------------------------------------
        public void onSceneTurretCreated(int index)
        {
        }

        //-------------------------------------------------------------------------
        public void onSetPlayerGold(uint et_player_rpcid, int new_gold)
        {
        }

        //-------------------------------------------------------------------------
        public void onSetPlayerTurretRate(uint et_player_rpcid, int turret_id, int rate)
        {
        }
    }

    // 玩家场景管理
    public class ClientPlayerDesktop<TDef> : Component<TDef> where TDef : DefPlayerDesktop, new()
    {
        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }
        public ClientPlayer<DefPlayer> CoPlayer { get; private set; }
        public DesktopConfigData DesktopConfigData { get; private set; }
        public Dictionary<string, Entity> MapEtActorMirrorByGuid { get; private set; }
        public Entity MeMirror { get; set; }
        public SeatInfo[] AllSeat { get; set; }// 所有座位

        public CRenderScene Scene = null;
        public bool Init = false;
        uint mMyPlayerId = 1;// 本人玩家id
        bool mbSingle = false;
        delegate void onAoIEvent(byte event_id, List<string> vec_param);
        Dictionary<byte, onAoIEvent> mMapAoIEvent = new Dictionary<byte, onAoIEvent>();

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientPlayerDesktop.init()");

            defNodeRpcMethod<DesktopResponse>(
                (ushort)MethodType.s2cPlayerDesktopResponse, s2cPlayerDesktopResponse);
            defNodeRpcMethod<DesktopNotify>(
                (ushort)MethodType.s2cPlayerDesktopNotify, s2cPlayerDesktopNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            Entity et_player = EntityMgr.findFirstEntityByType<EtPlayer>();
            CoPlayer = et_player.getComponent<ClientPlayer<DefPlayer>>();

            DesktopConfigData = new DesktopConfigData();
            MapEtActorMirrorByGuid = new Dictionary<string, Entity>();

            mMapAoIEvent[(byte)_eAoIEvent.SceneBroadcast] = onAoIEventSceneBroadcast;
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            foreach (var i in MapEtActorMirrorByGuid)
            {
                i.Value.close();
            }
            MapEtActorMirrorByGuid.Clear();

            AllSeat = null;

            if (Scene != null)
            {
                Scene.Dispose();
                Scene = null;
            }

            EbLog.Note("ClientPlayerDesktop.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            foreach (var i in MapEtActorMirrorByGuid)
            {
                i.Value.update(elapsed_tm);
            }

            if (Scene != null)
            {
                Scene.update(elapsed_tm);
            }
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            if (e is EvEntityFixedUpdate)
            {
                var ev = (EvEntityFixedUpdate)e;

                if (Scene != null)
                {
                    Scene.fixedUpdate(ev.tm);
                }
            }
            else if (e is EvUiClickOB)
            {
                // OB（站起）
                requestPlayerOb();
            }
            else if (e is EvUiClickWaitWhile)
            {
                // 暂离
                requestPlayerWaitWhile();
            }
            else if (e is EvUiClickSeat)
            {
                // 坐下
                var ev = (EvUiClickSeat)e;
                byte seat_index = ev.seat_index;
                requestPlayerSitdown(seat_index);
            }
            else if (e is EvUiClickPlayerReturn)
            {
                // 玩家回桌
                requestPlayerReturn();
            }
        }

        //-------------------------------------------------------------------------
        // 玩家桌内聊天通知
        public void onRecvChatFromDesktop(ChatMsgRecv msg_recv)
        {
        }

        //-------------------------------------------------------------------------
        // 玩家暂离
        public void requestPlayerWaitWhile()
        {
            DesktopRequest desktop_request;
            desktop_request.id = DesktopRequestId.PlayerWaitWhile;
            desktop_request.data = null;
            CoApp.rpc(MethodType.c2sPlayerDesktopRequest, desktop_request);
        }

        //-------------------------------------------------------------------------
        // 玩家回桌
        public void requestPlayerReturn()
        {
            DesktopRequest desktop_request;
            desktop_request.id = DesktopRequestId.PlayerReturn;
            desktop_request.data = null;
            CoApp.rpc(MethodType.c2sPlayerDesktopRequest, desktop_request);
        }

        //-------------------------------------------------------------------------
        // 玩家观战
        public void requestPlayerOb()
        {
            EbLog.Note("requestPlayerOb");
            DesktopRequest desktop_request;
            desktop_request.id = DesktopRequestId.PlayerOb;
            desktop_request.data = null;
            CoApp.rpc(MethodType.c2sPlayerDesktopRequest, desktop_request);
        }

        //-------------------------------------------------------------------------
        // 玩家坐下
        public void requestPlayerSitdown(byte seat_index)
        {
            DesktopRequest desktop_request;
            desktop_request.id = DesktopRequestId.PlayerSitdown;
            desktop_request.data = EbTool.protobufSerialize<byte>(seat_index);
            CoApp.rpc(MethodType.c2sPlayerDesktopRequest, desktop_request);
        }

        //-------------------------------------------------------------------------
        public void requestSceneRender2Logic(List<string> vec_param)
        {
            DesktopRequest desktop_request;
            desktop_request.id = DesktopRequestId.PlayerSceneAction;
            desktop_request.data = EbTool.protobufSerialize(vec_param);
            CoApp.rpc(MethodType.c2sPlayerDesktopRequest, desktop_request);
        }

        //-------------------------------------------------------------------------
        // 桌子响应
        void s2cPlayerDesktopResponse(DesktopResponse desktop_response)
        {
            EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopResponse()");

            switch (desktop_response.id)
            {
                case DesktopResponseId.PlayerAutoAction:// 托管
                    {
                    }
                    break;
                case DesktopResponseId.PlayerCancelAutoAction:// 取消托管
                    {
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 桌子通知
        void s2cPlayerDesktopNotify(DesktopNotify desktop_notify)
        {
            switch (desktop_notify.id)
            {
                case DesktopNotifyId.DesktopInit:// 桌子初始化
                    {
                        EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopNotify() DesktopInit");

                        CoPlayer.CoPlayerLobby.hideLobby();

                        clearDesktop();

                        var desktop_data = EbTool.protobufDeserialize<DesktopData>(desktop_notify.data);

                        DesktopConfigData = desktop_data.desktop_cfg_data;
                        //UiMbPlayDesktop ui_desk = UiMgr.Instance.createUi<UiMbPlayDesktop>(_eUiLayer.Background);
                        //ui_desk.setDeskInfo(this, 60f);

                        byte index = 0;
                        AllSeat = new SeatInfo[DesktopConfigData.seat_num];
                        foreach (var i in AllSeat)
                        {
                            SeatInfo seat_info = new SeatInfo();
                            seat_info.index = index;
                            seat_info.et_playermirror = null;
                            AllSeat[index] = seat_info;
                            index++;
                        }

                        EntityData me_data = desktop_data.list_actormirror.Find((EntityData entity_data) =>
                        {
                            return entity_data.entity_guid.Equals(CoPlayer.Entity.Guid);
                        });

                        _initActorMirror(me_data);

                        foreach (var i in desktop_data.list_actormirror)
                        {
                            if (i.entity_guid.Equals(CoPlayer.Entity.Guid)) continue;

                            _initActorMirror(i);
                        }

                        var co_me = MeMirror.getComponent<ClientActorMirror<DefActorMirror>>();
                        byte me_id = co_me.Def.PropActorIdInDesktop.get();
                        RenderListener listener = new RenderListener(this);
                        Scene = new CRenderScene();
                        //var loading = UiMgr.Instance.createUi<UiMbLoading>(_eUiLayer.Loading);
                        //Scene.onSceneLoading = loading.setLoadProgress;//null;//ui_mgr.getLoading().setRateOfProgress;
                        //Scene.create(mMyPlayerId, mbSingle, false, listener, "RenderSceneConfigure.json",
                        //    new JsonPacketReader("Media/Fishing/FishLord/").readJsonPacketList(),
                        //    new JsonPacketReader("Media/Fishing/Route/").readRouteJsonPacketList());
                    }
                    break;
                case DesktopNotifyId.PlayerSceneAction:// 玩家场景操作
                    {
                        var vec_param = EbTool.protobufDeserialize<List<string>>(desktop_notify.data);

                        if (Scene != null)
                        {
                            Scene.sceneOnRecvFromLogic(vec_param);
                        }
                    }
                    break;
                case DesktopNotifyId.PlayerSceneAoIUpdate:// 玩家场景广播
                    {
                        var ev_aoi = EbTool.protobufDeserialize<_tAoIEvent>(desktop_notify.data);

                        if (Scene != null && Init)
                        {
                            Scene.sceneOnRecvAoIFromLogic(ev_aoi.vec_param);
                        }
                    }
                    break;
                case DesktopNotifyId.PlayerChat:// 玩家聊天广播
                    {
                        if (string.IsNullOrEmpty(DesktopConfigData.desktop_etguid))
                        {
                            return;
                        }

                        var msg_recv = EbTool.protobufDeserialize<ChatMsgRecv>(desktop_notify.data);

                        Entity et_playermirror = null;
                        MapEtActorMirrorByGuid.TryGetValue(msg_recv.et_player_guid_send, out et_playermirror);
                        if (et_playermirror != null)
                        {
                            var co_playermirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
                            co_playermirror.desktopChat(msg_recv);
                        }
                    }
                    break;
                case DesktopNotifyId.PlayerEnter:// 玩家进入桌子
                    {
                        if (string.IsNullOrEmpty(DesktopConfigData.desktop_etguid))
                        {
                            return;
                        }

                        EntityData et_playermirror_data = EbTool.protobufDeserialize<EntityData>(desktop_notify.data);

                        if (MapEtActorMirrorByGuid.ContainsKey(et_playermirror_data.entity_guid))
                        {
                            return;
                        }

                        var et_playermirror = EntityMgr.genEntity<EtPlayerMirror, Entity>(et_playermirror_data, Entity);
                        var co_actormirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
                        MapEtActorMirrorByGuid[et_playermirror.Guid] = et_playermirror;

                        //byte seat_index = co_actormirror.Def.mPropSeatIndex.get();
                        //if (isValidSeatIndex(seat_index))
                        //{
                        //    AllSeat[seat_index].et_playermirror = et_playermirror;
                        //}

                        EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopNotify() PlayerEnter PlayerEtGuid=" + et_playermirror.Guid);
                    }
                    break;
                case DesktopNotifyId.PlayerLeave:// 玩家离开桌子
                    {
                        if (string.IsNullOrEmpty(DesktopConfigData.desktop_etguid))
                        {
                            return;
                        }

                        string player_et_guid = EbTool.protobufDeserialize<string>(desktop_notify.data);

                        EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopNotify() PlayerLeave PlayerEtGuid=" + player_et_guid);

                        Entity et_playermirror = null;
                        if (MapEtActorMirrorByGuid.TryGetValue(player_et_guid, out et_playermirror))
                        {
                            var co_actormirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
                            MapEtActorMirrorByGuid.Remove(player_et_guid);

                            foreach (var i in AllSeat)
                            {
                                if (i.et_playermirror != null && i.et_playermirror.Guid == player_et_guid)
                                {
                                    i.et_playermirror = null;
                                    break;
                                }
                            }

                            et_playermirror.close();
                        }
                    }
                    break;
                case DesktopNotifyId.PlayerOb:// 玩家Ob
                    {
                        if (string.IsNullOrEmpty(DesktopConfigData.desktop_etguid))
                        {
                            return;
                        }

                        string player_etguid = EbTool.protobufDeserialize<string>(desktop_notify.data);

                        EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopNotify() PlayerOb PlayerEtGuid=" + player_etguid);

                        Entity et_playermirror = null;
                        if (MapEtActorMirrorByGuid.TryGetValue(player_etguid, out et_playermirror))
                        {
                            var co_actormirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
                            co_actormirror.playerOb();
                        }
                    }
                    break;
                case DesktopNotifyId.PlayerSitdown:// 玩家坐下
                    {
                        if (string.IsNullOrEmpty(DesktopConfigData.desktop_etguid))
                        {
                            return;
                        }

                        var sitdown_data = EbTool.protobufDeserialize<PlayerSitdownData>(desktop_notify.data);

                        EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopNotify() PlayerSitdown PlayerEtGuid=" + sitdown_data.player_etguid);

                        Entity et_playermirror = null;
                        if (MapEtActorMirrorByGuid.TryGetValue(sitdown_data.player_etguid, out et_playermirror))
                        {
                            var co_actormirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
                            co_actormirror.playerSitdown(sitdown_data.seat_index, sitdown_data.stack, sitdown_data.state);
                        }
                    }
                    break;
                case DesktopNotifyId.PlayerWaitWhile:// 玩家暂离
                    {
                        if (string.IsNullOrEmpty(DesktopConfigData.desktop_etguid))
                        {
                            return;
                        }

                        var waitwhile_data = EbTool.protobufDeserialize<PlayerWaitWhileData>(desktop_notify.data);

                        EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopNotify() PlayerWaitWhile PlayerEtGuid=" + waitwhile_data.player_etguid);

                        Entity et_playermirror = null;
                        if (MapEtActorMirrorByGuid.TryGetValue(waitwhile_data.player_etguid, out et_playermirror))
                        {
                            var co_actormirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
                            co_actormirror.playerWaitWhile(waitwhile_data.wait_while_tm);
                        }
                    }
                    break;
                case DesktopNotifyId.PlayerReturn:// 玩家返回
                    {
                        if (string.IsNullOrEmpty(DesktopConfigData.desktop_etguid))
                        {
                            return;
                        }

                        var return_data = EbTool.protobufDeserialize<PlayerReturnData>(desktop_notify.data);

                        EbLog.Note("ClientPlayerDesktop.s2cPlayerDesktopNotify() PlayerReturn PlayerEtGuid=" + return_data.player_etguid);

                        Entity et_playermirror = null;
                        if (MapEtActorMirrorByGuid.TryGetValue(return_data.player_etguid, out et_playermirror))
                        {
                            var co_actormirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
                            co_actormirror.playerReturn(return_data.stack, return_data.state);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 场景内广播
        void onAoIEventSceneBroadcast(byte event_id, List<string> vec_param)
        {
            try
            {
                if (Scene != null)
                {
                    Scene.sceneOnRecvAoIFromLogic(vec_param);
                }
            }
            catch (Exception e)
            {
                EbLog.Error(e.ToString());
                throw e;
            }
        }

        //-------------------------------------------------------------------------
        public SeatInfo getSeat(string et_player_guid)
        {
            foreach (var i in AllSeat)
            {
                if (i.et_playermirror != null && i.et_playermirror.Guid == et_player_guid)
                {
                    return i;
                }
            }

            return null;
        }

        //-------------------------------------------------------------------------
        public int getPlayerCount()
        {
            int player_num = 0;
            foreach (var i in AllSeat)
            {
                if (i.et_playermirror != null) player_num++;
            }
            return player_num;
        }

        //-------------------------------------------------------------------------
        public void clearDesktop()
        {
            foreach (var i in MapEtActorMirrorByGuid)
            {
                i.Value.close();
            }
            MapEtActorMirrorByGuid.Clear();

            DesktopConfigData = null;
            AllSeat = null;

            //UiMgr.Instance.destroyCurrentUi<UiMbPlayDesktop>();
            //UiMgr.Instance.destroyCurrentUi<UiMbPlayerOperate>();
        }

        //-------------------------------------------------------------------------
        public bool isValidSeatIndex(byte seat_index)
        {
            if (seat_index < 0 || seat_index >= DesktopConfigData.seat_num) return false;
            else return true;
        }

        //-------------------------------------------------------------------------
        void _initActorMirror(EntityData entity_data)
        {
            var et_playermirror = EntityMgr.genEntity<EtPlayerMirror, Entity>(entity_data, Entity);
            MapEtActorMirrorByGuid[et_playermirror.Guid] = et_playermirror;

            //var co_actormirror = et_playermirror.getComponent<ClientActorMirror<DefActorMirror>>();
            //byte seat_index = co_actormirror.Def.mPropSeatIndex.get();
            //if (isValidSeatIndex(seat_index))
            //{
            //    AllSeat[seat_index].et_playermirror = et_playermirror;
            //}
        }
    }
}

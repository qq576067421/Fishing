using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayerLobby<TDef> : Component<TDef> where TDef : DefPlayerLobby, new()
    {
        //-------------------------------------------------------------------------
        public bool InLobby { get; private set; }
        ClientApp<DefApp> CoApp { get; set; }
        ClientPlayer<DefPlayer> CoPlayer { get; set; }
        DesktopSearchFilter DesktopSearchFilter { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientPlayerLobby.init()");

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();
            InLobby = false;

            defNodeRpcMethod<PlayerLobbyResponse>(
                (ushort)MethodType.s2cPlayerLobbyResponse, s2cPlayerLobbyResponse);
            defNodeRpcMethod<PlayerLobbyNotify>(
                (ushort)MethodType.s2cPlayerLobbyNotify, s2cPlayerLobbyNotify);
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientPlayerTrade.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            if (e is EvUiClickSearchDesk)
            {
                // 搜索牌桌
                var ev = (EvUiClickSearchDesk)e;

                enterLobby(ev.desktop_searchfilter);
            }
            else if (e is EvUiClickSearchFriendsDesk)
            {
                // 查找正在玩好友桌子
                //UiMbLobby ui_lobby = UiMgr.Instance.getCurrentUi<UiMbLobby>();
                //ui_lobby.setCurrentPlayingFriend(CoPlayerFriend.FriendListInDesktop);
            }
            else if (e is EvUiClickPlayInDesk)
            {
                // 玩家点击进入桌子玩
                var ev = (EvUiClickPlayInDesk)e;
                CoPlayer.requestEnterDesktop(ev.desk_etguid);
            }
            else if (e is EvUiClickFindSuitDeskTop)
            {
                // 查找合适牌桌
            }
            else if (e is EvUiClickLeaveLobby)
            {
                // 点击上一步，返回主界面            
                leaveLobby();
            }
            else if (e is EvUiRequestGetCurrentFriendPlayDesk)
            {
                // 查询制定玩家所在桌子
                var ev = (EvUiRequestGetCurrentFriendPlayDesk)e;
                requestSearchDesktopFollowFriend(ev.desktop_etguid);
            }
        }

        //-------------------------------------------------------------------------
        // 请求搜索牌桌
        public void requestSearchDesktop(DesktopSearchFilter search_filter)
        {
            // 开始转菊花
            //UiHelper.CreateWaiting("正在找牌桌...", false);

            DesktopSearchFilter = search_filter;

            PlayerLobbyRequest lobby_request;
            lobby_request.id = PlayerLobbyRequestId.SearchDesktop;
            lobby_request.data = EbTool.protobufSerialize(search_filter);

            CoApp.rpc(MethodType.c2sPlayerLobbyRequest, lobby_request);
        }

        //-------------------------------------------------------------------------
        // 请求搜索好友所在牌桌
        public void requestSearchDesktopFollowFriend(string desktop_etguid)
        {
            // 开始转菊花

            PlayerLobbyRequest lobby_request;
            lobby_request.id = PlayerLobbyRequestId.SearchDesktopFollowFriend;
            lobby_request.data = EbTool.protobufSerialize(desktop_etguid);

            CoApp.rpc(MethodType.c2sPlayerLobbyRequest, lobby_request);
        }

        //-------------------------------------------------------------------------
        void s2cPlayerLobbyResponse(PlayerLobbyResponse lobby_response)
        {
            if (!InLobby) return;

            switch (lobby_response.id)
            {
                case PlayerLobbyResponseId.SearchDesktop:// 搜索桌子
                    {
                        EbLog.Note("ClientPlayerLobby.s2cPlayerLobbyResponse() SearchDesktop");

                        // 停止转菊花
                        //UiHelper.DestroyWaiting();
                        var list_desktop_info = EbTool.protobufDeserialize<List<DesktopInfo>>(lobby_response.data);

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityGetLobbyDeskList>();
                        e.list_desktop = list_desktop_info;
                        e.send(null);
                        // 界面显示桌子列表
                    }
                    break;
                case PlayerLobbyResponseId.SearchDesktopFollowFriend:// 搜索玩家所在牌桌
                    {
                        EbLog.Note("ClientPlayerLobby.s2cPlayerLobbyResponse() SearchDesktopFollowFriend");

                        // 停止转菊花
                        //UiHelper.DestroyWaiting();

                        var list_desktop_info = EbTool.protobufDeserialize<List<DesktopInfo>>(lobby_response.data);

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntitySearchDesktopFollowFriend>();
                        e.list_desktop = list_desktop_info;
                        e.send(null);

                        // 界面显示桌子列表
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerLobbyNotify(PlayerLobbyNotify lobby_notify)
        {
            if (!InLobby) return;

            switch (lobby_notify.id)
            {
                //case PlayerNotifyId.SetAFK:
                //    {
                //        //var afk = EbTool.protobufDeserialize<bool>(player_notify.data);
                //        //CoActor.Def.mPropIsAFK.set(afk);
                //    }
                //    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        public void enterLobby(DesktopSearchFilter search_filter)
        {
            InLobby = true;

            requestSearchDesktop(search_filter);
        }

        //-------------------------------------------------------------------------
        public void leaveLobby()
        {
            //UiMgr.Instance.destroyCurrentUi<UiMbLobby>();
            //UiMgr.Instance.destroyCurrentUi<UiMbWaiting>();

            InLobby = false;

            CoPlayer.requestGetOnlinePlayerNum();
        }

        //-------------------------------------------------------------------------
        public void checkShowLobby()
        {
            if (InLobby)
            {
                //UiMbLobby ui_lobby = UiMgr.Instance.getCurrentUi<UiMbLobby>();
                //if (ui_lobby == null)
                //{
                //    UiMgr.Instance.createUi<UiMbLobby>(_eUiLayer.Background);
                //}
            }
        }

        //-------------------------------------------------------------------------
        public void hideLobby()
        {
            //UiMgr.Instance.destroyCurrentUi<UiMbLobby>();
            //UiMgr.Instance.destroyCurrentUi<UiMbWaiting>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayer<TDef> : Component<TDef> where TDef : DefPlayer, new()
    {
        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }
        public ClientActor<DefActor> CoActor { get; private set; }
        public ClientBag<DefBag> CoBag { get; private set; }
        public ClientStatus<DefStatus> CoStatus { get; private set; }
        public ClientPlayerChat<DefPlayerChat> CoPlayerChat { get; set; }
        public ClientPlayerFriend<DefPlayerFriend> CoPlayerFriend { get; set; }
        public ClientPlayerTask<DefPlayerTask> CoPlayerTask { get; set; }
        public ClientPlayerDesktop<DefPlayerDesktop> CoPlayerDesktop { get; set; }
        public ClientPlayerLobby<DefPlayerLobby> CoPlayerLobby { get; set; }
        public ClientPlayerMailBox<DefPlayerMailBox> CoPlayerMailBox { get; set; }
        public ClientPlayerRanking<DefPlayerRanking> CoPlayerRanking { get; set; }
        public int OnlinePlayerNum { get; private set; }// 在线玩家数
        float GetOnlinePlayerNumTimeElapsed { get; set; }
        //_eRequestPlayerInfoReason mRequestPlayerInfoReason;

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientPlayer.init() EntityGuid=" + Entity.Guid);

            defNodeRpcMethod<PlayerResponse>(
                (ushort)MethodType.s2cPlayerResponse, s2cPlayerResponse);
            defNodeRpcMethod<PlayerNotify>(
                (ushort)MethodType.s2cPlayerNotify, s2cPlayerNotify);

            EntityMgr.getDefaultEventPublisher().addHandler(Entity);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            OnlinePlayerNum = 0;
            GetOnlinePlayerNumTimeElapsed = 0f;

            // 销毁EtLogin
            Entity et_login = EntityMgr.findFirstEntityByType<EtLogin>();
            EntityMgr.destroyEntity(et_login);

            // ClientActor组件
            CoActor = Entity.getComponent<ClientActor<DefActor>>();

            // ClientBag组件
            CoBag = Entity.getComponent<ClientBag<DefBag>>();

            // ClientStatus组件
            CoStatus = Entity.getComponent<ClientStatus<DefStatus>>();

            // ClientPlayerChat组件
            CoPlayerChat = Entity.getComponent<ClientPlayerChat<DefPlayerChat>>();

            // ClientPlayerFriend组件
            CoPlayerFriend = Entity.getComponent<ClientPlayerFriend<DefPlayerFriend>>();

            // ClientPlayerTask组件
            CoPlayerTask = Entity.getComponent<ClientPlayerTask<DefPlayerTask>>();

            // ClientPlayerDesktop组件
            CoPlayerDesktop = Entity.getComponent<ClientPlayerDesktop<DefPlayerDesktop>>();

            // ClientPlayerLobby组件
            CoPlayerLobby = Entity.getComponent<ClientPlayerLobby<DefPlayerLobby>>();

            // ClientPlayerMailBox组件
            CoPlayerMailBox = Entity.getComponent<ClientPlayerMailBox<DefPlayerMailBox>>();

            // ClientPlayerRanking组件
            CoPlayerRanking = Entity.getComponent<ClientPlayerRanking<DefPlayerRanking>>();

            // 显示主界面
            //UiMgr.Instance.CoPlayer = (ClientPlayer<DefPlayer>)(IComponent)this;
            //UiMgr.Instance.NeedChangeAtlas = CoActor.Def.mPropIsVIP.get();
            //UiMgr.Instance.destroyCurrentUi<UiMbLoading>();
            //createMainUi();
            var ui_main = UiMgr.Instance.createUi<UiMain>("Main", "Main");
            ui_main.init();

            // 请求获取在线玩家数量
            requestGetOnlinePlayerNum();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            //UiMbLogin login = UiMgr.Instance.getCurrentUi<UiMbLogin>();
            //UiMgr.Instance.destroyAllUi(login);

            EntityMgr.getDefaultEventPublisher().removeHandler(Entity);

            EbLog.Note("ClientPlayer.release() EntityGuid=" + Entity.Guid);
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            // 定时10秒，请求获取在线玩家数量
            GetOnlinePlayerNumTimeElapsed += elapsed_tm;
            if (GetOnlinePlayerNumTimeElapsed > 10f)
            {
                GetOnlinePlayerNumTimeElapsed = 0f;

                requestGetOnlinePlayerNum();
            }
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            if (e is EvUiClickShop)
            {
                var ev = (EvUiClickShop)e;

                // 点击主界面商城
            }
            else if (e is EvUiClickFriend)
            {
                var ev = (EvUiClickFriend)e;

                // 点击主界面好友
            }
            else if (e is EvUiClickMsg)
            {
                var ev = (EvUiClickMsg)e;

                // 点击主界面消息
            }
            else if (e is EvUiClickHelp)
            {
                var ev = (EvUiClickHelp)e;

                // 点击主界面帮助
            }
            else if (e is EvUiClickEdit)
            {
                var ev = (EvUiClickEdit)e;

                // 点击主界面编辑
            }
            else if (e is EvUiClickLogin)
            {
                var ev = (EvUiClickLogin)e;

                EbLog.Note("ClientPlayer.handleEvent() 响应点击登陆按钮消息");

                // 点击主界面登陆
                CoApp.CoNetMonitor.disconnect();
            }
            else if (e is EvUiClickPlayNow)
            {
                var ev = (EvUiClickPlayNow)e;

                // 点击主界面立即玩
                //requestPlayNow();
            }
            //else if (e is EvUiClickCreateDeskTop)
            //{
            //    var ev = (EvUiClickCreateDeskTop)e;

            //    // 请求创建私人牌桌
            //    int seat_num = ev.seat_num;
            //    PrivateDesktopCreateInfo desktop_createinfo;
            //    desktop_createinfo.seat_num = seat_num;
            //    desktop_createinfo.desktop_tableid = ev.desk_topinfo_id;
            //    desktop_createinfo.is_vip = CoActor.Def.mPropIsVIP.get();

            //    requestCreatePrivateDesktop(desktop_createinfo);
            //}
            else if (e is EvUiClickExitDesk)
            {
                // 点击离开桌子
                requestLeaveDesktop();
            }
            //else if (e is EvUiClickEnterDesktop)
            //{
            //    requestEnterDesktopAny();
            //}
            //else if (e is EvUiClickGetSelfPlayerInfo)
            //{
            //    var ev = (EvUiClickGetSelfPlayerInfo)e;

            //    // 点击主界面玩家信息
            //}
            //else if (e is EvUiClickGetPlayerProfile)
            //{
            //    var ev = (EvUiClickGetPlayerProfile)e;
            //    requestGetPlayerInfoOther(ev.player_etguid);
            //    //requestGetPlayerInfo(ev.player_etguid, _eRequestPlayerInfoReason.PlayerProfile);
            //}
            else if (e is EvUiClickChangePlayerNickName)
            {
                // 改昵称
                var ev = (EvUiClickChangePlayerNickName)e;

                requestChangeNickName(ev.new_name);
            }
            else if (e is EvUiClickChangePlayerIndividualSignature)
            {
                // 改签名
                var ev = (EvUiClickChangePlayerIndividualSignature)e;

                requestChangeIndividualSignature(ev.new_individual_signature);
            }
            else if (e is EvUiClickChangePlayerProfileSkin)
            {
                // 换肤
                var ev = (EvUiClickChangePlayerProfileSkin)e;
                requestChangePlayerProfileSkin(ev.skin_id);
            }
            else if (e is EvUiClickRefreshIPAddress)
            {
                // 刷新IP所在地
                requestRefreshIpAddress();
            }
            else if (e is EvUiReportFriend)
            {
                // 举报好友
                var ev = (EvUiReportFriend)e;
                string et_guid = ev.friend_etguid;
                ReportPlayerType report_type = ev.report_type;
                requestReprotPlayer(et_guid, report_type);
            }
            else if (e is EvUiInviteFriendPlayTogether)
            {
                // 邀请好友一起玩
                var ev = (EvUiInviteFriendPlayTogether)e;
                DesktopConfigData desktop_configdata = CoPlayerDesktop.DesktopConfigData;

                if (desktop_configdata != null && !string.IsNullOrEmpty(desktop_configdata.desktop_etguid))
                {
                    requestInvitePlayerEnterDesktop(ev.friend_guid, desktop_configdata.desktop_etguid,
                        0, 0, CoPlayerDesktop.getPlayerCount(), desktop_configdata.seat_num);
                }
            }
            else if (e is EvUiClickChipTransaction)
            {
                // 点击送筹码
                var ev = (EvUiClickChipTransaction)e;

                if (CoActor.Def.PropGold.get() < 100000)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "筹码不足10万，不能发送!";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
                else
                {
                    //UiMbChipTransaction chip_transaction = UiMgr.Instance.createUi<UiMbChipTransaction>(_eUiLayer.MessgeBox);
                    //chip_transaction.setChipTransactionInfo(CoActor.Def.mPropChip.get(), CoActor.Def.mPropChip.get(),
                    //    100000, CoPlayerFriend.getFriendItem(ev.send_target_etguid));
                }
            }
            else if (e is EvUiCreateMainUi)
            {
                createMainUi();
            }
            else if (e is EvUiCreateExchangeChip)
            {
                // 创建换钱UI
                //UiMbExchangeChip chip_transaction = UiMgr.Instance.createUi<UiMbExchangeChip>(_eUiLayer.MessgeBox);
                //chip_transaction.setExchangeInfo(CoActor.Def.PropGold.get(), CoActor.Def.PropGold.get(), 100);
            }
            else if (e is EvUiRequestGetRankPlayerInfo)
            {
                // 请求排行榜玩家信息
                var ev = (EvUiRequestGetRankPlayerInfo)e;
                requestGetPlayerInfoOther(ev.player_etguid);
            }
            else if (e is EvUiRequestPlayerInfoOther)
            {
                // 请求其他玩家详细信息
                var ev = (EvUiRequestPlayerInfoOther)e;
                requestGetPlayerInfoOther(ev.player_etguid);
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerResponse(PlayerResponse player_response)
        {
            switch (player_response.id)
            {
                case PlayerResponseId.CreatePrivateDesktop:// 创建私有桌子
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() CreatePrivateDesktop");
                    }
                    break;
                case PlayerResponseId.LeaveDesktop:// 离开桌子
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() LeaveDesktop");

                        CoPlayerDesktop.clearDesktop();
                        CoPlayerLobby.checkShowLobby();

                        requestGetOnlinePlayerNum();
                    }
                    break;
                case PlayerResponseId.GetOnlinePlayerNum:// 获取在线玩家数
                    {
                        var online_player_num = EbTool.protobufDeserialize<int>(player_response.data);

                        //EbLog.Note("ClientPlayer.s2cPlayerResponse() GetOnlinePlayerNum Num=" + online_player_num);

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntitySetOnLinePlayerNum>();
                        e.online_num = online_player_num;
                        e.send(null);
                    }
                    break;
                case PlayerResponseId.GetPlayerInfoOther:// 获取牌桌上其他玩家信息
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() GetPlayerInfoOther");

                        var player_info = EbTool.protobufDeserialize<PlayerInfoOther>(player_response.data);

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityResponsePlayerInfoOther>();
                        e.player_info = player_info;
                        e.send(null);

                        _createPlayerInfoUi(player_info);
                        //switch (mRequestPlayerInfoReason)
                        //{
                        //    case _eRequestPlayerInfoReason.PlayerProfile:
                        //        _createPlayerInfoUi(player_info);
                        //        break;
                        //    case _eRequestPlayerInfoReason.RequestAddFriend:
                        //        UiMbAgreeOrDisAddFriendRequest friend_request = UiMgr.Instance.getCurrentUi<UiMbAgreeOrDisAddFriendRequest>();
                        //        if (friend_request != null)
                        //        {
                        //            friend_request.setPlayerInfo(player_info);
                        //        }
                        //        break;
                        //    case _eRequestPlayerInfoReason.ResponseAddFriend:
                        //        UiMbMsgBox msg_box = UiMgr.Instance.createUi<UiMbMsgBox>(_eUiLayer.MessgeBox, false);
                        //        msg_box.setNotifyMsgInfo("加好友", player_info.nick_name + "成为你的好友!");
                        //        break;
                        //    case _eRequestPlayerInfoReason.GetRankPlayerInfo:
                        //        _createPlayerInfoUi(player_info);
                        //        break;
                        //    default:
                        //        break;
                        //}
                    }
                    break;
                case PlayerResponseId.ChangeProfileSkin:// 个人资料换肤
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() ChangeProfileSkin");

                        var profileskin_tableid = EbTool.protobufDeserialize<int>(player_response.data);

                        //FloatMsgInfo f_info;
                        //f_info.msg = "换肤成功,请在游戏中查看!";
                        //f_info.color = Color.green;
                        //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                    }
                    break;
                case PlayerResponseId.RefreshIpAddress:// 刷新Ip所在地
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() RefreshIpAddress");

                        var ip_address = EbTool.protobufDeserialize<string>(player_response.data);

                        //UiMgr.Instance.destroyCurrentUi<UiMbWaiting>();

                        //FloatMsgInfo f_info;
                        //f_info.msg = "所在地刷新成功!";
                        //f_info.color = Color.green;
                        //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                    }
                    break;
                case PlayerResponseId.ReportPlayer:// 举报玩家
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() ReportPlayer");

                        var report = EbTool.protobufDeserialize<ReportPlayer>(player_response.data);

                        //FloatMsgInfo f_info;
                        //f_info.msg = "举报玩家成功!";
                        //f_info.color = Color.green;
                        //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                    }
                    break;
                case PlayerResponseId.InvitePlayerEnterDesktop:// 邀请玩家进桌
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() InvitePlayerEnterDesktop");

                        var r = EbTool.protobufDeserialize<ProtocolResult>(player_response.data);
                        if (r == ProtocolResult.Success)
                        {
                            //FloatMsgInfo f_info;
                            //f_info.msg = "邀请好友进桌发送成功!";
                            //f_info.color = Color.green;
                            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                        }
                    }
                    break;
                case PlayerResponseId.GivePlayerChip:// 赠送玩家筹码
                    {
                        EbLog.Note("ClientPlayer.s2cPlayerResponse() GivePlayerChip");

                        //var r = EbTool.protobufDeserialize<ProtocolResult>(player_response.data);
                        //FloatMsgInfo f_info;
                        //if (r == ProtocolResult.Success)
                        //{
                        //    f_info.msg = "赠送玩家筹码成功!";
                        //    f_info.color = Color.green;
                        //}
                        //else
                        //{
                        //    f_info.msg = "赠送玩家筹码失败!";
                        //    f_info.color = Color.red;
                        //}

                        //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerNotify(PlayerNotify player_notify)
        {
            switch (player_notify.id)
            {
                case PlayerNotifyId.InvitePlayerEnterDesktop:// 收到好友入桌邀请
                    {
                        var invite = EbTool.protobufDeserialize<InvitePlayerEnterDesktop>(player_notify.data);

                        var friend_item = invite.player_info;//CoPlayerFriend.getFriendInfo();
                        if (friend_item != null)
                        {
                            //UiMbMsgBox mb_msg = UiMgr.Instance.createUi<UiMbMsgBox>(_eUiLayer.MessgeBox, false);
                            //mb_msg.setMsgInfo("好友打牌邀请", "玩家: " + friend_item.nick_name + "\n下注: $" +
                            //    UiChipShowHelper.getChipShowStr(invite.sb) + "/$" + UiChipShowHelper.getChipShowStr(invite.bb)
                            //    + "\n玩家: " + invite.player_num + "/" + invite.seat_num, (bool accept) =>
                            //    {
                            //        if (accept)
                            //        {
                            //            requestEnterDesktop(invite.desktop_etguid);
                            //        }
                            //    });
                        }
                    }
                    break;
                case PlayerNotifyId.GivePlayerChip:// 收到玩家赠送筹码
                    {
                        var give_chip = EbTool.protobufDeserialize<GivePlayerChip>(player_notify.data);
                        var friend_item = give_chip.player_info;//CoPlayerFriend.getFriendInfo(give_chip.player_etguid);

                        //if (friend_item != null)
                        //{
                        //    UiMbMsgBox mb_msg = UiMgr.Instance.createUi<UiMbMsgBox>(_eUiLayer.MessgeBox, false);
                        //    mb_msg.setNotifyMsgInfo("好友赠送筹码", "你的好友 " + friend_item.nick_name + "送给你 $" +
                        //        UiChipShowHelper.getChipShowStr(give_chip.chip) + "筹码");
                        //}
                    }
                    break;
                case PlayerNotifyId.SetAFK:
                    {
                        var afk = EbTool.protobufDeserialize<bool>(player_notify.data);

                        CoActor.Def.mPropIsAFK.set(afk);
                    }
                    break;
                case PlayerNotifyId.ActorMapPropDirty:// Actor属性集变脏
                    {
                        var map_prop_dirty = EbTool.protobufDeserialize<Dictionary<string, string>>(player_notify.data);

                        if (map_prop_dirty == null) return;
                        CoActor.Def.applyMapPropDirty(map_prop_dirty);
                    }
                    break;
                case PlayerNotifyId.Levelup:// 玩家升级
                    {
                        var level_new = EbTool.protobufDeserialize<int>(player_notify.data);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 请求进入指定桌子
        public void requestEnterDesktop(string desktop_etguid)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.EnterDesktop;
            player_request.data = EbTool.protobufSerialize(desktop_etguid);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求进入任意桌子
        public void requestEnterDesktopAny()
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.EnterDesktopAny;
            player_request.data = null;
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求创建私人牌桌
        public void requestCreatePrivateDesktop(PrivateDesktopCreateInfo desktop_createinfo)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.CreatePrivateDesktop;
            player_request.data = EbTool.protobufSerialize(desktop_createinfo);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求离开桌子
        public void requestLeaveDesktop()
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.LeaveDesktop;
            player_request.data = null;
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求获取在线玩家总数
        public void requestGetOnlinePlayerNum()
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.GetOnlinePlayerNum;
            player_request.data = null;
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 控制台命令添加道具
        public void requestDevconsoleAddItem(int item_id, int count)
        {
            EbLog.Note("ClientPlayer.devconsoleAddItem() item_id=" + item_id + " count=" + count);

            Dictionary<byte, string> map_param = new Dictionary<byte, string>();
            map_param[0] = "AddItem";// Cmd
            map_param[1] = item_id.ToString();
            map_param[2] = count.ToString();

            PlayerRequest player_request;
            player_request.id = PlayerRequestId.DevConsoleCmd;
            player_request.data = EbTool.protobufSerialize<Dictionary<byte, string>>(map_param);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 控制台命令操作道具
        public void requestDevconsoleOperateItem(ushort item_objid, string operate_id)
        {
            EbLog.Note("ClientPlayer.devconsoleOperateItem() item_objid=" + item_objid);

            Dictionary<byte, string> map_param = new Dictionary<byte, string>();
            map_param[0] = "OperateItem";// Cmd
            map_param[1] = operate_id;
            map_param[2] = item_objid.ToString();

            PlayerRequest player_request;
            player_request.id = PlayerRequestId.DevConsoleCmd;
            player_request.data = EbTool.protobufSerialize<Dictionary<byte, string>>(map_param);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 控制台命令设置自己等级
        public void requestDevconsoleSetLevel(int level, int exp)
        {
            EbLog.Note("ClientPlayer.devconsoleSetLevel() level=" + level + " exp=" + exp);

            Dictionary<byte, string> map_param = new Dictionary<byte, string>();
            map_param[0] = "SetLevel";// Cmd
            map_param[1] = level.ToString();
            map_param[2] = exp.ToString();

            PlayerRequest player_request;
            player_request.id = PlayerRequestId.DevConsoleCmd;
            player_request.data = EbTool.protobufSerialize<Dictionary<byte, string>>(map_param);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求获取牌桌上其他玩家信息
        public void requestGetPlayerInfoOther(string et_player_guid)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.GetPlayerInfoOther;
            player_request.data = EbTool.protobufSerialize<string>(et_player_guid);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求换肤
        public void requestChangePlayerProfileSkin(int profileskin_tableid)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.ChangeProfileSkin;
            player_request.data = EbTool.protobufSerialize<int>(profileskin_tableid);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求改昵称
        public void requestChangeNickName(string nick_name)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.ChangeNickName;
            player_request.data = EbTool.protobufSerialize<string>(nick_name);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求改个人签名
        public void requestChangeIndividualSignature(string sign)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.ChangeIndividualSignature;
            player_request.data = EbTool.protobufSerialize<string>(sign);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求刷新Ip所在地
        public void requestRefreshIpAddress()
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.RefreshIpAddress;
            player_request.data = null;
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求举报玩家
        public void requestReprotPlayer(string player_etguid, ReportPlayerType report_type)
        {
            ReportPlayer report = new ReportPlayer();
            report.player_etguid = player_etguid;
            report.report_type = report_type;

            PlayerRequest player_request;
            player_request.id = PlayerRequestId.ReportPlayer;
            player_request.data = EbTool.protobufSerialize<ReportPlayer>(report);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求邀请玩家入桌
        public void requestInvitePlayerEnterDesktop(string friend_etguid,
            string desktop_etguid, int sb, int bb, int player_num, int seat_num)
        {
            PlayerInfo player_info = new PlayerInfo();
            player_info.player_etguid = friend_etguid;

            InvitePlayerEnterDesktop invite = new InvitePlayerEnterDesktop();
            invite.player_info = player_info;
            invite.desktop_etguid = desktop_etguid;
            invite.sb = sb;
            invite.bb = bb;
            invite.player_num = player_num;
            invite.seat_num = seat_num;

            PlayerRequest player_request;
            player_request.id = PlayerRequestId.InvitePlayerEnterDesktop;
            player_request.data = EbTool.protobufSerialize<InvitePlayerEnterDesktop>(invite);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        // 请求赠送玩家筹码
        public void requestGivePlayerChip(string player_etguid, int chip)
        {
            PlayerInfo player_info = new PlayerInfo();
            player_info.player_etguid = player_etguid;

            GivePlayerChip give_chip = new GivePlayerChip();
            give_chip.player_info = player_info;
            give_chip.chip = chip;

            PlayerRequest player_request;
            player_request.id = PlayerRequestId.GivePlayerChip;
            player_request.data = EbTool.protobufSerialize<GivePlayerChip>(give_chip);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        public void requestSetVip4Test(bool is_vip)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.SetVip4Test;
            player_request.data = EbTool.protobufSerialize<bool>(is_vip);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        public void requestSetAFK()
        {
            // 请求AFK或者取消AFK
            bool afk = CoActor.Def.mPropIsAFK.get();

            PlayerRequest player_request;
            player_request.id = PlayerRequestId.SetAFK;
            player_request.data = EbTool.protobufSerialize<bool>(!afk);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        public void requestSetAFK(bool afk)
        {
            PlayerRequest player_request;
            player_request.id = PlayerRequestId.SetAFK;
            player_request.data = EbTool.protobufSerialize<bool>(afk);
            CoApp.rpc(MethodType.c2sPlayerRequest, player_request);
        }

        //-------------------------------------------------------------------------
        public void createMainUi()
        {
            //UiMbMain mb_main = UiMgr.Instance.createUi<UiMbMain>(_eUiLayer.Background);
            //mb_main.setPlayerInfo(CoActor.Def);
            //mb_main.setCurrentPlayerNumInfo(0);
            //mb_main.setSystemEventListCount(CoPlayerMailBox.Def.mPropListSystemEvent.get().Count);
            //mb_main.setFriendInfo(CoPlayerFriend.Def.mPropMapFriend.get());
        }

        //-------------------------------------------------------------------------
        void _createPlayerInfoUi(PlayerInfoOther player_info)
        {
            if (player_info == null)
            {
                EbLog.Note("PlayerInfo Is Null");
                return;
            }

            var tb_profile = EbDataMgr.Instance.getData<TbDataPlayerProfileSkin>(player_info.profileskin_tableid);

            if (tb_profile != null)
            {
                //UiMbPlayerProfileOther ui_profileother = UiMgr.Instance.createUi<UiMbPlayerProfileOther>(
                //_eUiLayer.MessgeBox, true, null, null, tb_profile.ProfileSkinPrefabName);
                //ui_profileother.setPlayerInfo(player_info);
            }
        }
    }
}

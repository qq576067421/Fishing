using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayerFriend<TDef> : Component<TDef> where TDef : DefPlayerFriend, new()
    {
        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; set; }
        public ClientPlayer<DefPlayer> CoPlayer { get; set; }
        public List<PlayerInfo> FriendListInDesktop { get; set; }// 在打牌中的好友列表
        public PlayerInfo CurrentFriendInDesktop { get; set; }// 当前选中的在打牌中的好友
        public Dictionary<string, PlayerInfoFriend> MapFriendInfo { get; private set; }// 好友详细信息
        public Dictionary<string, List<ChatMsgRecord>> MapMsgRecord { get; set; }// 好友消息记录

        //-------------------------------------------------------------------------
        public override void init()
        {
            defNodeRpcMethod<PlayerFriendResponse>(
                (ushort)MethodType.s2cPlayerFriendResponse, s2cPlayerFriendResponse);
            defNodeRpcMethod<PlayerFriendNotify>(
                (ushort)MethodType.s2cPlayerFriendNotify, s2cPlayerFriendNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            Entity et_player = EntityMgr.findFirstEntityByType<EtPlayer>();
            CoPlayer = et_player.getComponent<ClientPlayer<DefPlayer>>();

            FriendListInDesktop = new List<PlayerInfo>();
            MapMsgRecord = new Dictionary<string, List<ChatMsgRecord>>();

            // 请求获取好友列表
            //requestGetFriendList();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            if (e is EvUiFindFriend)
            {
                var ev = (EvUiFindFriend)e;
                requestFindFriend(ev.find_info);
            }
            else if (e is EvUiAddFriend)
            {
                var ev = (EvUiAddFriend)e;
                requestAddFriend(ev.friend_etguid);
            }
            else if (e is EvUiRemoveFriend)
            {
                var ev = (EvUiRemoveFriend)e;
                requestDeleteFriend(ev.friend_etguid);
            }
            else if (e is EvUiAgreeAddFriend)
            {
                //同意加好友
                var ev = (EvUiAgreeAddFriend)e;
                requestAgreeAddFriend(ev.from_etguid, true);
            }
            else if (e is EvUiRefuseAddFriend)
            {
                //拒绝加好友
                var ev = (EvUiRefuseAddFriend)e;
                requestAgreeAddFriend(ev.from_etguid, false);
            }
            else if (e is EvUiRequestPlayerInfoFriend)
            {
                //请求好友详细信息
                var ev = (EvUiRequestPlayerInfoFriend)e;
                if (getFriendPlayerInfoFriend(ev.player_etguid) != null)
                {
                    requestGetPlayerInfoFriend(ev.player_etguid);
                }
            }
        }

        //-------------------------------------------------------------------------
        // 好友响应
        void s2cPlayerFriendResponse(PlayerFriendResponse playerfriend_response)
        {
            switch (playerfriend_response.id)
            {
                case PlayerFriendResponseId.GetPlayerInfoFriend:// s->c, 响应获取好友详细信息
                    {
                        EbLog.Note("ClientPlayerFriend.s2cPlayerFriendResponse() GetPlayerInfoFriend");

                        var player_info = EbTool.protobufDeserialize<PlayerInfoFriend>(playerfriend_response.data);

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityResponsePlayerInfoFriend>();
                        e.player_info = player_info;
                        e.send(null);
                    }
                    break;
                case PlayerFriendResponseId.RequestAddFriend:// s->c, 响应添加好友
                    {
                        EbLog.Note("ClientPlayerFriend.s2cPlayerFriendResponse() RequestAddFriend");

                        var result = EbTool.protobufDeserialize<ProtocolResult>(playerfriend_response.data);

                        if (result == ProtocolResult.Success)
                        {
                            // 添加好友成功
                            //FloatMsgInfo f_info;
                            //f_info.msg = "添加好友请求发送成功";
                            //f_info.color = Color.green;
                            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                        }
                        else if (result == ProtocolResult.FriendExistFriend)
                        {
                            //FloatMsgInfo f_info;
                            //f_info.msg = "已经是好友，无需重复添加";
                            //f_info.color = Color.red;
                            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                        }
                        else
                        {
                            //FloatMsgInfo f_info;
                            //f_info.msg = "添加好友请求发送失败";
                            //f_info.color = Color.red;
                            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                        }
                    }
                    break;
                case PlayerFriendResponseId.DeleteFriend:// s->c, 响应删除好友
                    {
                        EbLog.Note("ClientPlayerFriend.s2cPlayerFriendResponse() DeleteFriend");

                        var result = EbTool.protobufDeserialize<ProtocolResult>(playerfriend_response.data);

                        //FloatMsgInfo f_info;
                        //f_info.msg = "请求删除好友成功";
                        //f_info.color = Color.red;
                        //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                    }
                    break;
                case PlayerFriendResponseId.FindFriend:// s->c, 响应查找好友
                    {
                        EbLog.Note("ClientPlayerFriend.s2cPlayerFriendResponse() FindFriend");

                        var list_friend = EbTool.protobufDeserialize<List<PlayerInfo>>(playerfriend_response.data);

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityFindFriend>();
                        e.list_friend_item = list_friend;
                        e.send(null);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 好友通知
        void s2cPlayerFriendNotify(PlayerFriendNotify playerfriend_notify)
        {
            switch (playerfriend_notify.id)
            {
                case PlayerFriendNotifyId.RecommendPlayerList:// 推荐好友列表
                    {
                        //EbLog.Note("ClientPlayerFriend.s2cPlayerFriendNotify() RecommendPlayerList");

                        var list_recommend = EbTool.protobufDeserialize<List<PlayerInfo>>(playerfriend_notify.data);

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityRecommendPlayerList>();
                        e.list_recommend = list_recommend;
                        e.send(null);
                    }
                    break;
                case PlayerFriendNotifyId.AddFriend:// 通知添加好友
                    {
                        EbLog.Note("ClientPlayerFriend.s2cPlayerFriendNotify() AddFriend");

                        var friend_item = EbTool.protobufDeserialize<PlayerInfo>(playerfriend_notify.data);

                        Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();
                        map_friend[friend_item.player_etguid] = friend_item;

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityNotifyAddFriend>();
                        e.friend_item = friend_item;
                        e.send(null);
                    }
                    break;
                case PlayerFriendNotifyId.DeleteFriend:// 通知删除好友
                    {
                        EbLog.Note("ClientPlayerFriend.s2cPlayerFriendNotify() DeleteFriend");

                        var friend_etguid = EbTool.protobufDeserialize<string>(playerfriend_notify.data);

                        PlayerInfo friend = null;
                        Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();
                        if (map_friend.TryGetValue(friend_etguid, out friend))
                        {
                            map_friend.Remove(friend_etguid);
                        }

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityNotifyDeleteFriend>();
                        e.friend_etguid = friend_etguid;
                        e.send(null);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 请求获取好友详细信息
        public void requestGetPlayerInfoFriend(string player_etguid)
        {
            PlayerFriendRequest playerfriend_request;
            playerfriend_request.id = PlayerFriendRequestId.GetPlayerInfoFriend;
            playerfriend_request.data = EbTool.protobufSerialize<string>(player_etguid);

            CoApp.rpc(MethodType.c2sPlayerRequest, player_etguid);
        }

        //-------------------------------------------------------------------------
        // 请求添加好友
        public void requestAddFriend(string player_etguid)
        {
            PlayerFriendRequest playerfriend_request;
            playerfriend_request.id = PlayerFriendRequestId.RequestAddFriend;
            playerfriend_request.data = EbTool.protobufSerialize<string>(player_etguid);

            CoApp.rpc(MethodType.c2sPlayerFriendRequest, playerfriend_request);
        }

        //-------------------------------------------------------------------------
        // 请求是否同意添加好友
        public void requestAgreeAddFriend(string player_etguid, bool agree)
        {
            AddFriendAgree addfriend_agree;
            addfriend_agree.player_etguid = player_etguid;
            addfriend_agree.agree = agree;

            PlayerFriendRequest playerfriend_request;
            playerfriend_request.id = PlayerFriendRequestId.AgreeAddFriend;
            playerfriend_request.data = EbTool.protobufSerialize<AddFriendAgree>(addfriend_agree);

            CoApp.rpc(MethodType.c2sPlayerFriendRequest, playerfriend_request);
        }

        //-------------------------------------------------------------------------
        // 请求删除好友
        public void requestDeleteFriend(string et_player_guid)
        {
            PlayerFriendRequest playerfriend_request;
            playerfriend_request.id = PlayerFriendRequestId.DeleteFriend;
            playerfriend_request.data = EbTool.protobufSerialize<string>(et_player_guid);

            CoApp.rpc(MethodType.c2sPlayerFriendRequest, playerfriend_request);
        }

        //-------------------------------------------------------------------------
        // 请求查找好友
        public void requestFindFriend(string find_info)
        {
            PlayerFriendRequest playerfriend_request;
            playerfriend_request.id = PlayerFriendRequestId.FindFriend;
            playerfriend_request.data = EbTool.protobufSerialize<string>(find_info);

            CoApp.rpc(MethodType.c2sPlayerFriendRequest, playerfriend_request);
        }

        //-------------------------------------------------------------------------
        // 添加好友聊天消息记录
        public void addFriendMsgRecord(ChatMsgRecv msg)
        {
            string friend_etguid = "";

            ChatMsgRecord record = new ChatMsgRecord();
            if (msg.et_player_guid_send == Entity.Guid)
            {
                record.is_me = true;
                friend_etguid = msg.et_player_guid_recv;
            }
            else
            {
                record.is_me = false;
                friend_etguid = msg.et_player_guid_send;
            }
            record.content = msg.content;
            record.dt = msg.dt;

            List<ChatMsgRecord> list_record = null;
            MapMsgRecord.TryGetValue(friend_etguid, out list_record);
            if (list_record == null)
            {
                list_record = new List<ChatMsgRecord>();
                MapMsgRecord[friend_etguid] = list_record;
            }

            list_record.Add(record);
            if (list_record.Count > 1000) list_record.RemoveAt(0);

            //UiMbChatMsg chat_msg = UiMgr.Instance.getCurrentUi<UiMbChatMsg>();
            //if (chat_msg != null)
            //{
            //    chat_msg.setCurrentChatMsg(record, msg.et_player_guid_send);
            //}
        }

        //-------------------------------------------------------------------------
        public PlayerInfo getFriendInfo(string friend_etguid)
        {
            PlayerInfo friend_item = null;
            Def.mPropMapFriend.get().TryGetValue(friend_etguid, out friend_item);
            return friend_item;
        }

        //-------------------------------------------------------------------------
        public PlayerInfoFriend getFriendPlayerInfoFriend(string friend_etguid)
        {
            PlayerInfoFriend player_info_friend = null;
            MapFriendInfo.TryGetValue(friend_etguid, out player_info_friend);
            return player_info_friend;
        }
    }
}

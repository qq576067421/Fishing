using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayerChat<TDef> : Component<TDef> where TDef : DefPlayerChat, new()
    {
        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }
        public ClientPlayer<DefPlayer> CoPlayer { get; private set; }
        public List<ChatMsgSend> ListSendTextHistory { get; set; }// 本人消息发送记录

        //-------------------------------------------------------------------------
        public override void init()
        {
            defNodeRpcMethod<PlayerChatResponse>(
                (ushort)MethodType.s2cPlayerChatResponse, s2cPlayerChatResponse);
            defNodeRpcMethod<PlayerChatNotify>(
                (ushort)MethodType.s2cPlayerChatNotify, s2cPlayerChatNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            Entity et_player = EntityMgr.findFirstEntityByType<EtPlayer>();
            CoPlayer = et_player.getComponent<ClientPlayer<DefPlayer>>();
            ListSendTextHistory = new List<ChatMsgSend>();
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
            if (e is EvUiSendMsg)
            {
                var ev = (EvUiSendMsg)e;
                requestSendChat(ev.chat_type, ev.target_guid, !ev.is_text_msg, ev.msg_content);
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerChatResponse(PlayerChatResponse playerchat_response)
        {
            switch (playerchat_response.id)
            {
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerChatNotify(PlayerChatNotify playerchat_notify)
        {
            switch (playerchat_notify.id)
            {
                case PlayerChatNotifyId.RecvChatMsg:
                    {
                        EbLog.Note("ClientPlayerChat.s2cPlayerChatNotify() RecvChatMsg");

                        var msg_recv = EbTool.protobufDeserialize<ChatMsgRecv>(playerchat_notify.data);

                        // 好友聊天消息记录
                        if (msg_recv.chat_type == ChatType.Friend)
                        {
                            var co_friend = Entity.getComponent<ClientPlayerFriend<DefPlayerFriend>>();
                            co_friend.addFriendMsgRecord(msg_recv);

                            CoPlayer.CoPlayerMailBox.haveNewMsg(true);
                        }

                        // 桌内聊天广播，转发给桌子
                        if (msg_recv.chat_type == ChatType.Desktop)
                        {
                            CoPlayer.CoPlayerDesktop.onRecvChatFromDesktop(msg_recv);
                        }

                        // 广播接收Chat消息
                        //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityChatAddChatContent>();
                        //e.chat_info = chat_info;
                        //e.send(null);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 请求发送消息
        public void requestSendChat(ChatType chat_type, string dst_et_guid, bool is_emotion, string msg)
        {
            ChatMsgSend msg_send = new ChatMsgSend();
            msg_send.chat_type = chat_type;
            msg_send.et_player_guid_recv = dst_et_guid;
            msg_send.is_emotion = is_emotion;
            msg_send.content = msg;

            ListSendTextHistory.Add(msg_send);
            if (ListSendTextHistory.Count > 50) ListSendTextHistory.RemoveAt(0);

            PlayerChatRequest playerchat_request;
            playerchat_request.id = PlayerChatRequestId.SendChatMsg;
            playerchat_request.data = EbTool.protobufSerialize<ChatMsgSend>(msg_send);

            CoApp.rpc(MethodType.c2sPlayerChatRequest, playerchat_request);
        }
    }
}

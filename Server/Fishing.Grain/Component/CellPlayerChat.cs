using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellPlayerChat<TDef> : Component<TDef> where TDef : DefPlayerChat, new()
{
    //-------------------------------------------------------------------------
    CellActor<DefActor> CoActor { get; set; }
    CellPlayer<DefPlayer> CoPlayer { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        EnableSave2Db = false;
        EnableNetSync = false;

        CoActor = Entity.getComponent<CellActor<DefActor>>();
        CoPlayer = Entity.getComponent<CellPlayer<DefPlayer>>();
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
    }

    //-------------------------------------------------------------------------
    public Task<MethodData> c2sPlayerChatRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var playerchat_request = EbTool.protobufDeserialize<PlayerChatRequest>(method_data.param1);
        switch (playerchat_request.id)
        {
            case PlayerChatRequestId.SendChatMsg:
                {
                    var msg = EbTool.protobufDeserialize<ChatMsgSend>(playerchat_request.data);

                    switch (msg.chat_type)
                    {
                        case ChatType.Desktop:// 牌桌中聊天广播
                            {
                                ChatMsgRecv msg_recv = new ChatMsgRecv();
                                msg_recv.chat_type = msg.chat_type;
                                msg_recv.is_emotion = msg.is_emotion;
                                msg_recv.content = msg.content;
                                msg_recv.et_player_guid_recv = msg.et_player_guid_recv;
                                msg_recv.et_player_guid_send = Entity.Guid;
                                msg_recv.dt = DateTime.Now;

                                CoPlayer.CoPlayerDesktop.c2sSendDesktopChat(msg_recv);
                            }
                            break;
                        case ChatType.Friend:// 好友聊天
                            {
                                // 返回给自己
                                ChatMsgRecv msg_recv = new ChatMsgRecv();
                                msg_recv.chat_type = msg.chat_type;
                                msg_recv.is_emotion = msg.is_emotion;
                                msg_recv.content = msg.content;
                                msg_recv.et_player_guid_recv = msg.et_player_guid_recv;
                                msg_recv.et_player_guid_send = Entity.Guid;
                                msg_recv.dt = DateTime.Now;

                                // 发送给好友
                                var grain = Entity.getUserData<GrainCellPlayer>();
                                var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(msg.et_player_guid_recv));
                                grain_playerproxy.s2sRecvChatFromFriend(msg_recv);
                                
                                _s2cChatMsg(msg_recv);
                            }
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }

        return Task.FromResult(result);
    }

    //---------------------------------------------------------------------
    // 好友聊天
    public Task s2sProxyRecvChatFromFriend(ChatMsgRecv msg_recv)
    {
        _s2cChatMsg(msg_recv);

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    // 桌内聊天
    public Task s2sRecvChatFromDesktop(ChatMsgRecv msg_recv)
    {
        _s2cChatMsg(msg_recv);

        return TaskDone.Done;
    }

    //-------------------------------------------------------------------------
    void _s2cChatMsg(ChatMsgRecv msg_recv)
    {
        PlayerChatNotify playerchat_notify;
        playerchat_notify.id = PlayerChatNotifyId.RecvChatMsg;
        playerchat_notify.data = EbTool.protobufSerialize<ChatMsgRecv>(msg_recv);

        MethodData method_data = new MethodData();
        method_data.method_id = MethodType.s2cPlayerChatNotify;
        method_data.param1 = EbTool.protobufSerialize<PlayerChatNotify>(playerchat_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(method_data);
    }
}

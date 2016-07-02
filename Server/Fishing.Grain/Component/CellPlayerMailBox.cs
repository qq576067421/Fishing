using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellPlayerMailBox<TDef> : Component<TDef> where TDef : DefPlayerMailBox, new()
{
    //-------------------------------------------------------------------------
    CellPlayer<DefPlayer> CoPlayer { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
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
    public Task<MethodData> c2sPlayerMailBoxRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var playersecretary_request = EbTool.protobufDeserialize<PlayerMailBoxRequest>(method_data.param1);
        switch (playersecretary_request.id)
        {
            case PlayerMailBoxRequestId.MailBoxInitInfo:// 请求邮箱初始化信息
                {
                    MailBoxInitInfo info = new MailBoxInitInfo();
                    info.list_maildata = Def.mPropListMailData.get();

                    PlayerMailBoxResponse playersecretary_response;
                    playersecretary_response.id = PlayerMailBoxResponseId.MailBoxInitInfo;
                    playersecretary_response.data = EbTool.protobufSerialize<MailBoxInitInfo>(info);

                    result.method_id = MethodType.s2cPlayerMailBoxResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerMailBoxResponse>(playersecretary_response);
                    return Task.FromResult(result);
                }
            case PlayerMailBoxRequestId.MailOperate:// 请求邮件操作
                {
                    var mail_operate = EbTool.protobufDeserialize<MailOperate>(playersecretary_request.data);
                    var list_mail = Def.mPropListMailData.get();
                    list_mail.Find((MailData mail_data) =>
                    {
                        if (mail_data.mail_guid == mail_operate.mail_guid)
                        {
                            if (mail_operate.mail_operate_type == MailOperateType.Read)
                            {
                                mail_data.read = true;
                            }
                            else if (mail_operate.mail_operate_type == MailOperateType.RecvAttachment)
                            {
                                mail_data.recv_attachment = true;
                                mail_data.read = true;

                                // 将附件中的道具放到玩家背包中
                                if (mail_data.list_attachment != null)
                                {
                                    var co_bag = Entity.getComponent<CellBag<DefBag>>();
                                    if (mail_data.list_attachment.Count > co_bag.leftOpenSlotCount())
                                    {
                                        return false;
                                    }

                                    foreach (var i in mail_data.list_attachment)
                                    {
                                        Item item = null;
                                        co_bag.newItem(i, out item);
                                    }
                                }
                            }
                            return true;
                        }
                        return false;
                    });

                    if (mail_operate.mail_operate_type == MailOperateType.Delete)
                    {
                        list_mail.RemoveAll(mail_data => mail_data.mail_guid == mail_operate.mail_guid);
                    }

                    MailOperate mail_operate_result = new MailOperate();
                    mail_operate_result.result = ProtocolResult.Success;
                    mail_operate_result.mail_guid = mail_operate.mail_guid;
                    mail_operate_result.mail_operate_type = mail_operate.mail_operate_type;

                    PlayerMailBoxResponse playersecretary_response;
                    playersecretary_response.id = PlayerMailBoxResponseId.MailOperate;
                    playersecretary_response.data = EbTool.protobufSerialize<MailOperate>(mail_operate_result);

                    result.method_id = MethodType.s2cPlayerMailBoxResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerMailBoxResponse>(playersecretary_response);
                    return Task.FromResult(result);
                }
            case PlayerMailBoxRequestId.DeleteSystemEvent:// 请求删除系统事件
                {
                    var se_guid = EbTool.protobufDeserialize<string>(playersecretary_request.data);
                    List<SystemEvent> list_se = Def.mPropListSystemEvent.get();// 系统事件列表
                    foreach (var i in list_se)
                    {
                        if (i.id == se_guid)
                        {
                            list_se.Remove(i);
                            break;
                        }
                    }
                }
                break;
            default:
                break;
        }

        return Task.FromResult(result);
    }

    //---------------------------------------------------------------------
    public Task s2sProxyRecvMail(MailData mail_data)
    {
        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    // 请求加好友
    public Task s2sProxyRequestAddFriend(PlayerInfo player_info)
    {
        SystemEvent se = new SystemEvent();
        se.id = Guid.NewGuid().ToString();
        se.type = SystemEventType.RequestAddFriend;
        se.data1 = EbTool.jsonSerialize(player_info);

        List<SystemEvent> list_se = Def.mPropListSystemEvent.get();// 系统事件列表
        list_se.Add(se);

        PlayerMailBoxNotify mailbox_notify;
        mailbox_notify.id = PlayerMailBoxNotifyId.NewSystemEvent;
        mailbox_notify.data = EbTool.protobufSerialize<SystemEvent>(se);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerMailBoxNotify;
        notify_data.param1 = EbTool.protobufSerialize<PlayerMailBoxNotify>(mailbox_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    // 响应加好友
    public Task s2sProxyResponseAddFriend(PlayerInfo player_info, bool agree)
    {
        SystemEvent se = new SystemEvent();
        se.id = Guid.NewGuid().ToString();
        se.type = SystemEventType.ResponseAddFriend;
        se.data1 = EbTool.jsonSerialize(player_info);
        se.data2 = agree.ToString();

        List<SystemEvent> list_se = Def.mPropListSystemEvent.get();// 系统事件列表
        list_se.Add(se);

        PlayerMailBoxNotify mailbox_notify;
        mailbox_notify.id = PlayerMailBoxNotifyId.NewSystemEvent;
        mailbox_notify.data = EbTool.protobufSerialize<SystemEvent>(se);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerMailBoxNotify;
        notify_data.param1 = EbTool.protobufSerialize<PlayerMailBoxNotify>(mailbox_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);

        return TaskDone.Done;
    }
}

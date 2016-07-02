using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class ClientPlayerMailBox<TDef> : Component<TDef> where TDef : DefPlayerMailBox, new()
    {
        //-------------------------------------------------------------------------
        ClientApp<DefApp> CoApp { get; set; }
        ClientPlayer<DefPlayer> CoPlayer { get; set; }
        //public List<MailData> ListMailData { get; private set; }
        public bool Init { get; private set; }
        int MsgCount { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientPlayerMailBox.init()");

            defNodeRpcMethod<PlayerMailBoxResponse>(
                (ushort)MethodType.s2cPlayerMailBoxResponse, s2cPlayerMailBoxResponse);
            defNodeRpcMethod<PlayerMailBoxNotify>(
                (ushort)MethodType.s2cPlayerMailBoxNotify, s2cPlayerMailBoxNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();
            //ListMailData = new List<MailData>();
            Init = false;

            requestMailBoxInitInfo();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientPlayerMailBox.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            if (e is EvUiRequestShowSystemEvent)
            {
                List<SystemEvent> list_sysevent = Def.mPropListSystemEvent.get();
                if (list_sysevent.Count > 0)
                {
                    SystemEvent first_event = list_sysevent[0];
                    PlayerInfo player_info = EbTool.jsonDeserialize<PlayerInfo>(first_event.data1);
                    switch (first_event.type)
                    {
                        case SystemEventType.RequestAddFriend:
                            //UiMbAgreeOrDisAddFriendRequest friend_request = UiMgr.Instance.createUi<UiMbAgreeOrDisAddFriendRequest>(_eUiLayer.MessgeBox);
                            //friend_request.setPlayerInfo(player_info);
                            break;
                        case SystemEventType.ResponseAddFriend:
                            //UiMbMsgBox msg_box = UiMgr.Instance.createUi<UiMbMsgBox>(_eUiLayer.MessgeBox, false);
                            //msg_box.setNotifyMsgInfo("加好友", player_info.nick_name + "成为你的好友!");
                            break;
                        default:
                            break;
                    }

                    requestDeleteSystemEvent(first_event.id);
                }
            }
            else if (e is EvUiClickMsg)
            {
                clearNewMsg();
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerMailBoxResponse(PlayerMailBoxResponse playersecretary_response)
        {
            switch (playersecretary_response.id)
            {
                //case PlayerMailBoxResponseId.MailBoxInitInfo:// 响应请求邮箱初始化信息
                //    {
                //        EbLog.Note("ClientPlayerMailBox.s2cPlayerSecretaryResponse() SetupSecreartyInitInfo");

                //        var secretary_init_info = EbTool.protobufDeserialize<MailBoxInitInfo>(playersecretary_response.data);

                //        if (secretary_init_info.list_maildata != null)
                //        {
                //            ListMailData = secretary_init_info.list_maildata;
                //        }
                //    }
                //    break;
                case PlayerMailBoxResponseId.MailOperate:// 响应请求邮件操作
                    {
                        EbLog.Note("ClientPlayerMailBox.s2cPlayerSecretaryResponse() SecretaryMailOperate");

                        var mail_operate = EbTool.protobufDeserialize<MailOperate>(playersecretary_response.data);

                        if (mail_operate.result == ProtocolResult.Success)
                        {
                            var list_maildata = Def.mPropListMailData.get();
                            MailData mail_data = list_maildata.Find((MailData mail) =>
                            {
                                if (mail.mail_guid.Equals(mail_operate.mail_guid))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            });

                            if (mail_data != null)
                            {
                                switch (mail_operate.mail_operate_type)
                                {
                                    case MailOperateType.Delete:
                                        list_maildata.Remove(mail_data);
                                        break;
                                    case MailOperateType.Read:
                                        mail_data.read = true;
                                        break;
                                    case MailOperateType.RecvAttachment:
                                        mail_data.recv_attachment = true;
                                        break;
                                }
                            }

                            //UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityMailInfoChange>().send(null);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerMailBoxNotify(PlayerMailBoxNotify playersecretary_notify)
        {
            switch (playersecretary_notify.id)
            {
                case PlayerMailBoxNotifyId.NewMail:// 通知有新的邮件
                    {
                        EbLog.Note("ClientPlayerMailBox.s2cPlayerMailBoxNotify() NewMail");

                        var list_new_mail_data = EbTool.protobufDeserialize<List<MailData>>(playersecretary_notify.data);

                        if (list_new_mail_data != null)
                        {
                            var list_maildata = Def.mPropListMailData.get();
                            list_maildata.AddRange(list_new_mail_data);
                            //UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityGetNewMail>().send(null);
                        }
                    }
                    break;
                case PlayerMailBoxNotifyId.NewSystemEvent:// 通知有新的系统事件
                    {
                        EbLog.Note("ClientPlayerMailBox.s2cPlayerMailBoxNotify() NewSystemEvent");

                        var se = EbTool.protobufDeserialize<SystemEvent>(playersecretary_notify.data);

                        var list_se = Def.mPropListSystemEvent.get();
                        list_se.Add(se);

                        haveNewMsg();
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 请求初始化邮箱信息
        public void requestMailBoxInitInfo()
        {
            PlayerMailBoxRequest mailbox_request;
            mailbox_request.id = PlayerMailBoxRequestId.MailBoxInitInfo;
            mailbox_request.data = null;

            CoApp.rpc(MethodType.c2sPlayerMailBoxRequest, mailbox_request);
        }

        //-------------------------------------------------------------------------
        // 请求邮件操作
        public void requestMailOperate(MailOperate mail_operate)
        {
            PlayerMailBoxRequest mailbox_request;
            mailbox_request.id = PlayerMailBoxRequestId.MailOperate;
            mailbox_request.data = EbTool.protobufSerialize<MailOperate>(mail_operate);

            CoApp.rpc(MethodType.c2sPlayerMailBoxRequest, mailbox_request);
        }

        //-------------------------------------------------------------------------
        // 请求删除系统事件
        public void requestDeleteSystemEvent(string se_guid)
        {
            PlayerMailBoxRequest mailbox_request;
            mailbox_request.id = PlayerMailBoxRequestId.DeleteSystemEvent;
            mailbox_request.data = EbTool.protobufSerialize<string>(se_guid);

            CoApp.rpc(MethodType.c2sPlayerMailBoxRequest, mailbox_request);

            var list_se = Def.mPropListSystemEvent.get();
            foreach (var i in list_se)
            {
                if (i.id == se_guid)
                {
                    list_se.Remove(i);
                    break;
                }
            }

            //UiMbMain mb_main = UiMgr.Instance.createUi<UiMbMain>();
            //mb_main.setSystemEventListCount(Def.mPropListSystemEvent.get().Count);
        }

        //-------------------------------------------------------------------------
        public void haveNewMsg(bool is_chagmsg = false)
        {
            if (is_chagmsg)
            {
                //UiMbChatMsg mb_chat = UiMgr.Instance.getCurrentUi<UiMbChatMsg>();
                //if (mb_chat != null)
                //{
                //    return;
                //}

                MsgCount++;
            }

            //UiMbMain mb_main = UiMgr.Instance.getCurrentUi<UiMbMain>();

            //if (mb_main != null)
            //{
            //    //mb_main.setSystemEventListCount(Def.mPropListSystemEvent.get().Count + MsgCount);
            //}
        }

        //-------------------------------------------------------------------------
        public void clearNewMsg()
        {
            MsgCount = 0;

            //UiMbMain mb_main = UiMgr.Instance.getCurrentUi<UiMbMain>();

            //if (mb_main != null)
            //{
            //    //mb_main.setSystemEventListCount(Def.mPropListSystemEvent.get().Count + MsgCount);
            //}
        }
    }
}

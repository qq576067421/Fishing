using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientNetMonitor<TDef> : Component<TDef> where TDef : DefNetMonitor, new()
    {
        //-------------------------------------------------------------------------
        EntityRpcSessionSuperSocketC mSession;
        const float mReconnectTmSpan = 5f;
        float mReconnectTm = 0f;
        EcPing mClientPing = new EcPing();

        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }
        public IRpcSession Session { get { return mSession; } }
        public EcPing ClientPing { get { return mClientPing; } }
        public string AccId { get; set; }
        public string Acc { get; set; }
        public string Pwd { get; set; }
        public string Token { get; set; }
        public string BaseIp { get; set; }
        public int BasePort { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientNetMonitor.init()");

            defNodeRpcMethod<AccountResponse>(
                (ushort)MethodType.s2cAccountResponse, s2cAccountResponse);
            defNodeRpcMethod<AccountNotify>(
                (ushort)MethodType.s2cAccountNotify, s2cAccountNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();

            AccId = "";
            Acc = "";
            Token = "";
            BaseIp = "";
            BasePort = 5882;
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            disconnect();

            EbLog.Note("ClientNetMonitor.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (mSession != null)
            {
                mSession.update(elapsed_tm);
            }

            if (mSession != null)
            {
                mClientPing.update(elapsed_tm);
            }

            // 断线重连判定
            if (mSession == null && !string.IsNullOrEmpty(Token))
            {
                mReconnectTm += elapsed_tm;
                //UiMbMessageBox ui_msgbox = UiMgr.Instance.getCurrentUi<UiMbMessageBox>();
                //if (ui_msgbox == null) ui_msgbox = UiMgr.Instance.createUi<UiMbMessageBox>(_eUiLayer.MessgeBox);
                if (mReconnectTm > mReconnectTmSpan)
                {
                    mReconnectTm = 0f;
                    //_connectBase();
                    //ui_msgbox.showMessageLable("断线重连中！", null, null, _disconnection, true, "返回登录");
                }
                else
                {
                    float tm_span = mReconnectTmSpan - mReconnectTm;
                    if (tm_span < 0f) tm_span = 0f;
                    //string info = string.Format("已断线，{0}秒后进行重连！", (int)(tm_span + 1));
                    //ui_msgbox.showMessageLable(info, null, null, _disconnection, true, "返回登录");
                }
            }
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
        }

        //-------------------------------------------------------------------------
        public void connectBase(string base_ip, int base_port)
        {
            if (mSession != null)
            {
                mSession.close();
                mSession = null;
            }

            BaseIp = base_ip;
            BasePort = base_port;

            mSession = new EntityRpcSessionSuperSocketC(EntityMgr);
            mSession.OnSocketConnected = _onSocketConnected;
            mSession.OnSocketClosed = _onSocketClosed;
            mSession.OnSocketError = _onSocketError;
            mSession.connect(BaseIp, BasePort);

            //FloatMsgInfo f_info;
            //f_info.msg = "请求进入游戏世界";
            //f_info.color = Color.green;
            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
        }

        //-------------------------------------------------------------------------
        public void disconnect()
        {
            EbLog.Note("ClientNetMonitor.disconnect()");

            mClientPing.stop();

            if (mSession != null)
            {
                mSession.close();
                mSession = null;
            }

            AccId = "";
            Token = "";
        }

        //-------------------------------------------------------------------------
        void s2cAccountResponse(AccountResponse account_response)
        {
            switch (account_response.id)
            {
                case AccountResponseId.EnterWorld:// 进入游戏世界
                    {
                        var enterworld_response = EbTool.protobufDeserialize<ClientEnterWorldResponse>(account_response.data);

                        if (enterworld_response == null || enterworld_response.result != ProtocolResult.Success
                            || enterworld_response.et_player_data == null
                            || string.IsNullOrEmpty(enterworld_response.et_player_data.entity_guid))
                        {
                            // 进入游戏世界失败，则断开连接

                            string s = "进入游戏世界失败";
                            if (enterworld_response != null)
                            {
                                s = string.Format("进入游戏世界失败，ErrorCode={0}", enterworld_response.result);
                            }

                            //FloatMsgInfo f_info;
                            //f_info.msg = s;
                            //f_info.color = Color.red;
                            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

                            disconnect();
                        }
                        else
                        {
                            // 进入游戏世界成功

                            //FloatMsgInfo f_info;
                            //f_info.msg = "进入游戏世界成功";
                            //f_info.color = Color.green;
                            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

                            Entity et_player = EntityMgr.createEntityByData<EtPlayer>(
                                enterworld_response.et_player_data, CoApp.Entity);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cAccountNotify(AccountNotify account_notify)
        {
            switch (account_notify.id)
            {
                case AccountNotifyId.Logout:
                    {
                        var result = EbTool.protobufDeserialize<ProtocolResult>(account_notify.data);

                        if (result == ProtocolResult.LogoutNewLogin)
                        {
                            //FloatMsgInfo f_info;
                            //f_info.msg = "您的帐号已在别处登陆";
                            //f_info.color = Color.red;
                            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                        }

                        disconnect();
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void _onSocketConnected(object client, EventArgs args)
        {
            // 请求进入游戏世界
            ClientEnterWorldRequest enterworld_request = new ClientEnterWorldRequest();
            enterworld_request.acc_id = AccId;
            //enterworld_request.acc_name = Acc;
            enterworld_request.acc_name = "test1000";
            enterworld_request.token = Token;

            AccountRequest account_request;
            account_request.id = AccountRequestId.EnterWorld;
            account_request.data = EbTool.protobufSerialize(enterworld_request);
            CoApp.rpc(MethodType.c2sAccountRequest, account_request);

            mClientPing.start(BaseIp);
        }

        //-------------------------------------------------------------------------
        void _onSocketClosed(object client, EventArgs args)
        {
            EbLog.Note("ClientNetMonitor._onSocketClosed()");

            _onSocketClose();
        }

        //-------------------------------------------------------------------------
        void _onSocketError(object rec, SocketErrorEventArgs args)
        {
            EbLog.Note("ClientNetMonitor._onSocketError()");

            //FloatMsgInfo f_info;
            //f_info.msg = "连接已断开!";
            //f_info.color = Color.red;
            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

            _onSocketClose();
        }

        //-------------------------------------------------------------------------
        void _onSocketClose()
        {
            //UiMgr.Instance.destroyCurrentUi<UiMbWaiting>();

            mSession = null;

            mClientPing.stop();

            // 销毁玩家
            Entity et_player = EntityMgr.findFirstEntityByType<EtPlayer>();
            if (et_player != null)
            {
                EntityMgr.destroyEntity(et_player);
            }

            // 返回登陆
            if (!EntityMgr.SignDestroy)
            {
                Entity et_login = EntityMgr.findFirstEntityByType<EtLogin>();
                if (et_login == null)
                {
                    EntityMgr.createEntity<EtLogin>(null, CoApp.Entity);
                }
            }
        }
    }
}

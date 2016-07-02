using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;
using GF.UCenter.Common.Portable.Contracts;
using GF.UCenter.Common.Portable.Models.AppClient;

namespace Ps
{
    public class _tGuestPlayerInfo
    {
        public string account_id;
        public string account_name;
        public string pwd;
    }

    public class ClientLogin<TDef> : Component<TDef> where TDef : DefLogin, new()
    {
        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }
        public const string mGuestPlayerKey = "GuestPlayerKey";
        _tGuestPlayerInfo mGuestPlayerInfo;
        string mPassword = string.Empty;

        //-------------------------------------------------------------------------
        public override void init()
        {
            Debug.Log("ClientLogin.init()");

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();

            EntityMgr.getDefaultEventPublisher().addHandler(Entity);

            MbMain.Instance.setupMain();
            CoApp.CoDataEye.setupDataEye();

            // 显示登录界面
            UiMgr.Instance.destroyFirstUi<UiLoading>();
            UiLogin ui_login = UiMgr.Instance.createUi<UiLogin>("Login", "Login");

            //string version_info = string.Format("应用版本：{0} 数据版本：{1}", CoApp.BundleVersion, CoApp.DataVersion);
            //mb_login.setVersion(version_info);

            if (PlayerPrefs.HasKey(ClientLogin<DefLogin>.mGuestPlayerKey))
            {
                mGuestPlayerInfo = EbTool.jsonDeserialize<_tGuestPlayerInfo>(PlayerPrefs.GetString(ClientLogin<DefLogin>.mGuestPlayerKey));
            }
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            UiMgr.Instance.destroyFirstUi<UiLogin>();

            Debug.Log("ClientLogin.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            if (e is EvUiLoginClickBtnLogin)
            {
                // 请求登录
                var ev = (EvUiLoginClickBtnLogin)e;

                if (CoApp.CoUCenter.WWWLogin != null)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "正在登陆中，请稍候";
                    //f_info.color = Color.green;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
                else
                {
                    _requestLogin(ev.acc, ev.pwd);
                }
            }
            else if (e is EvUiLoginRegisterAccount)
            {
                // 请求注册帐号
                var ev = (EvUiLoginRegisterAccount)e;
                AccountRegisterInfo request = ev.register_acc_data;
                if (request.Email == null) request.Email = "";
                if (request.IdentityNum == null) request.IdentityNum = "";
                if (request.PhoneNum == null) request.PhoneNum = "";
                if (request.Name == null) request.Name = "";

                if (request.Password == request.SuperPassword)
                {
                    //UiHelper.DestroyWaiting();

                    //FloatMsgInfo f_info;
                    //f_info.msg = "注册失败，密码和超级密码不能相同";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
                else if (CoApp.CoUCenter.WWWRegister != null)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "正在注册中，请稍候";
                    //f_info.color = Color.green;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }

                if (mGuestPlayerInfo == null)
                {
                    //UiHelper.CreateWaiting();
                    mPassword = request.Password;
                    CoApp.CoUCenter.register(request, _onUCenterRegister);
                    //todo: 销毁注册界面 UiMgr.Instance.destroyCurrentUi<UiMbCreateAccount>();
                }
                else
                {
                    AccountConvertInfo guest_convertregisterdata = new AccountConvertInfo();
                    if (guest_convertregisterdata.IdentityNum == null) guest_convertregisterdata.IdentityNum = "";
                    if (guest_convertregisterdata.PhoneNum == null) guest_convertregisterdata.PhoneNum = "";
                    if (guest_convertregisterdata.Name == null) guest_convertregisterdata.Name = "";
                    guest_convertregisterdata.AccountId = mGuestPlayerInfo.account_id;
                    guest_convertregisterdata.AccountName = request.AccountName;
                    guest_convertregisterdata.Email = request.Email;
                    guest_convertregisterdata.Password = request.Password;
                    guest_convertregisterdata.OldPassword = mGuestPlayerInfo.pwd;
                    guest_convertregisterdata.SuperPassword = request.SuperPassword;
                    guest_convertregisterdata.Sex = request.Sex;
                    //UiHelper.CreateWaiting();
                    mPassword = request.Password;
                    CoApp.CoUCenter.convert(guest_convertregisterdata, _onUCenterConvert);
                    //todo: 销毁注册界面 UiMgr.Instance.destroyCurrentUi<UiMbCreateAccount>();
                }
                //UiHelper.DestroyWaiting();
            }
            else if (e is EvUiLoginClickBtnFacebook)
            {
                // 请求Facebook登录
                var ev = (EvUiLoginClickBtnFacebook)e;
            }
            else if (e is EvUiLoginClickBtnVisiter)
            {
                // 请求游客登录
                var ev = (EvUiLoginClickBtnVisiter)e;
                if (mGuestPlayerInfo == null)
                {
                    //UiHelper.CreateWaiting();
                    CoApp.CoUCenter.guest(_uCenterGuestCallBack);
                }
                else
                {
                    _requestLogin(mGuestPlayerInfo.account_name, mGuestPlayerInfo.pwd);
                }
            }
            else if (e is EvUiRequestGetPwd)
            {
                // 请求获取忘记密码
                var ev = (EvUiRequestGetPwd)e;
                string super_pwd = ev.super_pwd;
            }
            else if (e is EvUiLoginDeleteGuest)
            {
                //清除游客
                _clearGuest();
            }
        }

        //-------------------------------------------------------------------------
        void _requestLogin(string acc, string pwd)
        {
            AccountLoginInfo request = new AccountLoginInfo();
            request.AccountName = acc;
            request.Password = pwd;
            //UiHelper.CreateWaiting();
            CoApp.CoUCenter.login(request, _onUCenterLogin);
        }

        //-------------------------------------------------------------------------
        void _clearGuest()
        {
            mGuestPlayerInfo = null;
            PlayerPrefs.DeleteKey(mGuestPlayerKey);
        }

        //-------------------------------------------------------------------------
        void _onUCenterRegister(UCenterResponseStatus status, AccountRegisterResponse response, UCenterError error)
        {
            //UiHelper.DestroyWaiting();

            if (status == UCenterResponseStatus.Success)
            {
                //FloatMsgInfo f_info;
                //f_info.msg = "帐号注册成功";
                //f_info.color = Color.green;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

                CoApp.CoNetMonitor.AccId = response.AccountId;
                CoApp.CoNetMonitor.Acc = response.AccountName;

                _requestLogin(response.AccountName, mPassword);
                //ClientLoginRequest login_request = new ClientLoginRequest();
                //login_request.acc = CoApp.CoNetMonitor.Acc;
                //login_request.pwd = RegisterPwdCache;
                //EcEngine.Instance.CoUCenterSDK.login(login_request, _onUCenterLogin);
                //RegisterPwdCache = null;
            }
            else if (error != null)
            {
                if (error.ErrorCode == UCenterErrorCode.AccountRegisterFailedAlreadyExist)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "帐号注册失败，帐号已存在";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
                else
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "帐号注册失败";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
            }
        }

        //-------------------------------------------------------------------------    
        void _onUCenterConvert(UCenterResponseStatus status, AccountConvertResponse response, UCenterError error)
        {
            //UiHelper.DestroyWaiting();

            if (status == UCenterResponseStatus.Success)
            {
                //FloatMsgInfo f_info;
                //f_info.msg = "游客帐号转正成功";
                //f_info.color = Color.green;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

                CoApp.CoNetMonitor.AccId = response.AccountId;
                CoApp.CoNetMonitor.Acc = response.AccountName;

                _clearGuest();
                _requestLogin(response.AccountName, mPassword);
            }
            else if (error != null)
            {
                if (error.ErrorCode == UCenterErrorCode.AccountRegisterFailedAlreadyExist)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "游客帐号转正失败，帐号已存在";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
                else
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "游客帐号转正失败";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
            }
        }

        //-------------------------------------------------------------------------    
        void _uCenterGuestCallBack(UCenterResponseStatus status, AccountGuestLoginResponse response, UCenterError error)
        {
            if (status == UCenterResponseStatus.Success)
            {
                if (mGuestPlayerInfo == null)
                {
                    mGuestPlayerInfo = new _tGuestPlayerInfo();
                }

                mGuestPlayerInfo.account_id = response.AccountId;
                mGuestPlayerInfo.account_name = response.AccountName;
                mGuestPlayerInfo.pwd = response.Password;
                PlayerPrefs.SetString(mGuestPlayerKey, EbTool.jsonSerialize(mGuestPlayerInfo));

                _requestLogin(mGuestPlayerInfo.account_name, mGuestPlayerInfo.pwd);
            }
            else if (error != null)
            {
                //if (error.ErrorCode == GF.UCenter.Common.Portable.UCenterErrorCode.AccountRegisterFailedAlreadyExist)
                //{
                //    FloatMsgInfo f_info;
                //    f_info.msg = "帐号已存在";
                //    f_info.color = Color.red;
                //    UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                //}
                //UiHelper.DestroyWaiting();
                Debug.LogError(string.Format("_uCenterGuestCallBack::Error::ErrorCode::{0} ErrorMsg::{1}", error.ErrorCode.ToString(), error.Message));
            }
            else
            {
                //FloatMsgInfo f_info;
                //f_info.msg = "注册失败";
                //f_info.color = Color.red;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                //UiHelper.DestroyWaiting();
            }
        }

        //-------------------------------------------------------------------------
        void _onUCenterLogin(UCenterResponseStatus status, AccountLoginResponse response, UCenterError error)
        {
            EbLog.Note("ClientLogin._onUCenterLogin() Status=" + status);
            if (error != null) EbLog.Note("ErrorCode=" + error.ErrorCode + " ErrorMsg=" + error.Message);

            if (status == UCenterResponseStatus.Success)
            {
                CoApp.CoNetMonitor.AccId = response.AccountId;
                CoApp.CoNetMonitor.Acc = response.AccountName;
                CoApp.CoNetMonitor.Token = response.Token;

                //FloatMsgInfo f_info;
                //f_info.msg = "登陆成功";
                //f_info.color = Color.green;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

                // DataEye登陆
                //CoApp.CoDataEye.login(Acc, AccId);

                ClientConfig cc = MbMain.Instance.ClientConfig;
                CoApp.CoNetMonitor.connectBase(cc.BaseIp, cc.BasePort);
            }
            else if (error != null)
            {
                if (error.ErrorCode == UCenterErrorCode.AccountNotExist)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "帐号不存在";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

                    if (mGuestPlayerInfo != null)
                    {
                        //if(mGuestPlayerInfo.account_id==)
                        mGuestPlayerInfo = null;
                        PlayerPrefs.DeleteKey(mGuestPlayerKey);
                    }
                }
                else if (error.ErrorCode == UCenterErrorCode.AccountLoginFailedPasswordNotMatch)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "帐号或密码错误";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
                else if (error.ErrorCode == UCenterErrorCode.Failed)
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "登陆失败";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
                else
                {
                    //FloatMsgInfo f_info;
                    //f_info.msg = "其他登陆错误";
                    //f_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                }
            }
            //UiHelper.DestroyWaiting();
        }
    }
}

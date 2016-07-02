using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GF.Common;
using GF.UCenter.Common.Portable.Contracts;
using GF.UCenter.Common.Portable.Models.AppClient;
using GF.UCenter.Common.Portable.Models.Ip;

public delegate void OnUCenterRegister(UCenterResponseStatus status, AccountRegisterResponse response, UCenterError error);
public delegate void OnUCenterLogin(UCenterResponseStatus status, AccountLoginResponse response, UCenterError error);
public delegate void OnUCenterGuestLogin(UCenterResponseStatus status, AccountGuestLoginResponse response, UCenterError error);
public delegate void OnUCenterConvert(UCenterResponseStatus status, AccountConvertResponse response, UCenterError error);
public delegate void OnUCenterResetPassword(UCenterResponseStatus status, AccountResetPasswordResponse response, UCenterError error);
public delegate void OnUCenterUploadProfileImage(UCenterResponseStatus status, AccountUploadProfileImageResponse response, UCenterError error);
public delegate void OnGetAppConfig(UCenterResponseStatus status, AppConfigurationResponse response, UCenterError error);
public delegate void OnGetIpAddress(UCenterResponseStatus status, IPInfoResponse response, UCenterError error);

public class ClientUCenterSDK<TDef> : Component<TDef> where TDef : DefUCenterSDK, new()
{
    //-------------------------------------------------------------------------
    public string UCenterDomain { get; set; }
    public WWW WWWRegister { get; private set; }
    public WWW WWWLogin { get; private set; }
    public WWW WWWGuestLogin { get; private set; }
    public WWW WWWConvert { get; private set; }
    public WWW WWWResetPassword { get; private set; }
    public WWW WWWUploadProfileImage { get; private set; }
    public WWW WWWGetAppConfig { get; private set; }
    public WWW WWWGetIpAddress { get; private set; }
    Action<UCenterResponseStatus, AccountRegisterResponse, UCenterError> RegisterHandler { get; set; }
    Action<UCenterResponseStatus, AccountLoginResponse, UCenterError> LoginHandler { get; set; }
    Action<UCenterResponseStatus, AccountGuestLoginResponse, UCenterError> GuestLoginHandler { get; set; }
    Action<UCenterResponseStatus, AccountConvertResponse, UCenterError> ConvertHandler { get; set; }
    Action<UCenterResponseStatus, AccountResetPasswordResponse, UCenterError> ResetPasswordHandler { get; set; }
    Action<UCenterResponseStatus, AccountUploadProfileImageResponse, UCenterError> UploadProfileImageHandler { get; set; }
    Action<UCenterResponseStatus, AppConfigurationResponse, UCenterError> GetAppConfigHandler { get; set; }
    Action<UCenterResponseStatus, IPInfoResponse, UCenterError> GetIpAddressHandler { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        EbLog.Note("ClientUCenterSDK.init()");
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        EbLog.Note("ClientUCenterSDK.release()");
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        if (_checkResponse<AccountRegisterResponse>(WWWRegister, RegisterHandler))
        {
            WWWRegister = null;
            RegisterHandler = null;
        }

        if (_checkResponse<AccountLoginResponse>(WWWLogin, LoginHandler))
        {
            WWWLogin = null;
            LoginHandler = null;
        }

        if (_checkResponse<AccountGuestLoginResponse>(WWWGuestLogin, GuestLoginHandler))
        {
            WWWGuestLogin = null;
            GuestLoginHandler = null;
        }

        if (_checkResponse<AccountConvertResponse>(WWWConvert, ConvertHandler))
        {
            WWWConvert = null;
            ConvertHandler = null;
        }

        if (_checkResponse<AccountResetPasswordResponse>(WWWResetPassword, ResetPasswordHandler))
        {
            WWWResetPassword = null;
            ResetPasswordHandler = null;
        }

        if (_checkResponse<AccountUploadProfileImageResponse>(WWWUploadProfileImage, UploadProfileImageHandler))
        {
            WWWUploadProfileImage = null;
            UploadProfileImageHandler = null;
        }

        if (_checkResponse<AppConfigurationResponse>(WWWGetAppConfig, GetAppConfigHandler))
        {
            WWWGetAppConfig = null;
            GetAppConfigHandler = null;
        }

        if (_checkResponse<IPInfoResponse>(WWWGetIpAddress, GetIpAddressHandler))
        {
            WWWGetIpAddress = null;
            GetIpAddressHandler = null;
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //-------------------------------------------------------------------------
    public void register(AccountRegisterInfo request, OnUCenterRegister handler)
    {
        if (WWWRegister != null)
        {
            return;
        }

        RegisterHandler = new Action<UCenterResponseStatus, AccountRegisterResponse, UCenterError>(handler);

        string http_url = _genUrl("register");

        string param = EbTool.jsonSerialize(request);
        byte[] bytes = Encoding.UTF8.GetBytes(param);

        Dictionary<string, string> headers = _genHeader(bytes.Length);

        WWWRegister = new WWW(http_url, bytes, headers);
    }

    //-------------------------------------------------------------------------
    public void login(AccountLoginInfo request, OnUCenterLogin handler)
    {
        if (WWWLogin != null)
        {
            return;
        }

        LoginHandler = new Action<UCenterResponseStatus, AccountLoginResponse, UCenterError>(handler);

        string http_url = _genUrl("login");

        string param = EbTool.jsonSerialize(request);
        byte[] bytes = Encoding.UTF8.GetBytes(param);

        Dictionary<string, string> headers = _genHeader(bytes.Length);

        WWWLogin = new WWW(http_url, bytes, headers);
    }

    //-------------------------------------------------------------------------
    public void guest(OnUCenterGuestLogin handler)
    {
        if (WWWGuestLogin != null)
        {
            return;
        }

        GuestLoginHandler = new Action<UCenterResponseStatus, AccountGuestLoginResponse, UCenterError>(handler);

        string http_url = _genUrl("guest");

        WWWForm form = new WWWForm();
        form.AddField("Accept", "application/x-www-form-urlencoded");
        form.AddField("Content-Type", "application/json; charset=utf-8");
        form.AddField("Content-Length", 0);
        form.AddField("Host", _getHostName());
        form.AddField("User-Agent", "");

        WWWGuestLogin = new WWW(http_url, form);
    }

    //-------------------------------------------------------------------------
    public void convert(AccountConvertInfo request, OnUCenterConvert handler)
    {
        if (WWWConvert != null)
        {
            return;
        }

        ConvertHandler = new Action<UCenterResponseStatus, AccountConvertResponse, UCenterError>(handler);

        string http_url = _genUrl("convert");

        string param = EbTool.jsonSerialize(request);
        byte[] bytes = Encoding.UTF8.GetBytes(param);

        Dictionary<string, string> headers = _genHeader(bytes.Length);

        WWWConvert = new WWW(http_url, bytes, headers);
    }

    //-------------------------------------------------------------------------
    public void resetPassword(AccountResetPasswordInfo request, OnUCenterResetPassword handler)
    {
        if (WWWResetPassword != null)
        {
            return;
        }

        ResetPasswordHandler = new Action<UCenterResponseStatus, AccountResetPasswordResponse, UCenterError>(handler);

        string http_url = _genUrl("resetpassword");

        string param = EbTool.jsonSerialize(request);
        byte[] bytes = Encoding.UTF8.GetBytes(param);

        Dictionary<string, string> headers = _genHeader(bytes.Length);

        WWWResetPassword = new WWW(http_url, bytes, headers);
    }

    //-------------------------------------------------------------------------
    public void uploadProfileImage(string account_id, MemoryStream stream, OnUCenterUploadProfileImage handler)
    {
        if (WWWUploadProfileImage != null)
        {
            return;
        }

        UploadProfileImageHandler = new Action<UCenterResponseStatus, AccountUploadProfileImageResponse, UCenterError>(handler);

        string http_url = _genUrl("upload/" + account_id);

        byte[] bytes = stream.ToArray();

        Dictionary<string, string> headers = _genHeader(bytes.Length);

        WWWUploadProfileImage = new WWW(http_url, bytes, headers);
    }

    //-------------------------------------------------------------------------
    public void getAppConfig(string app_id, OnGetAppConfig handler)
    {
        if (WWWGetAppConfig != null)
        {
            return;
        }

        GetAppConfigHandler = new Action<UCenterResponseStatus, AppConfigurationResponse, UCenterError>(handler);

        string http_url = null;
        if (UCenterDomain.EndsWith("/"))
        {
            http_url = string.Format("{0}api/appclient/conf?appId={1}", UCenterDomain, app_id);
        }
        else
        {
            http_url = string.Format("{0}/api/appclient/conf?appId={1}", UCenterDomain, app_id);
        }

        WWWForm form = new WWWForm();
        form.AddField("Accept", "application/x-www-form-urlencoded");
        form.AddField("Content-Type", "application/json; charset=utf-8");
        form.AddField("Content-Length", 0);
        form.AddField("Host", _getHostName());
        form.AddField("User-Agent", "");

        WWWGetAppConfig = new WWW(http_url, form);
    }

    //-------------------------------------------------------------------------
    public void getIpAddress(OnGetIpAddress handler)
    {
        if (WWWGetIpAddress != null)
        {
            return;
        }

        GetIpAddressHandler = new Action<UCenterResponseStatus, IPInfoResponse, UCenterError>(handler);

        string http_url = null;
        if (UCenterDomain.EndsWith("/"))
        {
            http_url = string.Format("{0}api/appclient/ip", UCenterDomain);
        }
        else
        {
            http_url = string.Format("{0}/api/appclient/ip", UCenterDomain);
        }

        WWWForm form = new WWWForm();
        form.AddField("Accept", "application/x-www-form-urlencoded");
        form.AddField("Content-Type", "application/json; charset=utf-8");
        form.AddField("Content-Length", 0);
        form.AddField("Host", _getHostName());
        form.AddField("User-Agent", "");

        WWWGetIpAddress = new WWW(http_url, form);
    }

    //-------------------------------------------------------------------------
    Dictionary<string, string> _genHeader(int content_len)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers["Accept"] = "application/x-www-form-urlencoded";
        headers["Content-Type"] = "application/json; charset=utf-8";
        headers["Content-Length"] = content_len.ToString();
        headers["Host"] = _getHostName();
        headers["User-Agent"] = "";

        return headers;
    }

    //-------------------------------------------------------------------------
    string _getHostName()
    {
        string host = UCenterDomain;
        host = host.Replace("https://", "");
        host = host.Replace("http://", "");

        if (host.EndsWith("/"))
        {
            host = host.TrimEnd('/');
        }

        return host;
    }

    //-------------------------------------------------------------------------
    string _genUrl(string api)
    {
        string http_url = null;
        if (UCenterDomain.EndsWith("/"))
        {
            http_url = string.Format("{0}api/account/{1}", UCenterDomain, api);
        }
        else
        {
            http_url = string.Format("{0}/api/account/{1}", UCenterDomain, api);
        }

        return http_url;
    }

    //-------------------------------------------------------------------------
    bool _checkResponse<TResponse>(WWW www, Action<UCenterResponseStatus, TResponse, UCenterError> handler)
    {
        if (www != null)
        {
            if (www.isDone)
            {
                UCenterResponse<TResponse> response = null;

                if (string.IsNullOrEmpty(www.error))
                {
                    try
                    {
                        response = EbTool.jsonDeserialize<UCenterResponse<TResponse>>(www.text);
                    }
                    catch (Exception ex)
                    {

                        EbLog.Error("ClientUCenterSDK.update() UCenterResponse Error");
                        EbLog.Error(ex.ToString());
                    }
                }
                else
                {
                    EbLog.Error(www.url);
                    EbLog.Error(www.error);
                }

                www = null;

                if (handler != null)
                {
                    if (response != null)
                    {
                        handler(response.Status, response.Result, response.Error);
                    }
                    else
                    {
                        var error = new UCenterError();
                        error.ErrorCode = UCenterErrorCode.Failed;
                        error.Message = "";
                        handler(UCenterResponseStatus.Error, default(TResponse), error);
                    }

                    handler = null;
                }

                return true;
            }
        }

        return false;
    }
}

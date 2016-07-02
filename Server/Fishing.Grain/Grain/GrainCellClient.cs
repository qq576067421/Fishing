using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using GF.Common;
using GF.Server;
using GF.UCenter.Common;
using GF.UCenter.SDK.AppServer;

namespace Ps
{
    // Client无状态服务
    [Reentrant]
    [StatelessWorker]
    public class GrainCellClient : Grain, ICellClientService
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }
        UCenterClient CoUCenterSDK { get; set; }

        //---------------------------------------------------------------------
        public override Task OnActivateAsync()
        {
            string host = ConfigurationManager.AppSettings["UCenterDomain"];
            CoUCenterSDK = new UCenterClient(host);

            return base.OnActivateAsync();
        }

        //---------------------------------------------------------------------
        public override Task OnDeactivateAsync()
        {
            CoUCenterSDK = null;

            return base.OnDeactivateAsync();
        }

        //---------------------------------------------------------------------
        // 客户端请求
        async Task<MethodData> ICellClientService.c2sRequest(MethodData method_data)
        {
            MethodData result = new MethodData();
            result.method_id = MethodType.None;

            var account_request = EbTool.protobufDeserialize<AccountRequest>(method_data.param1);
            switch (account_request.id)
            {
                case AccountRequestId.EnterWorld:// 请求进入游戏世界
                    {
                        var enterworld_request = EbTool.protobufDeserialize<ClientEnterWorldRequest>(account_request.data);

                        string info = string.Format("客户端请求进入游戏世界\nAccId={0}, AccName={1}, Token={2}",
                            enterworld_request.acc_id, enterworld_request.acc_name, enterworld_request.token);
                        Logger.Info(info);

                        ClientEnterWorldResponse enterworld_response = new ClientEnterWorldResponse();
                        enterworld_response.result = ProtocolResult.Failed;
                        enterworld_response.acc_id = enterworld_request.acc_id;
                        enterworld_response.acc_name = enterworld_request.acc_name;
                        enterworld_response.token = enterworld_request.token;

                        AppVerifyAccountInfo app_verifyaccount_request = new AppVerifyAccountInfo();
                        app_verifyaccount_request.AppId = ConfigurationManager.AppSettings["UCenterAppId"];
                        app_verifyaccount_request.AppSecret = ConfigurationManager.AppSettings["UCenterAppSecret"];
                        app_verifyaccount_request.AccountId = enterworld_request.acc_id;
                        app_verifyaccount_request.AccountToken = enterworld_request.token;

                        // 去UCenter验证Account
                        AppVerifyAccountResponse app_verifyaccount_response = null;
                        try
                        {
                            app_verifyaccount_response = await CoUCenterSDK.AppVerifyAccountAsync(app_verifyaccount_request);
                        }
                        catch (Exception ex)
                        {
                            Logger.Info(ex.ToString());
                            enterworld_response.result = ProtocolResult.EnterWorldAccountVerifyFailed;
                            goto End;
                        }

                        // 去UCenter获取Account对应的AppData
                        AppAccountDataResponse app_data_response = null;
                        try
                        {
                            AppAccountDataInfo app_data_info = new AppAccountDataInfo();
                            app_data_info.AppId = app_verifyaccount_request.AppId;
                            app_data_info.AppSecret = app_verifyaccount_request.AppSecret;
                            app_data_info.AccountId = app_verifyaccount_request.AccountId;
                            app_data_info.Data = null;
                            app_data_response = await CoUCenterSDK.AppReadAccountDataAsync(app_data_info);
                        }
                        catch (Exception ex)
                        {
                            Logger.Info(ex.ToString());
                            enterworld_response.result = ProtocolResult.EnterWorldAccountVerifyFailed;
                            goto End;
                        }

                        // 检测Token是否过期
                        //TimeSpan ts = app_verifyaccount_response.now_dt - app_verifyaccount_response.last_login_dt;
                        //if (ts.TotalSeconds > 60)
                        //{
                        //    enterworld_response.result = ProtocolResult.EnterWorldTokenExpire;
                        //    goto End;
                        //}

                        // PlayerEtGuid
                        string new_player_etguid = "";
                        if (app_data_response != null && !string.IsNullOrEmpty(app_data_response.Data))
                        {
                            var data_read = EbTool.jsonDeserialize<Dictionary<string, string>>(app_data_response.Data);
                            new_player_etguid = data_read["player_etguid"];
                        }

                        if (string.IsNullOrEmpty(new_player_etguid))
                        {
                            // 玩家角色未创建

                            new_player_etguid = Guid.NewGuid().ToString();

                            // 去UCenter写入Account对应的AppData
                            try
                            {
                                Dictionary<string, string> data_write = new Dictionary<string, string>();
                                data_write["player_etguid"] = new_player_etguid;

                                AppAccountDataInfo app_data_info = new AppAccountDataInfo();
                                app_data_info.AppId = app_verifyaccount_request.AppId;
                                app_data_info.AppSecret = app_verifyaccount_request.AppSecret;
                                app_data_info.AccountId = app_verifyaccount_request.AccountId;
                                app_data_info.Data = EbTool.jsonSerialize(data_write);
                                await CoUCenterSDK.AppWriteAccountDataAsync(app_data_info);
                            }
                            catch (Exception ex)
                            {
                                Logger.Info(ex.ToString());
                                enterworld_response.result = ProtocolResult.EnterWorldAccountVerifyFailed;
                                goto End;
                            }
                        }

                        NewPlayerInfo new_player_info = new NewPlayerInfo();
                        new_player_info.account_id = enterworld_request.acc_id;
                        new_player_info.account_name = enterworld_request.acc_name;
                        new_player_info.gender = true;
                        new_player_info.is_bot = false;
                        new_player_info.et_player_guid = new_player_etguid;

                        // 角色进入游戏
                        ICellPlayer grain_player = GrainFactory.GetGrain<ICellPlayer>(new Guid(new_player_etguid));
                        enterworld_response.et_player_data = await grain_player.c2sEnterWorld(new_player_info);
                        enterworld_response.result = ProtocolResult.Success;

                        End:
                        Ps.AccountResponse account_response;
                        account_response.id = AccountResponseId.EnterWorld;
                        account_response.data = EbTool.protobufSerialize<ClientEnterWorldResponse>(enterworld_response);

                        result.method_id = MethodType.s2cAccountResponse;
                        result.param1 = EbTool.protobufSerialize<Ps.AccountResponse>(account_response);
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class BaseLogin<TDef> : Component<TDef> where TDef : DefLogin, new()
{
    //-------------------------------------------------------------------------
    BaseApp<DefApp> CoApp { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoApp = (BaseApp<DefApp>)Entity.getCacheData("CoApp");

        defNodeRpcMethod<AccountRequest>(
            (ushort)MethodType.c2sAccountRequest, c2sAccountRequest);
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
    async void c2sAccountRequest(AccountRequest account_request)
    {
        IRpcSession s = EntityMgr.LastRpcSession;

        var task = await Task.Factory.StartNew<Task<MethodData>>(async () =>
            {
                MethodData method_data = new MethodData();
                method_data.method_id = MethodType.c2sAccountRequest;
                method_data.param1 = EbTool.protobufSerialize<AccountRequest>(account_request);

                MethodData r = null;
                try
                {
                    var grain_client = GrainClient.GrainFactory.GetGrain<ICellClientService>(0);
                    r = await grain_client.c2sRequest(method_data);
                }
                catch (Exception ex)
                {
                    EbLog.Error(ex.ToString());
                }

                return r;
            });

        if (task.Status == TaskStatus.Faulted || task.Result == null)
        {
            if (task.Exception != null)
            {
                EbLog.Error(task.Exception.ToString());
            }

            return;
        }

        MethodData result = task.Result;
        if (result.method_id == MethodType.None)
        {
            return;
        }

        var account_response = EbTool.protobufDeserialize<AccountResponse>(result.param1);
        if (account_response.id == AccountResponseId.EnterWorld)
        {
            var enterworld_response = EbTool.protobufDeserialize<ClientEnterWorldResponse>(account_response.data);
            var client_info = CoApp.getClientInfo(s);
            if (enterworld_response != null
                && enterworld_response.et_player_data != null
                && !string.IsNullOrEmpty(enterworld_response.et_player_data.entity_guid)
                && enterworld_response.result == ProtocolResult.Success
                && client_info != null)
            {
                client_info.acc = enterworld_response.acc_name;
                client_info.et_player_guid = enterworld_response.et_player_data.entity_guid;

                var grain_player = GrainClient.GrainFactory.GetGrain<ICellPlayer>(new Guid(client_info.et_player_guid));
                client_info.player_watcher = new CellPlayerObserver(CoApp, s);
                client_info.player_watcher_weak = await GrainClient.GrainFactory.CreateObjectReference<ICellPlayerObserver>(client_info.player_watcher);
                await grain_player.subPlayer(client_info.player_watcher_weak, client_info.remote_endpoint);

                EbLog.Note("BaseLogin.EnterWorld Success! PlayerEtGuid=" + client_info.et_player_guid);
            }
        }

        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(s, (ushort)MethodType.s2cAccountResponse, account_response);
        }
    }
}

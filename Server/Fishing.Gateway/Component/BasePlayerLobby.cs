using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class BasePlayerLobby<TDef> : Component<TDef> where TDef : DefPlayerLobby, new()
{
    //-------------------------------------------------------------------------
    BaseApp<DefApp> CoApp { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoApp = (BaseApp<DefApp>)Entity.getCacheData("CoApp");

        defNodeRpcMethod<PlayerLobbyRequest>(
            (ushort)MethodType.c2sPlayerLobbyRequest, c2sPlayerLobbyRequest);
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
    async void c2sPlayerLobbyRequest(PlayerLobbyRequest lobby_request)
    {
        IRpcSession s = EntityMgr.LastRpcSession;
        ClientInfo client_info = CoApp.getClientInfo(s);
        if (client_info == null) return;

        var task = await Task.Factory.StartNew<Task<MethodData>>(async () =>
         {
             MethodData method_data = new MethodData();
             method_data.method_id = MethodType.c2sPlayerLobbyRequest;
             method_data.param1 = EbTool.protobufSerialize<PlayerLobbyRequest>(lobby_request);

             MethodData r = null;
             try
             {
                 var grain_playerproxy = GrainClient.GrainFactory.GetGrain<ICellPlayer>(new Guid(client_info.et_player_guid));
                 r = await grain_playerproxy.c2sRequest(method_data);
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

        lock (CoApp.RpcLock)
        {
            var lobby_response = EbTool.protobufDeserialize<PlayerLobbyResponse>(result.param1);
            CoApp.rpcBySession(s, (ushort)MethodType.s2cPlayerLobbyResponse, lobby_response);
        }
    }
}

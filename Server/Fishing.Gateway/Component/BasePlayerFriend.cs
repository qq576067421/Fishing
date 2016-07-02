using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class BasePlayerFriend<TDef> : Component<TDef> where TDef : DefPlayerFriend, new()
{
    //-------------------------------------------------------------------------
    BaseApp<DefApp> CoApp { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoApp = (BaseApp<DefApp>)Entity.getCacheData("CoApp");

        defNodeRpcMethod<PlayerFriendRequest>(
            (ushort)MethodType.c2sPlayerFriendRequest, c2sPlayerFriendRequest);
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
    async void c2sPlayerFriendRequest(PlayerFriendRequest playerfriend_request)
    {
        IRpcSession s = EntityMgr.LastRpcSession;
        ClientInfo client_info = CoApp.getClientInfo(s);
        if (client_info == null) return;

        var task = await Task.Factory.StartNew<Task<MethodData>>(async () =>
        {
            MethodData method_data = new MethodData();
            method_data.method_id = MethodType.c2sPlayerFriendRequest;
            method_data.param1 = EbTool.protobufSerialize<PlayerFriendRequest>(playerfriend_request);

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
            var playerfriend_response = EbTool.protobufDeserialize<PlayerFriendResponse>(result.param1);
            CoApp.rpcBySession(s, (ushort)MethodType.s2cPlayerFriendResponse, playerfriend_response);
        }
    }
}

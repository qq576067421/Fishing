using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class BasePlayerRanking<TDef> : Component<TDef> where TDef : DefPlayerRanking, new()
{
    //-------------------------------------------------------------------------
    BaseApp<DefApp> CoApp { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoApp = (BaseApp<DefApp>)Entity.getCacheData("CoApp");

        defNodeRpcMethod<PlayerRankingRequest>(
            (ushort)MethodType.c2sPlayerRankingRequest, c2sPlayerRankingRequest);
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
    async void c2sPlayerRankingRequest(PlayerRankingRequest ranking_request)
    {
        IRpcSession s = EntityMgr.LastRpcSession;
        ClientInfo client_info = CoApp.getClientInfo(s);
        if (client_info == null) return;

        var task = await Task.Factory.StartNew<Task<MethodData>>(async () =>
        {
            MethodData method_data = new MethodData();
            method_data.method_id = MethodType.c2sPlayerRankingRequest;
            method_data.param1 = EbTool.protobufSerialize<PlayerRankingRequest>(ranking_request);

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
            var ranking_response = EbTool.protobufDeserialize<PlayerRankingResponse>(result.param1);
            CoApp.rpcBySession(s, (ushort)MethodType.s2cPlayerRankingResponse, ranking_response);
        }
    }
}

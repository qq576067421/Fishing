using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class BaseEquip<TDef> : Component<TDef> where TDef : DefEquip, new()
{
    //-------------------------------------------------------------------------
    BaseApp<DefApp> CoApp { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoApp = (BaseApp<DefApp>)Entity.getCacheData("CoApp");

        defNodeRpcMethod<EquipRequest>(
            (ushort)MethodType.c2sEquipRequest, c2sEquipRequest);
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
    async void c2sEquipRequest(EquipRequest equip_request)
    {
        IRpcSession s = EntityMgr.LastRpcSession;
        ClientInfo client_info = CoApp.getClientInfo(s);
        if (client_info == null) return;

        var task = await Task.Factory.StartNew<Task<MethodData>>(async () =>
        {
            MethodData method_data = new MethodData();
            method_data.method_id = MethodType.c2sEquipRequest;
            method_data.param1 = EbTool.protobufSerialize<EquipRequest>(equip_request);

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
            var equip_response = EbTool.protobufDeserialize<EquipResponse>(result.param1);
            CoApp.rpcBySession(s, (ushort)MethodType.s2cEquipResponse, equip_response);
        }
    }
}

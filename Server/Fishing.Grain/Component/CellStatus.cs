using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

// CellStatus消息，创建状态
public class EvCellStatusCreateStatus : EntityEvent
{
    public EvCellStatusCreateStatus() : base() { }
    public Item item;
}

public class CellStatus<TDef> : Component<TDef> where TDef : DefStatus, new()
{
    //-------------------------------------------------------------------------
    Dictionary<int, Item> mMapItem = new Dictionary<int, Item>();// item_id
    Queue<int> mItemSignDestroy = new Queue<int>();

    //-------------------------------------------------------------------------
    CellActor<DefActor> CoActor { get; set; }
    Dictionary<int, Dictionary<string, IProp>> MapPropStatus { get; set; }// 所有状态临时附加属性

    //-------------------------------------------------------------------------
    public override void init()
    {
        MapPropStatus = new Dictionary<int, Dictionary<string, IProp>>();
        CoActor = Entity.getComponent<CellActor<DefActor>>();
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        mMapItem.Clear();
        mItemSignDestroy = null;
        CoActor = null;
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        bool dirty = false;

        // 更新所有状态
        foreach (var i in mMapItem)
        {
            //UnitStatus unit_status = (UnitStatus)i.Value.UnitLink;
            //unit_status.update(elapsed_tm);
            //if (unit_status.Dirty)
            //{
            //    dirty = true;
            //    MapPropStatus[i.Key] = unit_status.MapProp;
            //}

            //if (unit_status.Finished)
            //{
            //    unit_status.destroy();
            //    mItemSignDestroy.Enqueue(i.Key);
            //    dirty = true;
            //}
        }

        // 删除已经标记为销毁的Status
        if (mItemSignDestroy.Count > 0)
        {
            foreach (var i in mItemSignDestroy)
            {
                MapPropStatus.Remove(i);
                mMapItem.Remove(i);
            }
        }

        if (dirty)
        {
            // 更新角色最终属性
            //CoActor.calcPropFinal();
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //-------------------------------------------------------------------------
    public Task<MethodData> c2sStatusRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var status_request = EbTool.protobufDeserialize<StatusRequest>(method_data.param1);
        switch (status_request.id)
        {
            case StatusRequestId.SetupStatus:// 请求获取背包初始化信息
                {
                    List<ItemData> list_item = getStatusInitData();

                    StatusResponse status_response;
                    status_response.id = StatusResponseId.SetupStatus;
                    status_response.data = EbTool.protobufSerialize<List<ItemData>>(list_item);

                    result.method_id = MethodType.s2cStatusResponse;
                    result.param1 = EbTool.protobufSerialize<StatusResponse>(status_response);
                }
                break;
            default:
                break;
        }

        return Task.FromResult(result);
    }

    //-------------------------------------------------------------------------
    public void s2cStatusNotifyCreateStatus(ItemData item_data)
    {
        StatusNotify status_notify;
        status_notify.id = StatusNotifyId.CreateStatus;
        status_notify.data = EbTool.protobufSerialize<ItemData>(item_data);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cStatusNotify;
        notify_data.param1 = EbTool.protobufSerialize<StatusNotify>(status_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    // 创建Status
    public void createStatus(Item item)
    {
        mMapItem[item.TbDataItem.Id] = item;

        // 广播创建状态Entity内部消息
        var ev = Publisher.genEvent<EvCellStatusCreateStatus>();
        ev.item = item;
        ev.send(this);
    }

    //-------------------------------------------------------------------------
    public List<ItemData> getStatusInitData()
    {
        // 状态背包中所有状态
        List<ItemData> list_itemdata = new List<ItemData>();
        foreach (var i in mMapItem)
        {
            list_itemdata.Add(i.Value.ItemData);
        }
        return list_itemdata;
    }
}

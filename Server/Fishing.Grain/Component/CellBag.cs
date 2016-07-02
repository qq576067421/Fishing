using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

// CellBag消息，添加道具
public class EvCellBagAddItem : EntityEvent
{
    public EvCellBagAddItem() : base() { }
    public Item item;
}

// CellBag消息，更新道具
public class EvCellBagUpdateItem : EntityEvent
{
    public EvCellBagUpdateItem() : base() { }
    public Item item;
}

// CellBag消息，删除道具
public class EvCellBagDeleteItem : EntityEvent
{
    public EvCellBagDeleteItem() : base() { }
    public string item_objid;
}

public class CellBag<TDef> : Component<TDef> where TDef : DefBag, new()
{
    //-------------------------------------------------------------------------
    Dictionary<string, Item> mMapItem = new Dictionary<string, Item>();// item_objid

    //-------------------------------------------------------------------------
    CellActor<DefActor> CoActor { get; set; }
    public Dictionary<string, Item> MapItem { get { return mMapItem; } }

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoActor = Entity.getComponent<CellActor<DefActor>>();

        // 初始化背包中所有Item
        Dictionary<string, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
        foreach (var i in map_itemdata4db)
        {
            if (i.Value.n > 0)
            {
                Item item = new Item(Entity, i.Value);
                mMapItem[item.ItemData.item_objid] = item;
            }
        }
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        mMapItem.Clear();
        CoActor = null;
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
    public Task<MethodData> c2sBagRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var bag_request = EbTool.protobufDeserialize<BagRequest>(method_data.param1);
        switch (bag_request.id)
        {
            case BagRequestId.SetupBag:// 请求获取背包初始化信息
                {
                    List<ItemData> list_item = getAllItem();

                    BagResponse bag_response;
                    bag_response.id = BagResponseId.SetupBag;
                    bag_response.data = EbTool.protobufSerialize<List<ItemData>>(list_item);

                    result.method_id = MethodType.s2cBagResponse;
                    result.param1 = EbTool.protobufSerialize<BagResponse>(bag_response);
                }
                break;
            case BagRequestId.OperateItem:// 请求使用道具
                {
                    var item_operate = EbTool.protobufDeserialize<ItemOperate>(bag_request.data);

                    var item_operate_response_data = operateItem(item_operate);

                    BagResponse bag_response;
                    bag_response.id = BagResponseId.OperateItem;
                    bag_response.data = EbTool.protobufSerialize<ItemOperateResponseData>(item_operate_response_data);

                    result.method_id = MethodType.s2cBagResponse;
                    result.param1 = EbTool.protobufSerialize<BagResponse>(bag_response);
                }
                break;
            default:
                break;
        }

        return Task.FromResult(result);
    }

    //-------------------------------------------------------------------------
    public void s2cBagNotifyAddItem(ItemData item_data)
    {
        BagNotify bag_notify;
        bag_notify.id = BagNotifyId.AddItem;
        bag_notify.data = EbTool.protobufSerialize<ItemData>(item_data);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cBagNotify;
        notify_data.param1 = EbTool.protobufSerialize<BagNotify>(bag_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    public void s2cBagNotifyDeleteItem(string item_objid)
    {
        BagNotify bag_notify;
        bag_notify.id = BagNotifyId.DeleteItem;
        bag_notify.data = EbTool.protobufSerialize<string>(item_objid);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cBagNotify;
        notify_data.param1 = EbTool.protobufSerialize<BagNotify>(bag_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    public void s2cBagNotifyUpdateItem(ItemData item_data)
    {
        BagNotify bag_notify;
        bag_notify.id = BagNotifyId.UpdateItem;
        bag_notify.data = EbTool.protobufSerialize<ItemData>(item_data);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cBagNotify;
        notify_data.param1 = EbTool.protobufSerialize<BagNotify>(bag_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    public ItemOperateResponseData operateItem(ItemOperate item_operate)
    {
        // 执行使用道具操作
        ItemOperateResponseData result = new ItemOperateResponseData();
        result.result = ProtocolResult.Failed;
        result.operate_id = item_operate.operate_id;
        result.item_objid = item_operate.item_objid;

        Item item = null;
        mMapItem.TryGetValue(item_operate.item_objid, out item);
        if (item == null)
        {
            return result;
        }
        item.operate(item_operate.operate_id);

        // 检测是否需要删除道具
        if (item.ItemData.n <= 0)
        {
            mMapItem.Remove(item_operate.item_objid);
            Dictionary<string, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
            map_itemdata4db.Remove(item_operate.item_objid);
        }

        result.result = ProtocolResult.Success;
        return result;
    }

    //-------------------------------------------------------------------------
    public void updateItem(Item item, bool notify_client = true)
    {
        // 更新道具Db存储容器
        Dictionary<string, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
        map_itemdata4db[item.ItemData.item_objid] = item.ItemData;

        // 广播添加道具Entity内部消息
        if (notify_client)
        {
            var ev = Publisher.genEvent<EvCellBagUpdateItem>();
            ev.item = item;
            ev.send(this);
        }
    }

    //-------------------------------------------------------------------------
    public ProtocolResult newItem(int item_id, int count, out Item item)
    {
        item = null;

        int need_slot_count = calcNeedSlotCount(item_id, count);
        if (mMapItem.Count + need_slot_count > Def.mPropSlotOpenCount.get())
        {
            return ProtocolResult.BagFull;
        }

        if (need_slot_count == 1)
        {
            item = new Item(Entity, item_id, (byte)count);
            addItem(item);
        }
        else
        {
            var tbdata_item = EbDataMgr.Instance.getData<TbDataItem>(item_id);

            int index = need_slot_count - 1;
            while (index > 0)
            {
                index--;
                item = new Item(Entity, item_id, (byte)tbdata_item.MaxOverlapNum);
                addItem(item);
            }

            item = new Item(Entity, item_id, (byte)(count % tbdata_item.MaxOverlapNum));
            addItem(item);
        }

        return ProtocolResult.Success;
    }

    //-------------------------------------------------------------------------
    public ProtocolResult newItem(ItemData item_data, out Item item)
    {
        item = null;

        int need_slot_count = calcNeedSlotCount(item_data.item_id, item_data.n);
        if (mMapItem.Count + need_slot_count > Def.mPropSlotOpenCount.get())
        {
            return ProtocolResult.BagFull;
        }

        item = new Item(Entity, item_data);
        addItem(item);

        return ProtocolResult.Success;
    }

    //-------------------------------------------------------------------------
    public ProtocolResult addItem(Item item, bool notify_client = true)
    {
        if (mMapItem.Count >= Def.mPropSlotOpenCount.get())
        {
            return ProtocolResult.BagFull;
        }

        // 添加道具
        mMapItem[item.ItemData.item_objid] = item;

        // 更新道具Db存储容器
        Dictionary<string, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
        map_itemdata4db[item.ItemData.item_objid] = item.ItemData;

        // 广播添加道具Entity内部消息
        if (notify_client)
        {
            var ev = Publisher.genEvent<EvCellBagAddItem>();
            ev.item = item;
            ev.send(this);
        }

        return ProtocolResult.Success;
    }

    //-------------------------------------------------------------------------
    public void deleteItem(string item_objid)
    {
        // 执行删除道具操作
        mMapItem.Remove(item_objid);

        // 更新道具Db存储容器
        Dictionary<string, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
        map_itemdata4db.Remove(item_objid);

        // 广播删除道具Entity内部消息
        var ev = Publisher.genEvent<EvCellBagDeleteItem>();
        ev.item_objid = item_objid;
        ev.send(this);
    }

    //-------------------------------------------------------------------------
    public void deleteItem(int item_id, int count)
    {
        List<string> list_itemobjid_delete = new List<string>();

        int count_left = count;
        foreach (var i in mMapItem)
        {
            if (count_left == 0) break;
            if (i.Value.TbDataItem.Id == item_id)
            {
                if (i.Value.TbDataItem.MaxOverlapNum == 1)
                {
                    count_left--;
                    list_itemobjid_delete.Add(i.Key);
                }
                else
                {
                    if (count_left >= i.Value.ItemData.n)
                    {
                        list_itemobjid_delete.Add(i.Key);
                        count_left -= i.Value.ItemData.n;
                    }
                    else
                    {
                        count_left = 0;
                        i.Value.ItemData.n -= (byte)count_left;
                    }
                }
            }
        }

        foreach (var i in list_itemobjid_delete)
        {
            mMapItem.Remove(i);
        }
    }

    //-------------------------------------------------------------------------
    // 道具数量减1
    public void subItemCount(string item_objid, int item_num)
    {
        Item item = null;
        mMapItem.TryGetValue(item_objid, out item);
        if (item == null) return;

        if (item.TbDataItem.MaxOverlapNum == 1)
        {
            deleteItem(item_objid);
        }
        else
        {
            item.ItemData.n -= (byte)item_num;
            if ((int)item.ItemData.n <= 0)
            {
                deleteItem(item_objid);
            }
            else
            {
                updateItem(item);
            }
        }
    }

    //-------------------------------------------------------------------------
    public Item getItem(string item_objid)
    {
        Item item = null;
        mMapItem.TryGetValue(item_objid, out item);
        return item;
    }

    //-------------------------------------------------------------------------
    public Item getThenRemoveItem(string item_objid)
    {
        Item item = null;
        mMapItem.TryGetValue(item_objid, out item);
        if (item == null) return item;

        mMapItem.Remove(item_objid);

        // 更新道具Db存储容器
        Dictionary<string, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
        map_itemdata4db.Remove(item_objid);

        return item;
    }

    //-------------------------------------------------------------------------
    public List<ItemData> getAllItem()
    {
        List<ItemData> list_itemdata = new List<ItemData>();
        foreach (var i in mMapItem)
        {
            list_itemdata.Add(i.Value.ItemData);
        }

        return list_itemdata;
    }

    //-------------------------------------------------------------------------
    public int getItemNumByItemId(int item_id)
    {
        int count = 0;
        foreach (var i in mMapItem)
        {
            if (i.Value.TbDataItem.Id == item_id)
            {
                if (i.Value.TbDataItem.MaxOverlapNum == 1)
                {
                    count++;
                }
                else
                {
                    count += i.Value.ItemData.n;
                }
            }
        }
        return count;
    }

    //-------------------------------------------------------------------------
    public bool isFull()
    {
        return (mMapItem.Count >= Def.mPropSlotOpenCount.get());
    }

    //-------------------------------------------------------------------------
    public short leftOpenSlotCount()
    {
        return (short)(Def.mPropSlotOpenCount.get() - mMapItem.Count);
    }

    //-------------------------------------------------------------------------
    // 计算创建指定数量指定ItemId的物品所占的背包格子数
    public int calcNeedSlotCount(int item_id, int item_count)
    {
        int slot_count = 0;
        var tbdata_item = EbDataMgr.Instance.getData<TbDataItem>(item_id);
        if (tbdata_item == null) return slot_count;

        if (tbdata_item.MaxOverlapNum == 1)
        {
            slot_count = item_count;
        }
        else
        {
            int a = item_count / tbdata_item.MaxOverlapNum;
            int b = item_count % tbdata_item.MaxOverlapNum;
            slot_count = a;
            if (b > 0) slot_count++;
        }

        return slot_count;
    }
}

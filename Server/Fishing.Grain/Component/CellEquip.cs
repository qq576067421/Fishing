using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

// CellEquip消息，穿装备
public class EvCellEquipTakeonEquip : EntityEvent
{
    public EvCellEquipTakeonEquip() : base() { }
    public EquipSlot equip_slot;
    public Item item;
}

// CellEquip消息，脱装备
public class EvCellEquipTakeoffEquip : EntityEvent
{
    public EvCellEquipTakeoffEquip() : base() { }
    public EquipSlot equip_slot;
}

// CellEquip消息，更新装备
public class EvCellEquipUpdateEquip : EntityEvent
{
    public EvCellEquipUpdateEquip() : base() { }
}

public class CellEquip<TDef> : Component<TDef> where TDef : DefEquip, new()
{
    //-------------------------------------------------------------------------
    Dictionary<EquipSlot, Item> mMapItem = new Dictionary<EquipSlot, Item>();

    //-------------------------------------------------------------------------
    CellActor<DefActor> CoActor { get; set; }
    List<Dictionary<string, IProp>> ListPropEquip { get; set; }// 装备附加属性

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoActor = Entity.getComponent<CellActor<DefActor>>();
        ListPropEquip = new List<Dictionary<string, IProp>>();
        //CoActor.Def.ListPropEquip = ListPropEquip;

        // 初始化装备栏中所有Item
        Dictionary<byte, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
        foreach (var i in map_itemdata4db)
        {
            //Item item = new Item(Entity, i.Value);
            //UnitEquip equip = (UnitEquip)item.UnitLink;
            //equip.active();

            //EquipSlot equip_slot = Def.getEquipSlot(item);
            //mMapItem[equip_slot] = item;
        }

        _calcPropEquip();
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
    public Task<MethodData> c2sEquipRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var equip_request = EbTool.protobufDeserialize<EquipRequest>(method_data.param1);
        switch (equip_request.id)
        {
            case EquipRequestId.SetupEquip:// 请求初始化装备
                {
                    List<ItemData> list_item = getAllItem();

                    EquipResponse equip_response;
                    equip_response.id = EquipResponseId.SetupEquip;
                    equip_response.data = EbTool.protobufSerialize<List<ItemData>>(list_item);

                    result.method_id = MethodType.s2cEquipResponse;
                    result.param1 = EbTool.protobufSerialize<EquipResponse>(equip_response);
                }
                break;
            case EquipRequestId.TakeoffEquip:// 请求脱装备
                {
                    var equip_slot = EbTool.protobufDeserialize<EquipSlot>(equip_request.data);

                    takeoffEquip(equip_slot);
                }
                break;
            default:
                break;
        }

        return Task.FromResult(result);
    }

    //-------------------------------------------------------------------------
    public void takeonEquip(Item item)
    {
        EquipSlot equip_slot = Def.getEquipSlot(item);
        Item item_old = null;
        mMapItem.TryGetValue(equip_slot, out item_old);

        //UnitEquip equip = (UnitEquip)item.UnitLink;
        //equip.active();
        //mMapItem[equip_slot] = item;

        //// 更新装备Db存储容器
        //Dictionary<byte, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
        //map_itemdata4db[(byte)equip_slot] = item.ItemData;

        //// 角色背包处理
        //var co_bag = Entity.getComponent<CellBag<DefBag>>();
        //co_bag.deleteItem(item.ItemData.item_objid);
        //if (item_old != null) co_bag.addItem(item_old);

        //_calcPropEquip();

        //// 广播穿装备Entity内部消息
        //{
        //    var ev = Publisher.genEvent<EvCellEquipTakeonEquip>();
        //    ev.equip_slot = equip_slot;
        //    ev.item = item;
        //    ev.send(this);
        //}
    }

    //-------------------------------------------------------------------------
    public Item takeoffEquip(EquipSlot equip_slot)
    {
        var co_bag = Entity.getComponent<CellBag<DefBag>>();
        if (co_bag.isFull())
        {
            return null;
        }

        Item item = null;
        mMapItem.TryGetValue(equip_slot, out item);
        if (item != null)
        {
            mMapItem.Remove(equip_slot);

            // 更新道具Db存储容器
            Dictionary<byte, ItemData> map_itemdata4db = Def.mPropMapItemData4Db.get();
            map_itemdata4db.Remove((byte)equip_slot);

            // 添加到角色背包中
            co_bag.addItem(item);

            _calcPropEquip();

            // 广播脱装备Entity内部消息
            {
                var ev = Publisher.genEvent<EvCellEquipTakeoffEquip>();
                ev.equip_slot = equip_slot;
                ev.send(this);
            }
        }

        return item;
    }

    //-------------------------------------------------------------------------
    public void s2cEquipNotifyTakeon(ItemData item_data)
    {
        EquipNotify equip_notify;
        equip_notify.id = EquipNotifyId.TakeonEquip;
        equip_notify.data = EbTool.protobufSerialize<ItemData>(item_data);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cEquipNotify;
        notify_data.param1 = EbTool.protobufSerialize<EquipNotify>(equip_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    public void s2cEquipNotifyTakeoff(EquipSlot equip_slot)
    {
        EquipNotify equip_notify;
        equip_notify.id = EquipNotifyId.TakeoffEquip;
        equip_notify.data = EbTool.protobufSerialize<EquipSlot>(equip_slot);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cEquipNotify;
        notify_data.param1 = EbTool.protobufSerialize<EquipNotify>(equip_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    public List<ItemData> getAllItem()
    {
        List<ItemData> list_itemdata = new List<ItemData>();
        foreach (var i in mMapItem)
        {
            ItemData item_data = i.Value.ItemData;
            list_itemdata.Add(item_data);
        }

        return list_itemdata;
    }

    //-------------------------------------------------------------------------
    public int getWeaponItemId()
    {
        Item item = null;
        mMapItem.TryGetValue(EquipSlot.Weapon, out item);
        if (item == null) return 0;
        else return item.TbDataItem.Id;
    }

    //-------------------------------------------------------------------------
    public void _calcPropEquip()
    {
        ListPropEquip.Clear();
        //foreach (var i in mMapItem)
        //{
        //    var equip = (UnitEquip)i.Value.UnitLink;
        //    ListPropEquip.Add(equip.MapProp);
        //}
        //CoActor.Def.ListPropEquip = ListPropEquip;

        var ev = Publisher.genEvent<EvCellEquipUpdateEquip>();
        ev.send(this);
    }
}

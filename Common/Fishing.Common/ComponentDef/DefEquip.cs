using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum EquipRequestId : byte
    {
        None = 0,// 无效
        SetupEquip = 10,// c->s, 请求初始化装备
        TakeoffEquip = 20,// c->s, // 请求脱装备
    }

    //-------------------------------------------------------------------------
    public enum EquipResponseId : byte
    {
        None = 0,// 无效
        SetupEquip = 10,// s->c, 响应初始化装备
    }

    //-------------------------------------------------------------------------
    public enum EquipNotifyId : byte
    {
        None = 0,// 无效
        TakeoffEquip = 10,// s->c, 通知脱下装备
        TakeonEquip = 20,// s->c, 通知穿上装备
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct EquipRequest
    {
        [ProtoMember(1)]
        public EquipRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct EquipResponse
    {
        [ProtoMember(1)]
        public EquipResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct EquipNotify
    {
        [ProtoMember(1)]
        public EquipNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    // 装备插槽Id
    public enum EquipSlot : byte
    {
        Helmet = 2,// 头盔        
        Breastplate,// 胸甲
        Gloves,// 手套
        Shoes,// 鞋子
        Ring,//	戒指
        Necklace = 9,// 项链
        Weapon = 10,// 主手武器                           
    }

    // 装备
    public class DefEquip : ComponentDef
    {
        //-------------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<Dictionary<byte, ItemData>> mPropMapItemData4Db;// key=slot_id, value=ItemData4Db
        const int mArmorsItemTypeId = 1;
        const int mWeaponsItemTypeId = 10;

        //-------------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropMapItemData4Db = defProp<Dictionary<byte, ItemData>>(map_param, "MapItemData", new Dictionary<byte, ItemData>());
        }

        //-------------------------------------------------------------------------
        public EquipSlot getEquipSlot(Item item)
        {
            EquipSlot equip_slot = EquipSlot.Weapon;
            TbDataItemType item_type = EbDataMgr.Instance.getData<TbDataItemType>(item.TbDataItem.ItemTypeId);

            if (item_type.ParentId == mWeaponsItemTypeId)
            {
                equip_slot = (EquipSlot)item_type.ParentId;
            }
            else
            {
                equip_slot = (EquipSlot)item.TbDataItem.ItemTypeId;
            }

            return equip_slot;
        }
    }
}

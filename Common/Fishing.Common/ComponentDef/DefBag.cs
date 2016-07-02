using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum BagRequestId : byte
    {
        None = 0,// 无效
        SetupBag = 10,// c->s, 请求初始化背包
        OperateItem = 20,// c->s, 请求使用道具
    }

    //-------------------------------------------------------------------------
    public enum BagResponseId : byte
    {
        None = 0,// 无效
        SetupBag = 10,// s->c, 响应初始化背包
        OperateItem = 20,// s->c, 响应使用道具
    }

    //-------------------------------------------------------------------------
    public enum BagNotifyId : byte
    {
        None = 0,// 无效
        AddItem = 10,// s->c, 通知添加道具
        DeleteItem = 20,// s->c, 通知删除道具
        UpdateItem = 30,// s->c，通知更新道具
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct BagRequest
    {
        [ProtoMember(1)]
        public BagRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct BagResponse
    {
        [ProtoMember(1)]
        public BagResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct BagNotify
    {
        [ProtoMember(1)]
        public BagNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class ItemOperate
    {
        [ProtoMember(1)]
        public string operate_id;
        [ProtoMember(2)]
        public string item_objid;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class ItemOperateResponseData
    {
        [ProtoMember(1)]
        public ProtocolResult result;
        [ProtoMember(2)]
        public string operate_id;
        [ProtoMember(3)]
        public string item_objid;
        [ProtoMember(4)]
        public byte overlap_num;
    }

    // 背包
    public class DefBag : ComponentDef
    {
        //---------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<Dictionary<string, ItemData>> mPropMapItemData4Db;// key=item_objid, value=ItemData4Db

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<ushort> mPropSlotCount;

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<ushort> mPropSlotOpenCount;

        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropMapItemData4Db = defProp<Dictionary<string, ItemData>>(map_param, "MapItemData", new Dictionary<string, ItemData>());
            mPropSlotCount = defProp<ushort>(map_param, "SlotCount", 100);
            mPropSlotOpenCount = defProp<ushort>(map_param, "SlotOpenCount", 100);
        }
    }
}

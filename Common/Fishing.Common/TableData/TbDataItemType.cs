using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    //public class TypeInfo
    //{
    //    public int id = 0;
    //    public int parent_id = 0;
    //    public string type_name = "";
    //    public string type_desc = "";
    //    public Dictionary<int, TypeInfo> children;
    //}

    //    static Dictionary<int, TypeInfo> mMapTypeInfoTree = new Dictionary<int, TypeInfo>();
    //    static Dictionary<int, TypeInfo> mMapTypeInfoAll = new Dictionary<int, TypeInfo>();

    public class TbDataItemType : EbData
    {
        //-------------------------------------------------------------------------    
        public int ParentId { get; private set; }
        public string TypeName { get; private set; }
        public string TypeDesc { get; private set; }
        public List<ItemOperateInfo> ListOperateInfo { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            ParentId = prop_set.getPropInt("I_ParentId").get();
            TypeName = prop_set.getPropString("T_TypeName").get();
            TypeDesc = prop_set.getPropString("T_TypeDesc").get();

            ListOperateInfo = new List<ItemOperateInfo>();

            int operate_id = prop_set.getPropInt("I_Operate1").get();
            if (operate_id > 0)
            {
                ItemOperateInfo operate_info = new ItemOperateInfo();
                operate_info.OperateId = ((_eOperateType)operate_id).ToString();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(operate_id);
                operate_info.OperateName = operate_type.OperateName;
                operate_info.IsCompoundType = operate_type.IsCompandType;
                operate_info.EffectData = new EffectData();
                operate_info.EffectData.EffectId = operate_type.OperateEffectId;
                ListOperateInfo.Add(operate_info);
            }

            operate_id = prop_set.getPropInt("I_Operate2").get();
            if (operate_id > 0)
            {
                ItemOperateInfo operate_info = new ItemOperateInfo();
                operate_info.OperateId = ((_eOperateType)operate_id).ToString();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(operate_id);
                operate_info.OperateName = operate_type.OperateName;
                operate_info.IsCompoundType = operate_type.IsCompandType;
                operate_info.EffectData = new EffectData();
                operate_info.EffectData.EffectId = operate_type.OperateEffectId;
                ListOperateInfo.Add(operate_info);
            }

            operate_id = prop_set.getPropInt("I_Operate3").get();
            if (operate_id > 0)
            {
                ItemOperateInfo operate_info = new ItemOperateInfo();
                operate_info.OperateId = ((_eOperateType)operate_id).ToString();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(operate_id);
                operate_info.OperateName = operate_type.OperateName;
                operate_info.IsCompoundType = operate_type.IsCompandType;
                operate_info.EffectData = new EffectData();
                operate_info.EffectData.EffectId = operate_type.OperateEffectId;
                ListOperateInfo.Add(operate_info);
            }

            operate_id = prop_set.getPropInt("I_Operate4").get();
            if (operate_id > 0)
            {
                ItemOperateInfo operate_info = new ItemOperateInfo();
                operate_info.OperateId = ((_eOperateType)operate_id).ToString();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(operate_id);
                operate_info.OperateName = operate_type.OperateName;
                operate_info.IsCompoundType = operate_type.IsCompandType;
                operate_info.EffectData = new EffectData();
                operate_info.EffectData.EffectId = operate_type.OperateEffectId;
                ListOperateInfo.Add(operate_info);
            }

            operate_id = prop_set.getPropInt("I_Operate5").get();
            if (operate_id > 0)
            {
                ItemOperateInfo operate_info = new ItemOperateInfo();
                operate_info.OperateId = ((_eOperateType)operate_id).ToString();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(operate_id);
                operate_info.OperateName = operate_type.OperateName;
                operate_info.IsCompoundType = operate_type.IsCompandType;
                operate_info.EffectData = new EffectData();
                operate_info.EffectData.EffectId = operate_type.OperateEffectId;
                ListOperateInfo.Add(operate_info);
            }

            operate_id = prop_set.getPropInt("I_Operate6").get();
            if (operate_id > 0)
            {
                ItemOperateInfo operate_info = new ItemOperateInfo();
                operate_info.OperateId = ((_eOperateType)operate_id).ToString();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(operate_id);
                operate_info.OperateName = operate_type.OperateName;
                operate_info.IsCompoundType = operate_type.IsCompandType;
                operate_info.EffectData = new EffectData();
                operate_info.EffectData.EffectId = operate_type.OperateEffectId;
                ListOperateInfo.Add(operate_info);
            }

            operate_id = prop_set.getPropInt("I_Operate7").get();
            if (operate_id > 0)
            {
                ItemOperateInfo operate_info = new ItemOperateInfo();
                operate_info.OperateId = ((_eOperateType)operate_id).ToString();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(operate_id);
                operate_info.OperateName = operate_type.OperateName;
                operate_info.IsCompoundType = operate_type.IsCompandType;
                operate_info.EffectData = new EffectData();
                operate_info.EffectData.EffectId = operate_type.OperateEffectId;
                ListOperateInfo.Add(operate_info);
            }
        }

        //-------------------------------------------------------------------------
        public Dictionary<int, TbDataItemType> getChildItemData()
        {
            Dictionary<int, TbDataItemType> map_child = new Dictionary<int, TbDataItemType>();
            foreach (var i in EbDataMgr.Instance.getMapData<TbDataItemType>())
            {
                TbDataItemType item_type = (TbDataItemType)i.Value;
                if (item_type.ParentId == Id)
                {
                    map_child.Add(item_type.Id, item_type);
                }
            }

            return map_child;
        }

        //-------------------------------------------------------------------------
        public Dictionary<int, TbDataItem> getCurrentTypeItems()
        {
            Dictionary<int, TbDataItem> map_items = new Dictionary<int, TbDataItem>();
            foreach (var i in EbDataMgr.Instance.getMapData<TbDataItem>())
            {
                TbDataItem item = (TbDataItem)i.Value;
                if (item.ItemTypeId == Id)
                {
                    map_items.Add(item.Id, item);
                }
            }

            return map_items;
        }

        //-------------------------------------------------------------------------
        public Dictionary<int, TbDataItem> getCurrentTypeCanCompand()
        {
            Dictionary<int, TbDataItem> map_items = new Dictionary<int, TbDataItem>();
            return map_items;
        }

        //-------------------------------------------------------------------------
        public Dictionary<int, TbDataItem> getCurrentTypeCanDeCompose()
        {
            Dictionary<int, TbDataItem> map_items = new Dictionary<int, TbDataItem>();
            return map_items;
        }

        //-------------------------------------------------------------------------
        //装备可镶嵌宝石
        public Dictionary<int, TbDataItem> getCurrentTypeCanInsetGem()
        {
            Dictionary<int, TbDataItem> map_items = new Dictionary<int, TbDataItem>();
            return map_items;
        }
    }
}

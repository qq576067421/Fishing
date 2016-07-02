using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    [Serializable]
    public class ItemOperateInfo
    {
        public string OperateId { get; set; }
        public string OperateName { get; set; }
        public bool IsCompoundType { get; set; }
        public EffectData EffectData { get; set; }
        public int SubOverlapNum { get; set; }
        public float CdMax { get; set; }
    }

    public class TbDataItem : EbData
    {
        //-------------------------------------------------------------------------
        public int ItemTypeId { get; private set; }
        public string UnitType { get; private set; }
        public string Name { get; private set; }
        public int MaxOverlapNum { get; private set; }
        public string Icon { get; private set; }
        public string Desc { get; private set; }
        public ItemOperateInfo MainOperateInfo { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            ItemTypeId = prop_set.getPropInt("I_ItemTypeId").get();
            Name = prop_set.getPropString("T_Name").get();
            MaxOverlapNum = prop_set.getPropInt("I_MaxOverlapNum").get();
            Icon = prop_set.getPropString("T_Icon").get();
            Desc = prop_set.getPropString("T_Desc").get();
            UnitType = prop_set.getPropString("T_UnitType").get();

            int prop_int = prop_set.getPropInt("I_OperateId").get();
            if (prop_int > 0)
            {
                MainOperateInfo = new ItemOperateInfo();
                MainOperateInfo.OperateId = ((_eOperateType)prop_int).ToString();
                MainOperateInfo.EffectData = new EffectData();
                TbDataOperateType operate_type = EbDataMgr.Instance.getData<TbDataOperateType>(prop_int);
                MainOperateInfo.EffectData.EffectId = operate_type.OperateEffectId;
                MainOperateInfo.OperateName = operate_type.OperateName;
                MainOperateInfo.SubOverlapNum = prop_set.getPropInt("I_SubOverlapNum").get();
                MainOperateInfo.CdMax = prop_set.getPropFloat("R_CdMax").get();
            }
        }

        //-------------------------------------------------------------------------
        public ToolTip getToolTip(_eUiItemParent item_from)
        {
            ToolTip tool_tip = new ToolTip();

            switch (UnitType)
            {
                case "Consumables":
                    ConsumToolTipHead consum_tooltip_head = new ConsumToolTipHead();
                    consum_tooltip_head.ItemIco = Icon;
                    consum_tooltip_head.ItemName = Name;
                    consum_tooltip_head.ItemTypeId = ItemTypeId;
                    tool_tip.ToolTipHead = consum_tooltip_head;

                    ConsumToolTipDetail consum_tooltip_detail = new ConsumToolTipDetail();
                    TbDataUnitConsumables consumable = EbDataMgr.Instance.getData<TbDataUnitConsumables>(Id);
                    List<_ToolTipContentDetailInfo> list_consum_detail = ToolTipHelper.Instant.getToolTipNormalContentDetailText(consumable.ListEffect);
                    consum_tooltip_detail.list_detail = list_consum_detail;
                    consum_tooltip_detail.MadeBy = "";
                    tool_tip.ToolTipDetail = consum_tooltip_detail;
                    break;
            }
            tool_tip.ToolTipEnd = _getToolTipEnd(item_from);

            return tool_tip;
        }

        //-------------------------------------------------------------------------
        ToolTipEnd _getToolTipEnd(_eUiItemParent item_from)
        {
            ItemOperateInfo main_operate = null;
            List<ItemOperateInfo> equip_list_operate_info = new List<ItemOperateInfo>();
            if (item_from == _eUiItemParent.Help)
            {
                main_operate = new ItemOperateInfo();
                main_operate.OperateId = "Compound";
                main_operate.OperateName = "合成";
                main_operate.IsCompoundType = true;
                main_operate.EffectData = new EffectData();
                main_operate.EffectData.EffectId = 510;

                ItemOperateInfo operate1 = new ItemOperateInfo();
                operate1.OperateId = "Market";
                operate1.OperateName = "市场";
                operate1.IsCompoundType = false;
                operate1.EffectData = new EffectData();
                operate1.EffectData.EffectId = 0;
                equip_list_operate_info.Add(operate1);
                ItemOperateInfo operate2 = new ItemOperateInfo();
                operate2.OperateId = "Drop";
                operate2.OperateName = "掉落";
                operate2.IsCompoundType = false;
                operate2.EffectData = new EffectData();
                operate2.EffectData.EffectId = 0;
                equip_list_operate_info.Add(operate2);
            }
            else
            {
                TbDataItemType item_type = EbDataMgr.Instance.getData<TbDataItemType>(ItemTypeId);
                List<ItemOperateInfo> list_operate = new List<ItemOperateInfo>(item_type.ListOperateInfo);
                ToolTipHelper.Instant.getItemOperate(list_operate, this);

                equip_list_operate_info.AddRange(list_operate);
                main_operate = MainOperateInfo;
            }
            ToolTipEnd tooltip_end = new ToolTipEnd();
            tooltip_end.ItemDesc = Desc;
            tooltip_end.MainOperateInfo = main_operate;
            tooltip_end.ListMoreOperateInfo = equip_list_operate_info;
            return tooltip_end;
        }
    }
}

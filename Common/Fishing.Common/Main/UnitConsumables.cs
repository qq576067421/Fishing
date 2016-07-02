using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class UnitConsumables : Unit
    {
        //-------------------------------------------------------------------------
        public Entity EtSrc { get; set; }
        public UnitType UnitType { get { return UnitType.Consumables; } }
        public Item Item { get; set; }
        public bool IsClient { get; set; }
        public string MadeBy { get; set; }//由谁制造
        public TbDataUnitConsumables TbDataUnitConsumables { get; set; }

        //-------------------------------------------------------------------------
        public void create(Entity et_src, bool is_client, Dictionary<byte, string> map_unit_data)
        {
            EtSrc = et_src;
            TbDataUnitConsumables = EbDataMgr.Instance.getData<TbDataUnitConsumables>(Item.TbDataItem.Id);
            if (map_unit_data == null || map_unit_data.Count == 0)
            {
                if (EtSrc != null)
                {
                    MadeBy = EtSrc.getComponentDef<DefActor>().mPropNickName.get();
                }
            }
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
        }

        //-------------------------------------------------------------------------
        public ToolTip getToolTip(_eUiItemParent item_from)
        {
            ToolTip tool_tip = new ToolTip();
            ConsumToolTipHead tooltip_head = new ConsumToolTipHead();
            tooltip_head.ItemIco = Item.TbDataItem.Icon;
            tooltip_head.ItemName = Item.TbDataItem.Name;
            tooltip_head.ItemTypeId = Item.TbDataItem.ItemTypeId;
            tool_tip.ToolTipHead = tooltip_head;

            ConsumToolTipDetail tooltip_detail = new ConsumToolTipDetail();
            List<_ToolTipContentDetailInfo> list_detail = ToolTipHelper.Instant.getToolTipNormalContentDetailText(((UnitConsumables)Item.UnitLink).TbDataUnitConsumables.ListEffect);
            tooltip_detail.list_detail = list_detail;
            tooltip_detail.MadeBy = MadeBy;
            tool_tip.ToolTipDetail = tooltip_detail;

            ToolTipEnd tooltip_end = new ToolTipEnd();
            tooltip_end.ItemDesc = Item.TbDataItem.Desc;
            ItemOperateInfo main_operate = null;
            List<ItemOperateInfo> list_operate_info = null;
            TbDataItemType item_type = EbDataMgr.Instance.getData<TbDataItemType>(Item.TbDataItem.ItemTypeId);
            if (item_from == _eUiItemParent.Chat || item_from == _eUiItemParent.Mail || item_from == _eUiItemParent.TaskReward)
            {
            }
            else
            {
                main_operate = Item.TbDataItem.MainOperateInfo;
                list_operate_info = new List<ItemOperateInfo>();
                list_operate_info.AddRange(item_type.ListOperateInfo);
            }
            ToolTipHelper.Instant.getItemOperate(list_operate_info, Item.TbDataItem);

            tooltip_end.MainOperateInfo = main_operate;
            tooltip_end.ListMoreOperateInfo = list_operate_info;
            tool_tip.ToolTipEnd = tooltip_end;

            return tool_tip;
        }
    }

    public class UnitConsumablesFactory<T> : TUnitFactory<T> where T : UnitConsumables, new()
    {
        public UnitConsumablesFactory(bool is_client)
            : base(is_client)
        {
        }
    }
}

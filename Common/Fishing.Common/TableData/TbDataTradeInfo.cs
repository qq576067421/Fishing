using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class TbDataTradeInfo : EbData
    {
        //-------------------------------------------------------------------------
        public _eTradeType TradeType { get; private set; }
        public string TradeName { get; private set; }
        public string[] TradeItemTypeIds { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            TradeType = (_eTradeType)prop_set.getPropInt("I_TradeType").get();
            TradeName = prop_set.getPropString("T_TradeName").get();
            string trade_item_type_ids = prop_set.getPropString("T_TradeItemTypeId").get();
            if (!string.IsNullOrEmpty(trade_item_type_ids))
            {
                TradeItemTypeIds = trade_item_type_ids.Split(';');
            }
        }

        //-------------------------------------------------------------------------
        public Dictionary<int, TbDataItemType> getTradeItemType()
        {
            Dictionary<int, TbDataItemType> map_trade_item_type = new Dictionary<int, TbDataItemType>();
            if (TradeItemTypeIds != null)
            {
                foreach (var i in TradeItemTypeIds)
                {
                    int id = int.Parse(i);
                    TbDataItemType item_type = EbDataMgr.Instance.getData<TbDataItemType>(id);
                    map_trade_item_type.Add(id, item_type);
                }
            }

            return map_trade_item_type;
        }
    }

    //-------------------------------------------------------------------------
    public enum _eTradeType
    {
        Market = 1,//集市
        AuctionHouse,//拍卖行
        Mall,//商会
    }
}

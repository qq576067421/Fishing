using System;
using System.Collections.Generic;
using GF.Common;

public class TbDataTrade : EbData
{
    //-------------------------------------------------------------------------
    public TbMallItemInfo MallItemInfo { get; private set; }
    public TbMarketItemInfo MarketItemInfo { get; private set; }
    public TbAuctionHouseItemInfo AuctionHouseItemInfo { get; private set; }

    //-------------------------------------------------------------------------					
    public override void load(EbPropSet prop_set)
    {
        MallItemInfo = new TbMallItemInfo();
        MallItemInfo.MallBuyPrice = prop_set.getPropInt("I_MallBuyPrice").get();
        MallItemInfo.MallSellPrice = prop_set.getPropInt("I_MallSellPrice").get();
        MallItemInfo.MallSellNum = prop_set.getPropInt("I_MallSellNum").get();
        MallItemInfo.MallBuyNum = prop_set.getPropInt("I_MallBuyNum").get();
        MallItemInfo.MallShelves = prop_set.getPropInt("I_MallShelves").get() == 0 ? false : true;

        MarketItemInfo = new TbMarketItemInfo();
        MarketItemInfo.SellPrice = prop_set.getPropInt("I_MarcketSellPrice").get();
        MarketItemInfo.FactoragePrice = prop_set.getPropInt("I_MarcketFactoragePrice").get();
        MarketItemInfo.SafeKeePrice = prop_set.getPropInt("I_MarcketSafeKeepPrice").get();
        MarketItemInfo.Shelves = prop_set.getPropInt("I_MarcketShelves").get() == 0 ? false : true;

        AuctionHouseItemInfo = new TbAuctionHouseItemInfo();
        AuctionHouseItemInfo.SellPrice = prop_set.getPropInt("I_AuctionHouseSellPrice").get();
        AuctionHouseItemInfo.FactoragePrice = prop_set.getPropInt("I_AuctionHouseFactoragePrice").get();
        AuctionHouseItemInfo.SafeKeePrice = prop_set.getPropInt("I_AuctionHouseSafeKeepPrice").get();
    }
}

//-------------------------------------------------------------------------
public class TbMallItemInfo
{
    public int MallBuyPrice;
    public int MallSellPrice;
    public int MallSellNum;//每天可卖数
    public int MallBuyNum;//每天可买数
    public bool MallShelves;//是否可以在商会卖 0否1是
}

//-------------------------------------------------------------------------
public class TbMarketItemInfo
{
    public int SellPrice;//推荐价格
    public int FactoragePrice;//手续费  成功出售后扣取
    public int SafeKeePrice;//保管价格 上架即扣取
    public bool Shelves;//是否可在集市卖
}

//-------------------------------------------------------------------------
public class TbAuctionHouseItemInfo
{
    public int SellPrice;//推荐价格
    public int FactoragePrice;//手续费  成功出售后扣取
    public int SafeKeePrice;//保管价格 上架即扣取    
}

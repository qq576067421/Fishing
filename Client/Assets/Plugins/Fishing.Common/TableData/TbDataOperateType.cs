using System;
using System.Collections.Generic;
using GF.Common;

public class TbDataOperateType : EbData
{
    //-------------------------------------------------------------------------
    public _eOperateType OperateType { get; private set; }
    public string OperateName { get; private set; }
    public int OperateEffectId { get; private set; }
    public bool IsCompandType { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        OperateType = (_eOperateType)Id;
        OperateName = prop_set.getPropString("T_OperateName").get();
        OperateEffectId = prop_set.getPropInt("I_EffectId").get();
        IsCompandType = prop_set.getPropInt("I_IsCompandType").get() == 1 ? true : false;
    }
}

//-------------------------------------------------------------------------
public enum _eOperateType
{
    None = 0,
    Enhance,
    Decompose,
    Compound,
    Split,
    Sell,
    Market,
    Auction,
    UpRank,
    Upgrade,
    Forget,
    Learn,
    Inset,
    Takeon,
    Takeoff,
    Use,
    Repair,
    Choose
}

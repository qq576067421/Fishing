using System;
using System.Collections.Generic;
using GF.Common;

public class TbDataTips : EbData
{
    //-------------------------------------------------------------------------
    public string Content { get; private set; }
    public Ps.TipsType TipsType { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        Content = prop_set.getPropString("Content").get();
        TipsType = (Ps.TipsType)prop_set.getPropInt("TipType").get();
    }
}

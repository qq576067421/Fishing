using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataDesktopInfo : EbData
{
    //-------------------------------------------------------------------------
    public int SmallBlind { get; private set; }
    public int BigBlind { get; private set; }
    public int BetMin { get; private set; }
    public int BetMax { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        SmallBlind = prop_set.getPropInt("I_SmallBlind").get();
        BigBlind = prop_set.getPropInt("I_BigBlind").get();
        BetMin = prop_set.getPropInt("I_BetMin").get();
        BetMax = prop_set.getPropInt("I_BetMax").get();
    }
}

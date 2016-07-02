using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataColorCode : EbData
{
    //-------------------------------------------------------------------------
    public string ReplaceCode { get; private set; }
    public string RealCode { get; private set; }
    public string Name { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        ReplaceCode = prop_set.getPropString("ColorReplaceCode").get();
        RealCode = prop_set.getPropString("ColorRealCode").get();
        Name = prop_set.getPropString("ColorName").get();
    }
}

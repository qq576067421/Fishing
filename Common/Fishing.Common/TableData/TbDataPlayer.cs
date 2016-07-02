using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataPlayer : EbData
{
    //-------------------------------------------------------------------------    
    public string AiName { get; private set; }
    public string DefaultIcon { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        AiName = prop_set.getPropString("AiName").get();
        DefaultIcon = prop_set.getPropString("DefaultIcon").get();
    }
}
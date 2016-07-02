using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataChatIco : EbData
{
    //-------------------------------------------------------------------------
    public string IcoName { get; private set; }
    public string IcoDescribe { get; private set; }
    public int IcoType { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        IcoName = prop_set.getPropString("IcoName").get();
        IcoDescribe = prop_set.getPropString("IcoDescribe").get();
        IcoType = prop_set.getPropInt("IcoType").get();
    }
}
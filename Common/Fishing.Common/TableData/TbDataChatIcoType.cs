using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataChatIcoType : EbData
{
    //-------------------------------------------------------------------------
    public string ChatIcoTypeNameEn { get; private set; }
    public string ChatIcoTypeNameCh { get; private set; }
    public string ChatIcoTypeIcoName { get; private set; }
    public int ChatIcoType { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        ChatIcoTypeNameEn = prop_set.getPropString("ChatIcoTypeNameEn").get();
        ChatIcoTypeNameCh = prop_set.getPropString("ChatIcoTypeNameCh").get();
        ChatIcoTypeIcoName = prop_set.getPropString("ChatIcoTypeIcoName").get();
        ChatIcoType = prop_set.getPropInt("ChatIcoType").get();
    }
}
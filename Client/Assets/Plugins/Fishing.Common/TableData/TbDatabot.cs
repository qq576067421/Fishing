using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataBot : EbData
{
    //-------------------------------------------------------------------------
    public string EtGuid { get; private set; }// EtGuid
    public string NickName { get; private set; }// 昵称

    public string AvatarIconName { get; private set; }
    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        EtGuid = prop_set.getPropString("T_EtGuid").get();
        NickName = prop_set.getPropString("T_NickName").get();
        AvatarIconName = prop_set.getPropString("T_AvatarIconName").get();
    }
}

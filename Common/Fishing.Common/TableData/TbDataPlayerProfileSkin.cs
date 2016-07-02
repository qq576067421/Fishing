using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataPlayerProfileSkin : EbData
{
    //-------------------------------------------------------------------------
    public string ProfileSkinName { get; private set; }
    public string ProfileSkinPrefabName { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        ProfileSkinName = prop_set.getPropString("T_ProfileSkinName").get();
        ProfileSkinPrefabName = prop_set.getPropString("T_ProfileSkinPrefabName").get();
    }
}

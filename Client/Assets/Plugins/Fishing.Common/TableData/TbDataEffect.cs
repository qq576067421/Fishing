using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataEffect : EbData
{
    //-------------------------------------------------------------------------
    public string Name { get; private set; }
    public string ScriptName { get; private set; }
    public string[] PredefineParamList { get; private set; }
    public string FormatDesc { get; private set; }
    public string SelfDefine1 { get; private set; }
    public string SelfDefine2 { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        Name = prop_set.getPropString("T_Name").get();
        ScriptName = prop_set.getPropString("T_ScriptName").get();
        string predefine_paramlist = prop_set.getPropString("T_PredefineParamList").get();
        if (!string.IsNullOrEmpty(predefine_paramlist)) PredefineParamList = predefine_paramlist.Split(';');
        FormatDesc = prop_set.getPropString("T_FormatDesc").get();
        SelfDefine1 = prop_set.getPropString("T_SelfDefine1").get();
        SelfDefine2 = prop_set.getPropString("T_SelfDefine2").get();
    }
}

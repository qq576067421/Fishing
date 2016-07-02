using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;
using Ps;

public class TbDataStatus : EbData
{
    //-------------------------------------------------------------------------
    public string Name { get; private set; }// 名称
    public int StatusCreator { get; private set; }// 状态生成器描述
    public string StatusTemplate { get; private set; }// 状态模板描述
    public StatusOverlapType StatusOverlapType { get; private set; }// 状态叠加方式
    public string Desc { get; private set; }// 描述
    public string PrefabName { get; private set; }// Status对应的客户端特效Prefab
    public string Icon { get; private set; }// 图标
    public string ToolTipTemplate { get; private set; }// ToolTip模板

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        Name = prop_set.getPropString("T_Name").get();
        StatusCreator = prop_set.getPropInt("I_StatusCreator").get();
        StatusTemplate = prop_set.getPropString("T_StatusTemplate").get();
        StatusOverlapType = (StatusOverlapType)prop_set.getPropInt("I_StatusOverlapType").get();
        Desc = prop_set.getPropString("T_Desc").get();
        PrefabName = prop_set.getPropString("T_PrefabName").get();
        Icon = prop_set.getPropString("T_Icon").get();
        ToolTipTemplate = prop_set.getPropString("T_ToolTipTemplate").get();
    }
}

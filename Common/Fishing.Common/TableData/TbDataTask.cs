using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public struct OneAwardItem
{
    public int item_id;
    public int count;
}

public class TbDataTask : EbData
{
    //-------------------------------------------------------------------------
    public Ps.TaskType TaskType { get; private set; }// 任务类型
    public Ps.TaskCategory TaskCategory { get; private set; }// 任务分类
    public int ParentId { get; private set; }// 父任务Id，0表示无父任务
    public int NextId { get; private set; }// 后续任务Id，0表示无后续任务
    public int RequireLevel { get; private set; }// 任务需求等级
    public int DoSceneId { get; private set; }// 执行任务所在场景Id
    public int AcceptNpcId { get; private set; }// 接任务的NpcId
    public int AcceptSceneId { get; private set; }// 接任务所在场景Id
    public string AcceptNpcDialogue { get; private set; }// 接任务的Npc对白
    public int FinishNpcId { get; private set; }// 交任务的NpcId
    public int FinishSceneId { get; private set; }// 交任务所在场景Id
    public string FinishNpcDialogue { get; private set; }// 交任务的Npc对白
    public string Name { get; private set; }// 任务名称
    public string Desc { get; private set; }// 任务描述
    public int AwardExp { get; private set; }// 奖励经验
    public int AwardSilver { get; private set; }// 奖励银币
    public List<OneAwardItem> ListAwardItem { get; private set; }// 奖励道具列表

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        TaskType = (Ps.TaskType)prop_set.getPropInt("I_TaskType").get();
        TaskCategory = (Ps.TaskCategory)prop_set.getPropInt("I_TaskCategory").get();
        ParentId = prop_set.getPropInt("I_ParentId").get();
        NextId = prop_set.getPropInt("I_NextId").get();
        Name = prop_set.getPropString("T_Name").get();
        Desc = prop_set.getPropString("T_Desc").get();
        RequireLevel = prop_set.getPropInt("I_RequireLevel").get();
        DoSceneId = prop_set.getPropInt("I_DoSceneId").get();
        AcceptNpcId = prop_set.getPropInt("I_AcceptNpcId").get();
        AcceptSceneId = prop_set.getPropInt("I_AcceptSceneId").get();
        AcceptNpcDialogue = prop_set.getPropString("T_AcceptNpcDialogue").get();
        FinishNpcId = prop_set.getPropInt("I_FinishNpcId").get();
        FinishSceneId = prop_set.getPropInt("I_FinishSceneId").get();
        FinishNpcDialogue = prop_set.getPropString("T_FinishNpcDialogue").get();
        AwardExp = prop_set.getPropInt("I_AwardExp").get();
        AwardSilver = prop_set.getPropInt("I_AwardSilver").get();

        ListAwardItem = new List<OneAwardItem>(3);
        Prop<int> prop = null;
        prop = prop_set.getPropInt("I_AwardItemId1");
        if (prop != null && prop.get() != 0)
        {
            OneAwardItem d;
            d.item_id = prop.get();
            d.count = prop_set.getPropInt("I_AwardItemCount1").get();
            ListAwardItem.Add(d);
        }

        prop = prop_set.getPropInt("I_AwardItemId2");
        if (prop != null && prop.get() != 0)
        {
            OneAwardItem d;
            d.item_id = prop.get();
            d.count = prop_set.getPropInt("I_AwardItemCount2").get();
            ListAwardItem.Add(d);
        }

        prop = prop_set.getPropInt("I_AwardItemId3");
        if (prop != null && prop.get() != 0)
        {
            OneAwardItem d;
            d.item_id = prop.get();
            d.count = prop_set.getPropInt("I_AwardItemCount3").get();
            ListAwardItem.Add(d);
        }
    }
}

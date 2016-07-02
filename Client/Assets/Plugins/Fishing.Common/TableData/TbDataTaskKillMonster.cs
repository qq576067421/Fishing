using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public struct OneMonster
{
    public int monster_id;
    public int count;
}

public class TbDataTaskKillMonster : EbData
{
    //-------------------------------------------------------------------------
    public string Progress { get; private set; }// 任务进度
    public int SceneProduceId { get; private set; }
    public List<OneMonster> ListMonster { get; private set; }// 怪物列表

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        Progress = prop_set.getPropString("T_Progress").get();
        SceneProduceId = prop_set.getPropInt("I_SceneProduceId").get();

        ListMonster = new List<OneMonster>();

        Prop<int> p = null;
        p = prop_set.getPropInt("I_MonsterId1");
        if (p != null && p.get() != 0)
        {
            OneMonster d;
            d.monster_id = p.get();
            d.count = prop_set.getPropInt("I_MonsterCount1").get();
            ListMonster.Add(d);
        }

        p = prop_set.getPropInt("I_MonsterId2");
        if (p != null && p.get() != 0)
        {
            OneMonster d;
            d.monster_id = p.get();
            d.count = prop_set.getPropInt("I_MonsterCount2").get();
            ListMonster.Add(d);
        }

        p = prop_set.getPropInt("I_MonsterId3");
        if (p != null && p.get() != 0)
        {
            OneMonster d;
            d.monster_id = p.get();
            d.count = prop_set.getPropInt("I_MonsterCount3").get();
            ListMonster.Add(d);
        }
    }
}

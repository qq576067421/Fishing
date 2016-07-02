using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public struct OneCollectItem
{
    public int item_id;
    public int count;
}

public class TbDataTaskCollectItem : EbData
{
    //-------------------------------------------------------------------------
    public string Progress { get; private set; }// 任务进度
    public int SceneProduceId { get; private set; }
    public List<OneCollectItem> ListCollectItem { get; private set; }// 采集物列表

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        Progress = prop_set.getPropString("T_Progress").get();
        SceneProduceId = prop_set.getPropInt("I_SceneProduceId").get();

        ListCollectItem = new List<OneCollectItem>();

        Prop<int> p = null;
        p = prop_set.getPropInt("I_CollectItemId1");
        if (p != null && p.get() != 0)
        {
            OneCollectItem d;
            d.item_id = p.get();
            d.count = prop_set.getPropInt("I_CollectItemCount1").get();
            ListCollectItem.Add(d);
        }

        p = prop_set.getPropInt("I_CollectItemId2");
        if (p != null && p.get() != 0)
        {
            OneCollectItem d;
            d.item_id = p.get();
            d.count = prop_set.getPropInt("I_CollectItemCount2").get();
            ListCollectItem.Add(d);
        }

        p = prop_set.getPropInt("I_CollectItemId3");
        if (p != null && p.get() != 0)
        {
            OneCollectItem d;
            d.item_id = p.get();
            d.count = prop_set.getPropInt("I_CollectItemCount3").get();
            ListCollectItem.Add(d);
        }
    }
}

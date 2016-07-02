using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;
using Ps;

public class OneItemData
{
    public int item_id;
    public int num_cur;
    public int num_total;
}

// 寻物任务
public class TaskCollectItem : TaskBase
{
    //-------------------------------------------------------------------------
    public TbDataTaskCollectItem TbDataTaskCollectItem { get; private set; }
    List<OneItemData> ListItemData { get; set; }
    CellPlayer<DefPlayer> CoPlayer { get; set; }

    //-------------------------------------------------------------------------
    public TaskCollectItem(TaskMgr task_mgr, Entity et, TaskData task_data)
        : base(task_mgr, et, task_data)
    {
    }

    //-------------------------------------------------------------------------
    public TaskCollectItem(TaskMgr task_mgr, Entity et, int task_id)
        : base(task_mgr, et, task_id)
    {
    }

    //-------------------------------------------------------------------------
    public override void onInit()
    {
        CoPlayer = Entity.getComponent<CellPlayer<DefPlayer>>();
        TbDataTaskCollectItem = EbDataMgr.Instance.getData<TbDataTaskCollectItem>(TaskData.task_id);
        ListItemData = new List<OneItemData>();

        if (TaskData.task_state == TaskState.Init)
        {
            if (TbDataTask.AcceptNpcId == 0)
            {
                TaskData.task_state = TaskState.Doing;
            }
            else
            {
                TaskData.task_state = TaskState.CanDo;
            }
        }

        // 主动查询一次背包中是否有所需任务物品
        foreach (var i in TbDataTaskCollectItem.ListCollectItem)
        {
            OneItemData one_item_data = new OneItemData();
            one_item_data.item_id = i.item_id;
            one_item_data.num_total = i.count;
            int num_cur = CoPlayer.CoBag.getItemNumByItemId(i.item_id);
            if (num_cur > i.count) num_cur = i.count;
            one_item_data.num_cur = num_cur;

            ListItemData.Add(one_item_data);
        }

        if (TaskData.task_state == TaskState.Doing && _isDone())
        {
            TaskData.task_state = TaskState.Done;
        }

        _saveTaskData();

        if (TaskData.task_state == TaskState.Done && TbDataTask.FinishNpcId == 0)
        {
            TaskData.task_state = TaskState.Release;
            TaskMgr._serverAddDirtyTask(this);
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
        if (e is EvCellBagAddItem)
        {
            // CellBag，添加道具消息
            var ev = (EvCellBagAddItem)e;

            // 主动查询一次背包中是否有所需任务物品
            foreach (var i in TbDataTaskCollectItem.ListCollectItem)
            {
                int num_cur = CoPlayer.CoBag.getItemNumByItemId(i.item_id);
                if (num_cur > i.count) num_cur = i.count;

                foreach (var j in ListItemData)
                {
                    if (j.item_id == i.item_id)
                    {
                        if (j.num_cur != num_cur)
                        {
                            j.num_cur = num_cur;
                            _saveTaskData();
                            TaskMgr._serverAddDirtyTask(this);
                        }
                        break;
                    }
                }
            }

            if (TaskData.task_state == TaskState.Doing && _isDone())
            {
                TaskData.task_state = TaskState.Done;
                TaskMgr._serverAddDirtyTask(this);
            }

            if (TaskData.task_state == TaskState.Done && TbDataTask.FinishNpcId == 0)
            {
                TaskData.task_state = TaskState.Release;
                TaskMgr._serverAddDirtyTask(this);
            }
        }
    }

    //-------------------------------------------------------------------------
    public override bool clientAboutNpc(int npc_id)
    {
        return false;
    }

    //-------------------------------------------------------------------------
    public override List<OneDialogue> clientGetNpcDialogueWhenPickNpc(int npc_id)
    {
        return null;
    }

    //-------------------------------------------------------------------------
    public override void clientNpcDialogueEnd(int npc_id, TaskState init_dialogue_task_state)
    {
    }

    //-------------------------------------------------------------------------
    public override string clientGetProgressText()
    {
        return "";
    }

    //-------------------------------------------------------------------------
    // 客户端获取任务自动执行所需信息
    public override TaskAutoInfo clientGetTaskAutoInfo()
    {
        return null;
    }

    //-------------------------------------------------------------------------
    public override void c2sTaskAccept()
    {
    }

    //-------------------------------------------------------------------------
    public override void c2sTaskExcute()
    {
    }

    //-------------------------------------------------------------------------
    public override void c2sTaskFinish()
    {
    }

    //-------------------------------------------------------------------------
    public override void c2sTaskGiveUp()
    {
    }

    //-------------------------------------------------------------------------
    void _saveTaskData()
    {
        StringBuilder sb = new StringBuilder(64);
        foreach (var i in ListItemData)
        {
            sb.Append(i.item_id.ToString());
            sb.Append(';');
            sb.Append(i.num_cur.ToString());
            sb.Append(';');
        }

        TaskData.task_data[0] = sb.ToString();
    }

    //-------------------------------------------------------------------------
    bool _isDone()
    {
        bool done = true;
        foreach (var i in ListItemData)
        {
            if (i.num_cur != i.num_total)
            {
                done = false;
                break;
            }
        }
        return done;
    }
}

public class TaskFactoryCollectItem : TaskFactory
{
    //-------------------------------------------------------------------------
    public override TaskType getTaskType()
    {
        return TaskType.CollectItem;
    }

    //-------------------------------------------------------------------------
    public override TaskBase createTask(TaskData task_data)
    {
        var task = new TaskCollectItem(TaskMgr, Entity, task_data);
        return task;
    }

    //-------------------------------------------------------------------------
    public override TaskBase createTask(int task_id)
    {
        var task = new TaskCollectItem(TaskMgr, Entity, task_id);
        return task;
    }
}

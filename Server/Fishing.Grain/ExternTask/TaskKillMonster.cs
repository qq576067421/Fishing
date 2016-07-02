using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;
using Ps;

public class OneMonsterData
{
    public int monster_id;
    public int num_cur;
    public int num_total;
}

// 打怪任务
public class TaskKillMonster : TaskBase
{
    //-------------------------------------------------------------------------
    public TbDataTaskKillMonster TbDataTaskKillMonster { get; private set; }
    List<OneMonsterData> ListMonsterData { get; set; }

    //-------------------------------------------------------------------------
    public TaskKillMonster(TaskMgr task_mgr, Entity et, TaskData task_data)
        : base(task_mgr, et, task_data)
    {
    }

    //-------------------------------------------------------------------------
    public TaskKillMonster(TaskMgr task_mgr, Entity et, int task_id)
        : base(task_mgr, et, task_id)
    {
    }

    //-------------------------------------------------------------------------
    public override void onInit()
    {
        TbDataTaskKillMonster = EbDataMgr.Instance.getData<TbDataTaskKillMonster>(TaskData.task_id);
        ListMonsterData = new List<OneMonsterData>();

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

        _loadTaskData();

        if (TaskData.task_state == TaskState.Doing && _isDone())
        {
            TaskData.task_state = TaskState.Done;
        }

        if (TaskData.task_state == TaskState.Done && TbDataTask.FinishNpcId == 0)
        {
            TaskData.task_state = TaskState.Release;
            TaskMgr._serverAddDirtyTask(this);
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
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
    void _loadTaskData()
    {
        ListMonsterData.Clear();

        foreach (var i in TbDataTaskKillMonster.ListMonster)
        {
            OneMonsterData one_monster_data = new OneMonsterData();
            one_monster_data.monster_id = i.monster_id;
            one_monster_data.num_cur = 0;
            one_monster_data.num_total = i.count;

            ListMonsterData.Add(one_monster_data);
        }

        string data = null;
        if (TaskData.task_data.TryGetValue(0, out data))
        {
            int index = 0;
            string[] list_data = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            while (index < list_data.Length)
            {
                int monster_id = int.Parse(list_data[index++]);
                int num_cur = int.Parse(list_data[index++]);

                foreach (var j in ListMonsterData)
                {
                    if (j.monster_id == monster_id)
                    {
                        j.num_cur = num_cur;
                        break;
                    }
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    void _saveTaskData()
    {
        StringBuilder sb = new StringBuilder(64);
        foreach (var i in ListMonsterData)
        {
            sb.Append(i.monster_id.ToString());
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
        foreach (var i in ListMonsterData)
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

public class TaskFactoryKillMonster : TaskFactory
{
    //-------------------------------------------------------------------------
    public override TaskType getTaskType()
    {
        return TaskType.KillMonster;
    }

    //-------------------------------------------------------------------------
    public override TaskBase createTask(TaskData task_data)
    {
        var task = new TaskKillMonster(TaskMgr, Entity, task_data);
        return task;
    }

    //-------------------------------------------------------------------------
    public override TaskBase createTask(int task_id)
    {
        var task = new TaskKillMonster(TaskMgr, Entity, task_id);
        return task;
    }
}

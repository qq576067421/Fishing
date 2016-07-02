using System;
using System.Collections.Generic;
using GF.Common;
using Ps;

// 对白任务
public class TaskDialogue : TaskBase
{
    //-------------------------------------------------------------------------
    public TbDataTaskDialogue TbDataTaskDialogue { get; private set; }

    //-------------------------------------------------------------------------
    public TaskDialogue(TaskMgr task_mgr, Entity et, TaskData task_data)
        : base(task_mgr, et, task_data)
    {
    }

    //-------------------------------------------------------------------------
    public TaskDialogue(TaskMgr task_mgr, Entity et, int task_id)
        : base(task_mgr, et, task_id)
    {
    }

    //-------------------------------------------------------------------------
    public override void onInit()
    {
        TbDataTaskDialogue = EbDataMgr.Instance.getData<TbDataTaskDialogue>(TaskData.task_id);

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
        if (TaskData.task_state == TaskState.Doing)
        {
            TaskData.task_state = TaskState.Done;
            TaskMgr._serverAddDirtyTask(this);

            if (TbDataTask.FinishNpcId == 0)
            {
                TaskData.task_state = TaskState.Release;
                TaskMgr._serverAddDirtyTask(this);
            }
        }
    }

    //-------------------------------------------------------------------------
    public override void c2sTaskGiveUp()
    {
    }
}

public class TaskFactoryDialogue : TaskFactory
{
    //-------------------------------------------------------------------------
    public override TaskType getTaskType()
    {
        return TaskType.Dialogue;
    }

    //-------------------------------------------------------------------------
    public override TaskBase createTask(TaskData task_data)
    {
        var task = new TaskDialogue(TaskMgr, Entity, task_data);
        return task;
    }

    //-------------------------------------------------------------------------
    public override TaskBase createTask(int task_id)
    {
        var task = new TaskDialogue(TaskMgr, Entity, task_id);
        return task;
    }
}

using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    // 对白任务
    public class TaskDialogue : TaskBase
    {
        //-------------------------------------------------------------------------
        public TbDataTaskDialogue TbDataTaskDialogue { get; private set; }
        ClientPlayerTask<DefPlayerTask> CoPlayerTask { get; set; }
        int DialogueIndex { get; set; }

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
            CoPlayerTask = Entity.getComponent<ClientPlayerTask<DefPlayerTask>>();
            TbDataTaskDialogue = EbDataMgr.Instance.getData<TbDataTaskDialogue>(TaskData.task_id);
            DialogueIndex = 0;
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
        }

        //-------------------------------------------------------------------------
        public override bool clientAboutNpc(int npc_id)
        {
            if (TaskData.task_state == TaskState.CanDo)
            {
                if (npc_id == TbDataTask.AcceptNpcId) return true;
            }
            else if (TaskData.task_state == TaskState.Doing)
            {
                if (npc_id == TbDataTaskDialogue.NpcId) return true;
            }
            else if (TaskData.task_state == TaskState.Done)
            {
                if (npc_id == TbDataTask.FinishNpcId) return true;
            }

            return false;
        }

        //-------------------------------------------------------------------------
        public override List<OneDialogue> clientGetNpcDialogueWhenPickNpc(int npc_id)
        {
            List<OneDialogue> list_dialogue = new List<OneDialogue>();
            if (TaskData.task_state == TaskState.CanDo && npc_id == TbDataTask.AcceptNpcId)
            {
                OneDialogue dialouge;
                dialouge.is_me = false;
                dialouge.dialogue = TbDataTask.AcceptNpcDialogue;
                list_dialogue.Add(dialouge);
            }
            else if (TaskData.task_state == TaskState.Doing && npc_id == TbDataTaskDialogue.NpcId)
            {
                foreach (var i in TbDataTaskDialogue.ListDialogue)
                {
                    list_dialogue.Add(i);
                }
            }
            else if (TaskData.task_state == TaskState.Done && npc_id == TbDataTask.FinishNpcId)
            {
                OneDialogue dialouge;
                dialouge.is_me = false;
                dialouge.dialogue = TbDataTask.FinishNpcDialogue;
                list_dialogue.Add(dialouge);
            }

            return list_dialogue;
        }

        //-------------------------------------------------------------------------
        public override void clientNpcDialogueEnd(int npc_id, TaskState init_dialogue_task_state)
        {
            if (TaskData.task_state == TaskState.Doing && init_dialogue_task_state == TaskState.Doing && npc_id == TbDataTaskDialogue.NpcId)
            {
                CoPlayerTask.requestFinishTask(TbDataTask.Id);
            }
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
            if (TaskData.task_state == TaskState.CanDo && TbDataTask.AcceptNpcId != 0)
            {
                TaskAutoInfo task_autoinfo = new TaskAutoInfo();
                task_autoinfo.type = TaskAutoType.NpcDialogue;
                task_autoinfo.scene_id = TbDataTask.AcceptSceneId;
                task_autoinfo.npc_id = TbDataTask.AcceptNpcId;
                task_autoinfo.pos = EbVector2.Zero;
                return task_autoinfo;
            }
            else if (TaskData.task_state == TaskState.Doing)
            {
                TaskAutoInfo task_autoinfo = new TaskAutoInfo();
                task_autoinfo.type = TaskAutoType.NpcDialogue;
                task_autoinfo.scene_id = TbDataTask.DoSceneId;
                task_autoinfo.npc_id = TbDataTaskDialogue.NpcId;
                task_autoinfo.pos = EbVector2.Zero;
                return task_autoinfo;
            }
            else if (TaskData.task_state == TaskState.Done && TbDataTask.FinishNpcId != 0)
            {
                TaskAutoInfo task_autoinfo = new TaskAutoInfo();
                task_autoinfo.type = TaskAutoType.NpcDialogue;
                task_autoinfo.scene_id = TbDataTask.FinishSceneId;
                task_autoinfo.npc_id = TbDataTask.FinishNpcId;
                task_autoinfo.pos = EbVector2.Zero;
                return task_autoinfo;
            }

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
}

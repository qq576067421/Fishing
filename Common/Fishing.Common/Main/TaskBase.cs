using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public enum TaskAutoType
    {
        NpcDialogue = 0,// 与Npc对话
        KillMonster// 打怪
    }

    public class TaskAutoInfo
    {
        public TaskAutoType type;
        public int scene_id;
        public int npc_id;
        public EbVector2 pos;
    }

    public abstract class TaskBase
    {
        //-------------------------------------------------------------------------
        public TaskMgr TaskMgr { get; private set; }
        public Entity Entity { get; private set; }
        public TbDataTask TbDataTask { get; private set; }
        public TaskData TaskData { get; private set; }

        //-------------------------------------------------------------------------
        public TaskBase(TaskMgr task_mgr, Entity et, TaskData task_data)
        {
            _init(task_mgr, et, task_data);
        }

        //-------------------------------------------------------------------------
        public TaskBase(TaskMgr task_mgr, Entity et, int task_id)
        {
            TaskData task_data = new TaskData();
            task_data.task_id = task_id;
            task_data.task_state = TaskState.Init;
            task_data.task_data = new Dictionary<byte, string>();

            _init(task_mgr, et, task_data);
        }

        //-------------------------------------------------------------------------
        public List<Item> getListAwardItem()
        {
            List<Item> l = new List<Item>();
            foreach (var i in TbDataTask.ListAwardItem)
            {
                Item item = new Item(Entity, i.item_id, 1);
                l.Add(item);
            }
            return l;
        }

        //-------------------------------------------------------------------------
        public void clientUpdateTask(TaskData task_data)
        {
            TaskData = task_data;
        }

        //-------------------------------------------------------------------------
        public abstract void onInit();

        //-------------------------------------------------------------------------
        public abstract void handleEvent(object sender, EntityEvent e);

        //-------------------------------------------------------------------------
        public abstract bool clientAboutNpc(int npc_id);

        //-------------------------------------------------------------------------
        public abstract List<OneDialogue> clientGetNpcDialogueWhenPickNpc(int npc_id);

        //-------------------------------------------------------------------------
        public abstract void clientNpcDialogueEnd(int npc_id, TaskState init_dialogue_task_state);

        //-------------------------------------------------------------------------
        public abstract string clientGetProgressText();

        //-------------------------------------------------------------------------
        // 客户端获取任务自动执行所需信息
        public abstract TaskAutoInfo clientGetTaskAutoInfo();

        //-------------------------------------------------------------------------
        public abstract void c2sTaskAccept();

        //-------------------------------------------------------------------------
        public abstract void c2sTaskExcute();

        //-------------------------------------------------------------------------
        public abstract void c2sTaskFinish();

        //-------------------------------------------------------------------------
        public abstract void c2sTaskGiveUp();

        //-------------------------------------------------------------------------
        void _init(TaskMgr task_mgr, Entity et, TaskData task_data)
        {
            TaskMgr = task_mgr;
            Entity = et;

            TaskData = task_data;
            if (TaskData.task_data == null) TaskData.task_data = new Dictionary<byte, string>();

            TbDataTask = EbDataMgr.Instance.getData<TbDataTask>(TaskData.task_id);

            onInit();
        }
    }

    public abstract class TaskFactory
    {
        //-------------------------------------------------------------------------
        public TaskMgr TaskMgr { get; set; }
        public Entity Entity { get; set; }

        //-------------------------------------------------------------------------
        public abstract TaskType getTaskType();

        //-------------------------------------------------------------------------
        public abstract TaskBase createTask(TaskData task_data);

        //-------------------------------------------------------------------------
        public abstract TaskBase createTask(int task_id);
    }
}

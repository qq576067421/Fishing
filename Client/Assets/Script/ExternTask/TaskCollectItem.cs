using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
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
        ClientPlayer<DefPlayer> CoPlayer { get; set; }

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
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();
            TbDataTaskCollectItem = EbDataMgr.Instance.getData<TbDataTaskCollectItem>(TaskData.task_id);
            ListItemData = new List<OneItemData>();

            _loadTaskData();
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
            ListItemData.Clear();

            foreach (var i in TbDataTaskCollectItem.ListCollectItem)
            {
                OneItemData one_item_data = new OneItemData();
                one_item_data.item_id = i.item_id;
                one_item_data.num_cur = 0;
                one_item_data.num_total = i.count;

                ListItemData.Add(one_item_data);
            }

            string data = null;
            if (TaskData.task_data != null && TaskData.task_data.TryGetValue(0, out data))
            {
                int index = 0;
                string[] list_data = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                while (index < list_data.Length)
                {
                    int item_id = int.Parse(list_data[index++]);
                    int num_cur = int.Parse(list_data[index++]);

                    foreach (var j in ListItemData)
                    {
                        if (j.item_id == item_id)
                        {
                            j.num_cur = num_cur;
                            break;
                        }
                    }
                }
            }
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
}

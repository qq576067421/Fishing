using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
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
        ClientPlayer<DefPlayer> CoPlayer { get; set; }

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
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();
            TbDataTaskKillMonster = EbDataMgr.Instance.getData<TbDataTaskKillMonster>(TaskData.task_id);
            ListMonsterData = new List<OneMonsterData>();

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
            if (TaskData.task_data != null && TaskData.task_data.TryGetValue(0, out data))
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
}

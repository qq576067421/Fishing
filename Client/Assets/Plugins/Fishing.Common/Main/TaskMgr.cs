using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class TaskMgr
    {
        //-------------------------------------------------------------------------
        public delegate void DelegateServerTaskAward(TaskBase task);
        public delegate void DelegateClientTaskStateChange(TaskBase task);

        //-------------------------------------------------------------------------
        Dictionary<int, TaskData> mMapDirtyTask = new Dictionary<int, TaskData>();

        //-------------------------------------------------------------------------
        public Dictionary<int, TaskBase> MapTask { get; private set; }
        public DelegateServerTaskAward OnServerTaskAward { get; set; }
        public DelegateClientTaskStateChange OnClientTaskStateChange { get; set; }
        Entity Entity { get; set; }
        bool IsClient { get; set; }
        Dictionary<TaskType, TaskFactory> MapTaskFactory { get; set; }
        Queue<int> QueTaskDone { get; set; }

        //-------------------------------------------------------------------------
        public TaskMgr(Entity et, bool is_client)
        {
            Entity = et;
            IsClient = is_client;
            MapTaskFactory = new Dictionary<TaskType, TaskFactory>();
            MapTask = new Dictionary<int, TaskBase>();
            QueTaskDone = new Queue<int>();
        }

        //-------------------------------------------------------------------------
        public void regTaskFactory(TaskFactory task_factory)
        {
            task_factory.TaskMgr = this;
            task_factory.Entity = Entity;

            MapTaskFactory[task_factory.getTaskType()] = task_factory;
        }

        //-------------------------------------------------------------------------
        public void serverParseTaskUpdateInfo()
        {
            // 读配置文件

            // 读数据库中的任务执行信息

            // 比对，更新任务运行时信息
        }

        //-------------------------------------------------------------------------
        public void serverUpdate(float elapsed_tm)
        {
            if (QueTaskDone.Count > 0)
            {
                foreach (var i in QueTaskDone)
                {
                    TaskBase task = null;
                    MapTask.TryGetValue(i, out task);
                    if (task == null) continue;

                    if (task.TaskData.task_state == TaskState.Release)
                    {
                        // 查询下一个任务
                        int next_task_id = task.TbDataTask.NextId;

                        // 创建下一个任务
                        TbDataTask next_tbdata_task = EbDataMgr.Instance.getData<TbDataTask>(next_task_id);

                        if (next_tbdata_task == null)
                        {
                            continue;
                        }

                        TaskFactory next_task_factory = null;
                        MapTaskFactory.TryGetValue(next_tbdata_task.TaskType, out next_task_factory);

                        if (next_task_factory != null)
                        {
                            TaskBase next_task = next_task_factory.createTask(next_task_id);
                            MapTask[next_task.TaskData.task_id] = next_task;

                            mMapDirtyTask[next_task_id] = next_task.TaskData;
                        }
                    }
                }
            }

            while (QueTaskDone.Count > 0)
            {
                int task_id = QueTaskDone.Dequeue();
                MapTask.Remove(task_id);
            }
        }

        //-------------------------------------------------------------------------
        public void clientUpdateTask(Dictionary<int, TaskData> map_taskdata)
        {
            foreach (var i in map_taskdata)
            {
                TaskBase task = null;
                MapTask.TryGetValue(i.Key, out task);
                if (task != null)
                {
                    TaskState old_state = task.TaskData.task_state;
                    task.clientUpdateTask(i.Value);
                    TaskState new_state = task.TaskData.task_state;

                    if (OnClientTaskStateChange != null && old_state != new_state)
                    {
                        OnClientTaskStateChange(task);
                    }

                    // 客户端移除已完成的任务
                    if (task.TaskData.task_state == TaskState.Release)
                    {
                        MapTask.Remove(task.TbDataTask.Id);
                    }
                }
                else
                {
                    TbDataTask tbdata_task = EbDataMgr.Instance.getData<TbDataTask>(i.Key);
                    TaskFactory task_factory = null;
                    MapTaskFactory.TryGetValue(tbdata_task.TaskType, out task_factory);

                    if (task_factory != null)
                    {
                        TaskBase task_base = task_factory.createTask(i.Value);
                        MapTask[task_base.TaskData.task_id] = task_base;

                        if (OnClientTaskStateChange != null)
                        {
                            OnClientTaskStateChange(task);
                        }
                    }
                }
            }
        }

        //-------------------------------------------------------------------------
        public List<TaskBase> clientGetTaskListAboutNpc(int npc_id)
        {
            List<TaskBase> list_task = null;

            foreach (var i in MapTask)
            {
                bool r = i.Value.clientAboutNpc(npc_id);
                if (r)
                {
                    if (list_task == null) list_task = new List<TaskBase>();
                    list_task.Add(i.Value);
                }
            }

            return list_task;
        }

        //-------------------------------------------------------------------------
        public void loadAllTask(List<TaskData> list_taskdata)
        {
            foreach (var i in list_taskdata)
            {
                TbDataTask tbdata_task = EbDataMgr.Instance.getData<TbDataTask>(i.task_id);
                TaskFactory task_factory = null;
                MapTaskFactory.TryGetValue(tbdata_task.TaskType, out task_factory);

                if (task_factory != null)
                {
                    TaskBase task_base = task_factory.createTask(i);
                    MapTask[task_base.TaskData.task_id] = task_base;
                }
            }
        }

        //-------------------------------------------------------------------------
        public void saveAllTask(List<TaskData> list_taskdata)
        {
            list_taskdata.Clear();
            foreach (var i in MapTask)
            {
                list_taskdata.Add(i.Value.TaskData);
            }
        }

        //-------------------------------------------------------------------------
        public List<TaskData> exportAllTaskData()
        {
            var list_taskdata = new List<TaskData>();
            return list_taskdata;
        }

        //-------------------------------------------------------------------------
        public void handleEvent(object sender, EntityEvent e)
        {
            foreach (var i in MapTask)
            {
                i.Value.handleEvent(sender, e);
            }
        }

        //-------------------------------------------------------------------------
        public void c2sTaskAccept(int task_id)
        {
            if (IsClient) return;

            TaskBase task = null;
            MapTask.TryGetValue(task_id, out task);
            if (task != null)
            {
                task.c2sTaskAccept();
            }
        }

        //-------------------------------------------------------------------------
        public void c2sTaskExcute(int task_id)
        {
            if (IsClient) return;

            TaskBase task = null;
            MapTask.TryGetValue(task_id, out task);
            if (task != null)
            {
                task.c2sTaskExcute();
            }
        }

        //-------------------------------------------------------------------------
        public void c2sTaskFinish(int task_id)
        {
            if (IsClient) return;

            TaskBase task = null;
            MapTask.TryGetValue(task_id, out task);
            if (task != null)
            {
                task.c2sTaskFinish();
            }
        }

        //-------------------------------------------------------------------------
        public void c2sTaskGiveUp(int task_id)
        {
            if (IsClient) return;

            TaskBase task = null;
            MapTask.TryGetValue(task_id, out task);
            if (task != null)
            {
                task.c2sTaskGiveUp();
            }
        }

        //-------------------------------------------------------------------------
        public Dictionary<int, TaskData> serverGetAllDirtyTask()
        {
            return mMapDirtyTask;
        }

        //-------------------------------------------------------------------------
        public void serverClearAllDirtyTask()
        {
            mMapDirtyTask.Clear();
        }

        //-------------------------------------------------------------------------
        public void _serverAddDirtyTask(TaskBase task)
        {
            mMapDirtyTask[task.TbDataTask.Id] = task.TaskData;

            if (task.TaskData.task_state == TaskState.Release)
            {
                // 发放本任务的任务奖励
                if (OnServerTaskAward != null)
                {
                    OnServerTaskAward(task);
                }

                // 添加到完成队列，同步给客户端后即删除已完成的任务
                QueTaskDone.Enqueue(task.TbDataTask.Id);
            }
        }
    }
}

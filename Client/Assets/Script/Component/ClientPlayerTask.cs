using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayerTask<TDef> : Component<TDef> where TDef : DefPlayerTask, new()
    {
        //-------------------------------------------------------------------------
        ClientApp<DefApp> CoApp { get; set; }
        ClientPlayer<DefPlayer> CoPlayer { get; set; }
        public TaskMgr TaskMgr { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientPlayerTask.init()");

            defNodeRpcMethod<PlayerTaskResponse>(
                (ushort)MethodType.s2cPlayerTaskResponse, s2cPlayerTaskResponse);
            defNodeRpcMethod<PlayerTaskNotify>(
                (ushort)MethodType.s2cPlayerTaskNotify, s2cPlayerTaskNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();

            TaskMgr = new TaskMgr(Entity, true);
            TaskMgr.regTaskFactory(new TaskFactoryCollectItem());
            TaskMgr.regTaskFactory(new TaskFactoryDialogue());
            TaskMgr.regTaskFactory(new TaskFactoryKillMonster());
            TaskMgr.OnClientTaskStateChange = _onTaskStateChange;

            requestSetupTask();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            TaskMgr = null;

            EbLog.Note("ClientPlayerTask.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            //if (e is EvUiTaskGiveUpTask)
            //{
            //    var ev = (EvUiTaskGiveUpTask)e;

            //    EbLog.Note("放弃任务 TaskId=" + ev.task.TbDataTask.Id);

            //    // 放弃任务
            //    TaskBase task = ev.task;
            //    requestGiveUpTask(task.TaskData.task_id);
            //}
            //else if (e is EvUiTaskGetTask)
            //{
            //    var ev = (EvUiTaskGetTask)e;

            //    EbLog.Note("获取任务 TaskId=" + ev.task.TbDataTask.Id);
            //}
            //else if (e is EvUiTaskDoTask)
            //{
            //    var ev = (EvUiTaskDoTask)e;

            //    EbLog.Note("执行任务 TaskId=" + ev.task.TbDataTask.Id);

            //    // 客户端自动执行任务
            //    TaskBase task = ev.task;
            //    TaskAutoInfo task_autoinfo = task.clientGetTaskAutoInfo();
            //    if (task_autoinfo != null)
            //    {
            //        CoPlayer.CoPlayerScene.doTask(ev.task.TbDataTask.Id, task_autoinfo);
            //    }
            //}
            //else if (e is EvUiNPCDialogueEnd)
            //{
            //    // 对话结束
            //    var ev = (EvUiNPCDialogueEnd)e;
            //    TaskState init_dialogue_task_state = ev.init_dialogue_task_state;
            //    int task_id = ev.task_id;
            //    int npc_id = ev.npc_id;
            //    TaskBase task;
            //    TaskMgr.MapTask.TryGetValue(task_id, out task);

            //    if (task != null)
            //    {
            //        task.clientNpcDialogueEnd(npc_id, init_dialogue_task_state);
            //    }
            //}
        }

        //-------------------------------------------------------------------------
        public List<TaskBase> getTaskListAboutNpc(int npc_id)
        {
            return TaskMgr.clientGetTaskListAboutNpc(npc_id);
        }

        //-------------------------------------------------------------------------
        void s2cPlayerTaskResponse(PlayerTaskResponse playertask_response)
        {
            switch (playertask_response.id)
            {
                case PlayerTaskResponseId.SetupTask:// 响应初始化任务
                    {
                        EbLog.Note("ClientPlayerTask.s2cPlayerTaskResponse() SetupTask");

                        var list_taskdata = EbTool.protobufDeserialize<List<TaskData>>(playertask_response.data);

                        TaskMgr.loadAllTask(list_taskdata);

                        //UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityPlayerTaskInitDone>().send(null);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cPlayerTaskNotify(PlayerTaskNotify playertask_notify)
        {
            switch (playertask_notify.id)
            {
                case PlayerTaskNotifyId.TaskUpdate:// 通知任务状态更新
                    {
                        EbLog.Note("ClientPlayerTask.s2cPlayerTaskNotify() TaskUpdate");

                        var map_taskdata = EbTool.protobufDeserialize<Dictionary<int, TaskData>>(playertask_notify.data);

                        //foreach (var i in map_taskdata)
                        //{
                        //    EbLog.Note("TaskId=" + i.Key + " TaskState=" + i.Value.task_state);
                        //}

                        TaskMgr.clientUpdateTask(map_taskdata);

                        //UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityRefreshTask>().send(null);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 根据任务状态发送任务请求
        //public void requestTask(TaskBase task)
        //{
        //    EbLog.Note("ClientPlayerTask.requestTask() TaskId=" + task.TbDataTask.Id);

        //    switch (task.TaskData.task_state)
        //    {
        //        case TaskState.CanDo:
        //            {
        //                requestAcceptTask(task.TaskData.task_id);
        //            }
        //            break;
        //        case TaskState.Doing:
        //            {
        //                requestExcuteTask(task.TaskData.task_id);
        //            }
        //            break;
        //        case TaskState.Done:
        //            {
        //                requestFinishTask(task.TaskData.task_id);
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //-------------------------------------------------------------------------
        // 请求初始化任务
        public void requestSetupTask()
        {
            PlayerTaskRequest playertask_request;
            playertask_request.id = PlayerTaskRequestId.SetupTask;
            playertask_request.data = null;

            CoApp.rpc(MethodType.c2sPlayerTaskRequest, playertask_request);
        }

        //-------------------------------------------------------------------------
        // 请求接受任务
        public void requestAcceptTask(int task_id)
        {
            PlayerTaskRequest playertask_request;
            playertask_request.id = PlayerTaskRequestId.TaskAccept;
            playertask_request.data = EbTool.protobufSerialize<int>(task_id);

            CoApp.rpc(MethodType.c2sPlayerTaskRequest, playertask_request);
        }

        //-------------------------------------------------------------------------
        // 请求执行任务
        public void requestExcuteTask(int task_id)
        {
            PlayerTaskRequest playertask_request;
            playertask_request.id = PlayerTaskRequestId.TaskExcute;
            playertask_request.data = EbTool.protobufSerialize<int>(task_id);

            CoApp.rpc(MethodType.c2sPlayerTaskRequest, playertask_request);
        }

        //-------------------------------------------------------------------------
        // 请求交付任务
        public void requestFinishTask(int task_id)
        {
            EbLog.Note("ClientPlayerTask.requestFinishTask() TaskId=" + task_id);

            PlayerTaskRequest playertask_request;
            playertask_request.id = PlayerTaskRequestId.TaskFinish;
            playertask_request.data = EbTool.protobufSerialize<int>(task_id);

            CoApp.rpc(MethodType.c2sPlayerTaskRequest, playertask_request);
        }

        //-------------------------------------------------------------------------
        // 请求放弃任务
        public void requestGiveUpTask(int task_id)
        {
            PlayerTaskRequest playertask_request;
            playertask_request.id = PlayerTaskRequestId.TaskGiveUp;
            playertask_request.data = EbTool.protobufSerialize<int>(task_id);

            CoApp.rpc(MethodType.c2sPlayerTaskRequest, playertask_request);
        }

        //-------------------------------------------------------------------------
        void _onTaskStateChange(TaskBase task)
        {
            if (task == null || task.TaskData == null) return;

            //FloatMsgInfo f_info;
            //f_info.msg = null;
            //f_info.color = Color.yellow;

            //switch (task.TaskData.task_state)
            //{
            //    case TaskState.CanDo:
            //        f_info.msg = string.Format("任务[{0}]可接受", task.TbDataTask.Name);
            //        break;
            //    case TaskState.Doing:
            //        f_info.msg = string.Format("任务[{0}]进行中", task.TbDataTask.Name);
            //        break;
            //    case TaskState.Done:
            //        f_info.msg = string.Format("任务[{0}]可交付", task.TbDataTask.Name);
            //        break;
            //    case TaskState.Release:
            //        f_info.msg = string.Format("任务[{0}]完成，获得奖励！", task.TbDataTask.Name);
            //        break;
            //    default:
            //        break;
            //}

            //if (f_info.msg != null) UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
        }
    }
}

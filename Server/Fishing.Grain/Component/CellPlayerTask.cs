using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellPlayerTask<TDef> : Component<TDef> where TDef : DefPlayerTask, new()
{
    //-------------------------------------------------------------------------
    CellActor<DefActor> CoActor { get; set; }
    CellPlayer<DefPlayer> CoPlayer { get; set; }
    TaskMgr TaskMgr { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        CoActor = Entity.getComponent<CellActor<DefActor>>();
        CoPlayer = Entity.getComponent<CellPlayer<DefPlayer>>();
        if (CoActor.Def.mPropIsBot.get()) return;

        TaskMgr = new TaskMgr(Entity, false);
        TaskMgr.regTaskFactory(new TaskFactoryCollectItem());
        TaskMgr.regTaskFactory(new TaskFactoryDialogue());
        TaskMgr.regTaskFactory(new TaskFactoryKillMonster());
        TaskMgr.OnServerTaskAward = _onTaskAward;

        if (Def.mPropFirstRun.get())
        {
            // 读取任务配置文件
            List<int> list_taskstory_starttasyid = CellApp.Instance.Cfg.ListTaskStoryStartTaskId;

            List<TaskData> list_taskdata = Def.mPropListTaskData.get();
            if (list_taskdata.Count == 0)
            {
                foreach (var i in list_taskstory_starttasyid)
                {
                    TaskData td = new TaskData();
                    td.task_id = i;
                    td.task_data = new Dictionary<byte, string>();
                    td.task_state = TaskState.Init;
                    list_taskdata.Add(td);
                }
            }

            Def.mPropFirstRun.set(true);
        }

        TaskMgr.loadAllTask(Def.mPropListTaskData.get());
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        TaskMgr = null;
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        if (TaskMgr != null)
        {
            bool dirty = false;
            var map_taskdata = TaskMgr.serverGetAllDirtyTask();
            if (map_taskdata.Count > 0)
            {
                dirty = true;
                _onTaskUpdate(map_taskdata);
                TaskMgr.serverClearAllDirtyTask();
            }

            TaskMgr.serverUpdate(elapsed_tm);

            if (dirty) TaskMgr.saveAllTask(Def.mPropListTaskData.get());
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
        if (TaskMgr != null)
        {
            TaskMgr.handleEvent(sender, e);

            var map_taskdata = TaskMgr.serverGetAllDirtyTask();
            if (map_taskdata.Count > 0)
            {
                _onTaskUpdate(map_taskdata);
                TaskMgr.serverClearAllDirtyTask();
            }

            TaskMgr.saveAllTask(Def.mPropListTaskData.get());
        }
    }

    //-------------------------------------------------------------------------
    public Task<MethodData> c2sPlayerTaskRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;
        if (CoActor.Def.mPropIsBot.get() || TaskMgr == null) goto End;

        var playertask_request = EbTool.protobufDeserialize<PlayerTaskRequest>(method_data.param1);
        switch (playertask_request.id)
        {
            case PlayerTaskRequestId.TaskAccept:// 请求接受任务
                {
                    var task_id = EbTool.protobufDeserialize<int>(playertask_request.data);
                    TaskMgr.c2sTaskAccept(task_id);
                }
                break;
            case PlayerTaskRequestId.TaskExcute:// 请求执行任务
                {
                    var task_id = EbTool.protobufDeserialize<int>(playertask_request.data);
                    TaskMgr.c2sTaskExcute(task_id);
                }
                break;
            case PlayerTaskRequestId.TaskFinish:// 请求交付任务
                {
                    var task_id = EbTool.protobufDeserialize<int>(playertask_request.data);
                    TaskMgr.c2sTaskFinish(task_id);
                }
                break;
            case PlayerTaskRequestId.TaskGiveUp:// 请求放弃任务
                {
                    var task_id = EbTool.protobufDeserialize<int>(playertask_request.data);
                    TaskMgr.c2sTaskGiveUp(task_id);
                }
                break;
            case PlayerTaskRequestId.SetupTask:// 请求初始化任务
                {
                    PlayerTaskResponse playertask_response;
                    playertask_response.id = PlayerTaskResponseId.SetupTask;
                    playertask_response.data = EbTool.protobufSerialize<List<TaskData>>(Def.mPropListTaskData.get());

                    result.method_id = MethodType.s2cPlayerTaskResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerTaskResponse>(playertask_response);
                }
                break;
            default:
                break;
        }

        End:
        return Task.FromResult(result);
    }

    //-------------------------------------------------------------------------
    void _onTaskUpdate(Dictionary<int, TaskData> map_taskdata)
    {
        if (CoActor.Def.mPropIsBot.get()) return;

        List<TaskData> list_taskdata = Def.mPropListTaskData.get();
        foreach (var i in map_taskdata)
        {
            bool have = false;
            foreach (var j in list_taskdata)
            {
                if (j.task_id == i.Key)
                {
                    j.task_data = i.Value.task_data;
                    j.task_state = i.Value.task_state;
                    have = true;
                    break;
                }
            }

            if (!have)
            {
                list_taskdata.Add(i.Value);
            }
        }

        PlayerTaskNotify playertask_notify;
        playertask_notify.id = PlayerTaskNotifyId.TaskUpdate;
        playertask_notify.data = EbTool.protobufSerialize<Dictionary<int, TaskData>>(map_taskdata);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerTaskNotify;
        notify_data.param1 = EbTool.protobufSerialize<PlayerTaskNotify>(playertask_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    // 任务奖励发放
    void _onTaskAward(TaskBase task)
    {
        // 经验奖励
        int award_exp = task.TbDataTask.AwardExp;
        int cur_exp = CoPlayer.CoActor.Def.mPropExperience.get();
        cur_exp += award_exp;
        CoPlayer.CoActor.Def.mPropExperience.set(cur_exp);

        // 道具奖励
        List<Item> award_list_item = task.getListAwardItem();
        foreach (var i in award_list_item)
        {
            CoPlayer.CoBag.addItem(i);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerTaskRequestId : byte
    {
        None = 0,// 无效
        SetupTask = 10,// c->s, 请求初始化任务
        TaskAccept = 20,// c->s, 请求接受任务
        TaskExcute = 30,// c->s, 请求执行任务
        TaskFinish = 40,// c->s, 请求交付任务
        TaskGiveUp = 50,// c->s, 请求放弃任务
    }

    //-------------------------------------------------------------------------
    public enum PlayerTaskResponseId : byte
    {
        None = 0,// 无效
        SetupTask = 10,// s->c, 响应初始化任务
    }

    //-------------------------------------------------------------------------
    public enum PlayerTaskNotifyId : byte
    {
        None = 0,// 无效
        TaskUpdate = 10,// 通知任务状态更新
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerTaskRequest
    {
        [ProtoMember(1)]
        public PlayerTaskRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerTaskResponse
    {
        [ProtoMember(1)]
        public PlayerTaskResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerTaskNotify
    {
        [ProtoMember(1)]
        public PlayerTaskNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    // 任务状态
    public enum TaskState : byte
    {
        Init = 0,// 不可接
        CanDo,// 可接未接
        Doing,// 已接进行中
        Done,// 已完成未交
        Release// 已交（同时发放奖励）
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class TaskData
    {
        [ProtoMember(1)]
        public int task_id;
        [ProtoMember(2)]
        public TaskState task_state;
        [ProtoMember(3)]
        public Dictionary<byte, string> task_data;
    }

    // 玩家任务
    public class DefPlayerTask : ComponentDef
    {
        //---------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, true)]
        public Prop<bool> mPropFirstRun;

        [PropAttrDistribution((byte)NodeType.Cell, true)]
        public Prop<List<TaskData>> mPropListTaskData;

        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropFirstRun = defProp<bool>(map_param, "FirstRun", true);
            mPropListTaskData = defProp<List<TaskData>>(map_param, "ListTaskData", new List<TaskData>());
        }
    }
}

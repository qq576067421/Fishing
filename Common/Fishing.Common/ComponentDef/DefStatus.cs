using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum StatusRequestId : byte
    {
        None = 0,// 无效
        SetupStatus = 10,// c->s, 请求初始化状态
    }

    //-------------------------------------------------------------------------
    public enum StatusResponseId : byte
    {
        None = 0,// 无效
        SetupStatus = 10,// s->c, 响应创建状态
    }

    //-------------------------------------------------------------------------
    public enum StatusNotifyId : byte
    {
        None = 0,// 无效
        CreateStatus = 10,// s->c, 通知创建状态
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct StatusRequest
    {
        [ProtoMember(1)]
        public StatusRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct StatusResponse
    {
        [ProtoMember(1)]
        public StatusResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct StatusNotify
    {
        [ProtoMember(1)]
        public StatusNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    // 状态
    public class DefStatus : ComponentDef
    {
        //-------------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
        }
    }
}

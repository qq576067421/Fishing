using System;
using System.Collections.Generic;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum AccountRequestId : byte
    {
        None = 0,// 无效
        EnterWorld = 10,// c->s, 请求进入游戏世界
    }

    //-------------------------------------------------------------------------
    public enum AccountResponseId : byte
    {
        None = 0,// 无效
        EnterWorld = 10,// s->c, 响应进入游戏世界
    }

    //-------------------------------------------------------------------------
    public enum AccountNotifyId : byte
    {
        None = 0,// 无效
        Logout = 10,// s->c，登出
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct AccountRequest
    {
        [ProtoMember(1)]
        public AccountRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct AccountResponse
    {
        [ProtoMember(1)]
        public AccountResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct AccountNotify
    {
        [ProtoMember(1)]
        public AccountNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class ClientEnterWorldRequest
    {
        [ProtoMember(1)]
        public string acc_id;
        [ProtoMember(2)]
        public string acc_name;
        [ProtoMember(3)]
        public string token;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class ClientEnterWorldResponse
    {
        [ProtoMember(1)]
        public ProtocolResult result;
        [ProtoMember(2)]
        public string acc_id;
        [ProtoMember(3)]
        public string acc_name;
        [ProtoMember(4)]
        public string token;
        [ProtoMember(5)]
        public EntityData et_player_data;
    }
}

// 登陆
public class DefLogin : ComponentDef
{
    //-------------------------------------------------------------------------
    public override void defAllProp(Dictionary<string, string> map_param)
    {
    }
}

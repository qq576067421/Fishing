using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerChatRequestId : byte
    {
        None = 0,// 无效
        SendChatMsg = 10,// c->s, 发送聊天消息
    }

    //-------------------------------------------------------------------------
    public enum PlayerChatResponseId : byte
    {
        None = 0,// 无效
    }

    //-------------------------------------------------------------------------
    public enum PlayerChatNotifyId : byte
    {
        None = 0,// 无效
        RecvChatMsg = 10,// s->c, 接收聊天消息
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerChatRequest
    {
        [ProtoMember(1)]
        public PlayerChatRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerChatResponse
    {
        [ProtoMember(1)]
        public PlayerChatResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerChatNotify
    {
        [ProtoMember(1)]
        public PlayerChatNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    // 聊天类型
    public enum ChatType
    {
        Friend,// 好友聊天
        Desktop,// 牌桌中聊天广播
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class ChatMsgSend
    {
        [ProtoMember(1)]
        public ChatType chat_type;
        [ProtoMember(2)]
        public string et_player_guid_recv;
        [ProtoMember(3)]
        public bool is_emotion;
        [ProtoMember(4)]
        public string content;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class ChatMsgRecv
    {
        [ProtoMember(1)]
        public ChatType chat_type;
        [ProtoMember(2)]
        public string et_player_guid_send;
        [ProtoMember(3)]
        public string et_player_guid_recv;
        [ProtoMember(4)]
        public bool is_emotion;
        [ProtoMember(5)]
        public string content;
        [ProtoMember(6)]
        public DateTime dt;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class ChatMsgRecord
    {
        [ProtoMember(1)]
        public bool is_me;
        [ProtoMember(2)]
        public bool is_emotion;
        [ProtoMember(3)]
        public string content;
        [ProtoMember(4)]
        public DateTime dt;
    }
}

// 玩家聊天
public class DefPlayerChat : ComponentDef
{
    //-------------------------------------------------------------------------
    public override void defAllProp(Dictionary<string, string> map_param)
    {
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerMailBoxRequestId : byte
    {
        None = 0,// 无效
        MailBoxInitInfo = 10,// c->s, 请求初始化小秘书信息
        MailOperate = 20,// c->s, 请求邮件操作
        DeleteSystemEvent = 30,// c->s，请求删除系统事件
    }

    //-------------------------------------------------------------------------
    public enum PlayerMailBoxResponseId : byte
    {
        None = 0,// 无效
        MailBoxInitInfo = 10,// s->c, 响应请求初始化小秘书信息
        MailOperate = 20,// s->c, 响应请求邮件操作
    }

    //-------------------------------------------------------------------------
    public enum PlayerMailBoxNotifyId : byte
    {
        None = 0,// 无效
        NewMail = 10,// s->c, 通知有新的邮件
        NewSystemEvent = 20,// s->c, 通知有新的系统事件
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerMailBoxRequest
    {
        [ProtoMember(1)]
        public PlayerMailBoxRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerMailBoxResponse
    {
        [ProtoMember(1)]
        public PlayerMailBoxResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerMailBoxNotify
    {
        [ProtoMember(1)]
        public PlayerMailBoxNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    public enum MailSenderType : byte
    {
        System = 0,// 系统邮件
        Player,
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class MailData
    {
        [ProtoMember(1)]
        public string mail_guid;
        [ProtoMember(2)]
        public string title = "";
        [ProtoMember(3)]
        public string content = "";
        [ProtoMember(4)]
        public List<ItemData> list_attachment = new List<ItemData>();// ItemData
        [ProtoMember(5)]
        public MailSenderType sender_type = MailSenderType.System;
        [ProtoMember(6)]
        public string sender_nickname = "";// 邮件发送者为玩家时，该字段有效
        [ProtoMember(7)]
        public string sender_icon = "";// 邮件发送者为玩家时，该字段有效
        [ProtoMember(8)]
        public DateTime send_datetime;
        [ProtoMember(9)]
        public bool read;// 是否已读，true表示已读
        [ProtoMember(10)]
        public bool recv_attachment;// 是否已领取附件，true表示已领取
    }

    //-------------------------------------------------------------------------
    public enum MailOperateType
    {
        None = 0,// 无操作
        Delete,// 删除邮件
        Read,// 标记为已读
        RecvAttachment,// 领取附件
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class MailOperate
    {
        [ProtoMember(1)]
        public ProtocolResult result;
        [ProtoMember(2)]
        public string mail_guid;
        [ProtoMember(3)]
        public MailOperateType mail_operate_type;
    }

    //-------------------------------------------------------------------------
    // 邮箱初始化信息
    [Serializable]
    [ProtoContract]
    public class MailBoxInitInfo
    {
        [ProtoMember(2)]
        public List<MailData> list_maildata;
    }

    //-------------------------------------------------------------------------
    public enum SystemEventType
    {
        RequestAddFriend = 0,// 请求加好友
        ResponseAddFriend,// 响应加好友
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class SystemEvent
    {
        [ProtoMember(1)]
        public string id;// Guid
        [ProtoMember(2)]
        public SystemEventType type;
        [ProtoMember(3)]
        public string data1;
        [ProtoMember(4)]
        public string data2;
        [ProtoMember(5)]
        public string data3;
    }

    // 玩家邮箱
    public class DefPlayerMailBox : ComponentDef
    {
        //-------------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<List<MailData>> mPropListMailData;// 邮件列表

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<List<SystemEvent>> mPropListSystemEvent;// 系统事件列表

        //-------------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropListMailData = defProp<List<MailData>>(map_param, "ListMailData", new List<MailData>());
            mPropListSystemEvent = defProp<List<SystemEvent>>(map_param, "ListSystemEvent", new List<SystemEvent>());
        }
    }
}

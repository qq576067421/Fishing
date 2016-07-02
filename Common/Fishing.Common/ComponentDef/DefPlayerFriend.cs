using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerFriendRequestId : byte
    {
        None = 0,// 无效
        GetPlayerInfoFriend = 10,// c->s, 请求获取好友玩家信息
        RequestAddFriend = 20,// c->s，请求添加好友
        AgreeAddFriend = 30,// c->s, 请求是否同意添加好友
        DeleteFriend = 40,// c->s, 请求删除好友
        FindFriend = 50,// c->s, 请求查找好友
    }

    //-------------------------------------------------------------------------
    public enum PlayerFriendResponseId : byte
    {
        None = 0,// 无效
        GetPlayerInfoFriend = 10,// s->c, 响应获取好友玩家信息
        RequestAddFriend = 20,// s->c, 响应请求添加好友
        AgreeAddFriend = 30,// s->c, 响应是否同意添加好友
        DeleteFriend = 40,// s->c, 响应删除好友
        FindFriend = 50,// s->c, 响应查找好友
    }

    //-------------------------------------------------------------------------
    public enum PlayerFriendNotifyId : byte
    {
        None = 0,// 无效
        RecommendPlayerList = 10,// 推荐玩家列表
        AddFriend = 20,// 通知加好友
        DeleteFriend = 30,// 通知删好友
        OnFriendLogin = 40,
        OnFriendLogout = 50,
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerFriendRequest
    {
        [ProtoMember(1)]
        public PlayerFriendRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerFriendResponse
    {
        [ProtoMember(1)]
        public PlayerFriendResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerFriendNotify
    {
        [ProtoMember(1)]
        public PlayerFriendNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct AddFriendAgree
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public bool agree;
    }

    // 玩家的好友
    public class DefPlayerFriend : ComponentDef
    {
        //-------------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<Dictionary<string, PlayerInfo>> mPropMapFriend;// 好友列表

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<Dictionary<string, List<ChatMsgRecord>>> mPropMapMsgRecord;// 好友聊天记录列表

        //-------------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropMapFriend = defProp<Dictionary<string, PlayerInfo>>(map_param, "MapFriend", new Dictionary<string, PlayerInfo>());
            mPropMapMsgRecord = defProp<Dictionary<string, List<ChatMsgRecord>>>(map_param, "MapMsgRecord", new Dictionary<string, List<ChatMsgRecord>>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerLobbyRequestId : byte
    {
        None = 0,// 无效
        SearchDesktop = 10,// c->s, 搜索桌子
        SearchDesktopFollowFriend = 20,// c->s, 搜索好友所在的桌子
    }

    //-------------------------------------------------------------------------
    public enum PlayerLobbyResponseId : byte
    {
        None = 0,// 无效
        SearchDesktop = 10,// s->c, 搜索桌子
        SearchDesktopFollowFriend = 20,// s->c, 搜索好友所在的桌子
    }

    //-------------------------------------------------------------------------
    public enum PlayerLobbyNotifyId : byte
    {
        None = 0,// 无效
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerLobbyRequest
    {
        [ProtoMember(1)]
        public PlayerLobbyRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerLobbyResponse
    {
        [ProtoMember(1)]
        public PlayerLobbyResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerLobbyNotify
    {
        [ProtoMember(1)]
        public PlayerLobbyNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PrivateDesktopCreateInfo
    {
        [ProtoMember(1)]
        public int seat_num;
        [ProtoMember(2)]
        public bool is_vip;
        [ProtoMember(3)]
        public int desktop_tableid;
    }
}

// PlayerLobby
public class DefPlayerLobby : ComponentDef
{
    //-------------------------------------------------------------------------
    public override void defAllProp(Dictionary<string, string> map_param)
    {
    }
}

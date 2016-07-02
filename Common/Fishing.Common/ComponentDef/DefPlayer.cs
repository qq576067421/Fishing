using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerRequestId : byte
    {
        None = 0,// 无效
        PlayNow = 10,// c->s, 立即玩
        CreatePrivateDesktop = 20,// c->s, 创建私有桌子
        EnterDesktopAny = 29,// c->s, 进入任意桌子
        EnterDesktop = 30,// c->s, 进入指定桌子
        LeaveDesktop = 40,// c->s, 离开桌子
        GetOnlinePlayerNum = 50,// c->s, 获取在线玩家数
        DevConsoleCmd = 60,// c->s, 请求执行控制台命令
        SetAFK = 70,// c->s, 请求设置是否挂机
        GetPlayerInfoOther = 80,// c->s, 请求获取其他玩家信息
        ChangeProfileSkin = 90,// c->s, 请求换肤
        ChangeNickName = 100,// c->s，请求改昵称
        ChangeIndividualSignature = 110,// c->s, 请求改签名
        RefreshIpAddress = 120,// c->s, 请求刷新Ip所在地
        ReportPlayer = 130,// c->s，举报玩家
        InvitePlayerEnterDesktop = 140,// c->s, 邀请玩家进桌
        GivePlayerChip = 150,// c->s, 赠送玩家筹码
        SetVip4Test = 160,// c->s, 请求设置是否为vip
    }

    //-------------------------------------------------------------------------
    public enum PlayerResponseId : byte
    {
        None = 0,// 无效
        CreatePrivateDesktop = 10,// s->c, 创建私有桌子
        EnterDesktopAny = 19,// s->c, 进入任意桌子
        EnterDesktop = 20,// s->c, 进入指定桌子
        LeaveDesktop = 30,// s->c, 离开桌子
        GetOnlinePlayerNum = 40,// s->c, 获取在线玩家数
        GetPlayerInfoOther = 50,// s->c, 响应获取其他玩家信息
        ChangeProfileSkin = 60,// s->c, 响应换肤
        ChangeNickName = 70,// s->c, 响应改昵称
        ChangeIndividualSignature = 80,// s->c, 响应改签名
        RefreshIpAddress = 90,// s->c, 响应刷新Ip所在地
        ReportPlayer = 100,// c->s，举报玩家
        InvitePlayerEnterDesktop = 110,// s->c, 响应邀请玩家进桌
        GivePlayerChip = 120,// s->c, 响应赠送玩家筹码
        SetVip4Test = 130,// s->c, 响应设置是否为vip
    }

    //-------------------------------------------------------------------------
    public enum PlayerNotifyId : byte
    {
        None = 0,// 无效
        InvitePlayerEnterDesktop = 10,// s->c, 收到进桌邀请
        GivePlayerChip = 20,// s->c,收到玩家赠送的筹码
        SetAFK = 30,// s->c, 通知设置是否挂机
        ActorMapPropDirty = 40,// s->c, 通知ClientActor应用脏属性集
        Levelup = 50,// s->c, 通知玩家升级
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerRequest
    {
        [ProtoMember(1)]
        public PlayerRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerResponse
    {
        [ProtoMember(1)]
        public PlayerResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerNotify
    {
        [ProtoMember(1)]
        public PlayerNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    // 玩家在服务器的状态
    public enum PlayerServerState : byte
    {
        Offline = 0,
        Hosting,
        Online,
    }

    //-------------------------------------------------------------------------
    // 玩家在线状态，供客户端使用
    public enum PlayerOnlineState : byte
    {
        Offline = 0,// 离线
        Online,// 在线
    }

    //-------------------------------------------------------------------------
    // 玩家信息，简要
    [Serializable]
    [ProtoContract]
    public class PlayerInfo
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public string nick_name;
        [ProtoMember(3)]
        public string icon;
        [ProtoMember(4)]
        public int level;
        [ProtoMember(5)]
        public int chip;
        [ProtoMember(6)]
        public int gold;
        [ProtoMember(7)]
        public PlayerOnlineState online_state;// 玩家在线状态，根据有没有CachePlayerData判断
        [ProtoMember(8)]
        public DateTime last_online_dt;// 玩家最后在线时间
    }

    //-------------------------------------------------------------------------
    // 玩家信息，大厅中
    [Serializable]
    [ProtoContract]
    public class PlayerInfoLobby
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public string nick_name;
        [ProtoMember(3)]
        public string icon;
        [ProtoMember(4)]
        public int chip;// 玩家筹码
    }

    //-------------------------------------------------------------------------
    // 玩家信息，点击查看好友
    [Serializable]
    [ProtoContract]
    public class PlayerInfoFriend
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public ulong player_id;
        [ProtoMember(3)]
        public string nick_name;
        [ProtoMember(4)]
        public string icon;
        [ProtoMember(5)]
        public int level;
        [ProtoMember(6)]
        public int exp;
        [ProtoMember(7)]
        public int chip;
        [ProtoMember(8)]
        public int gold;
        [ProtoMember(9)]
        public string individual_signature;// 个性签名
        [ProtoMember(10)]
        public int profileskin_tableid;
        [ProtoMember(11)]
        public string ip_address;
        [ProtoMember(12)]
        public bool is_vip;// 是否是VIP
        [ProtoMember(13)]
        public int vip_point;// VIP积分
        [ProtoMember(14)]
        public DateTime last_online_dt;// 玩家最后在线时间
        [ProtoMember(15)]
        public PlayerOnlineState online_state;// 玩家在线状态
        [ProtoMember(16)]
        public string desktop_etguid;// 如果玩家正在牌桌中，该牌桌的EtGuid
    }

    //-------------------------------------------------------------------------
    // 玩家信息，在牌桌中查看其他玩家信息
    [Serializable]
    [ProtoContract]
    public class PlayerInfoOther
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public ulong player_id;
        [ProtoMember(3)]
        public string nick_name;
        [ProtoMember(4)]
        public string icon;
        [ProtoMember(5)]
        public int level;
        [ProtoMember(6)]
        public int exp;
        [ProtoMember(7)]
        public int chip;
        [ProtoMember(8)]
        public int gold;
        [ProtoMember(9)]
        public string individual_signature;// 个性签名
        [ProtoMember(10)]
        public int profileskin_tableid;
        [ProtoMember(11)]
        public string ip_address;
        [ProtoMember(12)]
        public bool is_vip;// 是否是VIP
        [ProtoMember(13)]
        public int vip_point;// VIP积分
    }

    //-------------------------------------------------------------------------
    // 举报类型
    public enum ReportPlayerType
    {
        Avatar = 0,//阿凡达
        State,//状态
        AtackBehavior,// 攻击性行为
        ChipTransaction,// 筹码交易        
    }

    //-------------------------------------------------------------------------
    // 举报玩家
    [Serializable]
    [ProtoContract]
    public class ReportPlayer
    {
        [ProtoMember(1)]
        public ReportPlayerType report_type;
        [ProtoMember(2)]
        public string player_etguid;
    }

    //-------------------------------------------------------------------------
    // 好友进桌邀请
    [Serializable]
    [ProtoContract]
    public class InvitePlayerEnterDesktop
    {
        [ProtoMember(1)]
        public PlayerInfo player_info;// 邀请者信息
        [ProtoMember(2)]
        public string desktop_etguid;
        [ProtoMember(3)]
        public int sb;// 小盲注
        [ProtoMember(4)]
        public int bb;// 大盲注
        [ProtoMember(5)]
        public int player_num;// 座位上玩家总数
        [ProtoMember(6)]
        public int seat_num;// 总座位数
    }

    //-------------------------------------------------------------------------
    // 赠送玩家筹码
    [Serializable]
    [ProtoContract]
    public class GivePlayerChip
    {
        [ProtoMember(1)]
        public PlayerInfo player_info;// 赠送者信息
        [ProtoMember(2)]
        public int chip;
    }
}

// 玩家
public class DefPlayer : ComponentDef
{
    //-------------------------------------------------------------------------
    public override void defAllProp(Dictionary<string, string> map_param)
    {
    }
}

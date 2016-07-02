using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum DesktopRequestId : byte
    {
        None = 0,// 无效
        PlayerWaitWhile = 10,// 玩家暂离
        PlayerReturn = 20,// 玩家继续
        PlayerOb = 30,// 玩家观战
        PlayerSitdown = 40,// 玩家坐下
        PlayerAutoAction = 50,// 请求托管
        PlayerCancelAutoAction = 60,// 请求取消托管
        PlayerSceneAction = 70,// 玩家捕鱼场景中的操作
    }

    //-------------------------------------------------------------------------
    public enum DesktopResponseId : byte
    {
        None = 0,// 无效
        PlayerAutoAction = 30,// 响应托管
        PlayerCancelAutoAction = 40,// 响应取消托管
    }

    //-------------------------------------------------------------------------
    public enum DesktopNotifyId : byte
    {
        None = 0,// 无效
        DesktopInit = 10,// 桌子初始化
        PlayerChat = 90,// 玩家聊天广播
        PlayerEnter = 100,// 玩家进入桌子
        PlayerLeave = 110,// 玩家离开桌子
        PlayerWaitWhile = 120,// 玩家暂离
        PlayerReturn = 130,// 玩家继续
        PlayerOb = 140,// 玩家观战
        PlayerSitdown = 150,// 玩家坐下
        PlayerStateChange = 160,// 玩家状态改变
        PlayerAction = 170,// 玩家操作，包含状态改变
        PlayerGetTurn = 180,// 玩家获得行动机会

        PlayerSceneAction = 190,// 玩家捕鱼场景中的操作
        PlayerSceneAoIUpdate = 200,// 玩家捕鱼场景中的广播
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct DesktopRequest
    {
        [ProtoMember(1)]
        public DesktopRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct DesktopResponse
    {
        [ProtoMember(1)]
        public DesktopResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct DesktopNotify
    {
        [ProtoMember(1)]
        public DesktopNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    public enum DesktopGameSpeed
    {
        Unlimited = 0,
        Normal,// 正常
        Fast,// 快速
    }

    //-------------------------------------------------------------------------
    public enum DesktopPlayerState
    {
        Ob = 0,// 观战
        Wait4Next,// 未打牌，等待下一局
        WaitWhile,// 未打牌，暂时离开
        InGame,// 打牌中
    }

    //-------------------------------------------------------------------------
    public enum DesktopState
    {
        Idle = 0,
        PreFlop,
        Flop,
        Turn,
        River,
        GameEnd,
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct DesktopRequestPlayerEnter
    {
        [ProtoMember(1)]
        public string desktop_etguid;
        [ProtoMember(2)]
        public byte seat_index;// 255表示无效座位，观战
        [ProtoMember(3)]
        public int player_clip;// 玩家身上所有筹码
        [ProtoMember(4)]
        public int player_gold;// 玩家身上所有金币
        [ProtoMember(5)]
        public bool is_vip;// 是否是贵宾
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct DesktopRequestPlayerLeave
    {
        [ProtoMember(1)]
        public string desktop_etguid;
        [ProtoMember(2)]
        public bool sitdown;
        [ProtoMember(3)]
        public byte seat_index;
        [ProtoMember(3)]
        public string et_player_guid;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class PlayerSitdownData
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public byte seat_index;
        [ProtoMember(3)]
        public int stack;
        [ProtoMember(4)]
        public DesktopPlayerState state;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class PlayerWaitWhileData
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public float wait_while_tm;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class PlayerReturnData
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public int stack;
        [ProtoMember(3)]
        public DesktopPlayerState state;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class PlayerStateData
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public DesktopPlayerState state;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class PlayerBuyChipData
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public int win_chip;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class DesktopConfigData
    {
        [ProtoMember(1)]
        public string desktop_etguid;
        [ProtoMember(2)]
        public int seat_num;// 座位数
        [ProtoMember(3)]
        public bool is_vip;// 是否是VIP桌
        [ProtoMember(4)]
        public float desktop_waitwhile_tm;// 暂离等待时间（单位：秒）
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class DesktopData
    {
        [ProtoMember(1)]
        public DesktopConfigData desktop_cfg_data;
        [ProtoMember(2)]
        public DesktopState desktop_state;
        [ProtoMember(3)]
        public int desktop_currentroundmaxbet;
        [ProtoMember(4)]
        public int pot_main;
        [ProtoMember(5)]
        public List<EntityData> list_actormirror;// 玩家信息
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class DesktopSearchFilter
    {
        [ProtoMember(1)]
        public int seat_num;
        [ProtoMember(2)]
        public int desktop_tableid;
        [ProtoMember(3)]
        public bool is_vip;// 是否是贵宾
        [ProtoMember(4)]
        public bool is_seat_full;// 是否包含满座
    }
}

// 玩家桌子
public class DefPlayerDesktop : ComponentDef
{
    //-------------------------------------------------------------------------
    public override void defAllProp(Dictionary<string, string> map_param)
    {
    }
}

using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ps
{
    //-----------------------------------------------------------------------------
    public enum _eProtocolDesktop : byte
    {
        // 客户端发送给服务端的协议消息
        c2sRenderUpdate = 0,// 客户端更新消息
        c2sSnapshotScene,// 客户端请求初始化场景
        c2sFishHit,// 客户端子弹命中鱼的消息
        c2sFishNetHit,// 客户端渔网命中鱼的消息
        c2sTurretRate,// 客户端炮台倍率更新消息
        c2sManualFire,// 客户端提交手动发炮
        c2sAutoFire,// 客户端提交自动发炮
        c2sLockFish,// 客户端提交锁定鱼
        c2sUnlockFish,// 客户端提交解锁鱼
        c2sBeginLongpress,// 客户端提交开始长按状态
        c2sEndLongpress,// 客户端提交结束长按状态
        c2sBeginRapid,// 客户端提交开始极速状态
        c2sEndRapid,// 客户端提交结束极速状态

        // 服务端广播给桌内所有客户端的消息
        s2allcLogicUpdate,
        s2allcPlayerEnter,
        s2allcPlayerLeave,
        s2allcPlayerDropped,
        s2allcPlayerReConnect,
        s2allcLevelUpdate,
        s2allcCreateFishLord,
        s2allcFishDie,
        s2allcCreateClientEffect,
        s2allcAoeFishDie,
        s2allcTurretRate,
        s2allcManualFire,
        s2allcAutoFire,
        s2allcEfxFire,
        s2allcLockFish,
        s2allcUnlockFish,
        s2allcBeginLongpress,
        s2allcEndLongpress,
        s2allcBeginRapid,
        s2allcEndRapid,
        s2allcBeginPower,
        s2allcEndPower,
        s2allcSetTurret,
        s2allcSyncAllPlayerGold,

        // 服务端响应单个客户端的消息
        s2cSnapshotScene
    }

    //-----------------------------------------------------------------------------
    public enum _eAoIEvent : byte
    {
        PlayerSitdown = 0,
        PlayerOb,
        PlayerLeave,
        PlayerDropped,
        PlayerReConnect,
        PlayerDroppedCountdown,
        PlayerChat,
        RequstPlayerOb,
        RequstPlayerSitdown,
        RequestPlayerLeave,
        RequestPlayerChat,
        RequestScene,
        SceneBroadcast,
        Unknown
    }

    //-----------------------------------------------------------------------------
    [ProtoContract]
    public struct _tAoIEvent
    {
        [ProtoMember(1)]
        public _eAoIEvent id;
        [ProtoMember(2)]
        public List<string> vec_param;
    }

    //-----------------------------------------------------------------------------
    public struct _tRoomRes
    {
        public TbDataRoom._eRoomType room_itemid;
        public string room_name;
        public List<int> rate_list;
    }

    //-----------------------------------------------------------------------------
    [Serializable]
    public struct _tRoomInfo
    {
        public TbDataRoom._eRoomType room_type;
        public int obj_id;
    }

    //-----------------------------------------------------------------------------
    [Serializable]
    public struct _tDesktopSeat
    {
        public int index;
        public uint et_player_rpcid;
        public string player_nickname;
        public string player_iconname;
        public string player_vipname;
        public int player_level;
        public string player_title;
        public int gold;
        public string et_player_guid;
    }

    //-----------------------------------------------------------------------------
    [Serializable]
    public struct _tDesktopOb
    {
        public int index;
        public uint et_player_rpcid;
        public string player_nickname;
        public string player_iconname;
        public string player_vipname;
        public int player_level;
        public string player_title;
        public int gold;
        public string et_player_guid;
    }

    //-----------------------------------------------------------------------------
    [Serializable]
    public struct _tDesktopInfo
    {
        public int desktop_objid;
        public _tDesktopSeat[] seat_array;
        public _tDesktopOb[] ob_array;
    }
}

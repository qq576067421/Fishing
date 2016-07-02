using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    // 关卡状态
    public enum _eLevelState
    {
        Normal = 0,
        Switch,
        End
    }

    //-------------------------------------------------------------------------
    // 鱼类型
    public enum _eFishType
    {
        Fish = 0,// 鱼
        FishGroup,// 鱼组
        FishTeam// 鱼阵
    }

    //-------------------------------------------------------------------------
    // 路径类型
    public enum _eRouteType
    {
        BigFish = 0,
        SmallFish
    }

    //-------------------------------------------------------------------------
    // 场景中玩家信息
    public struct _tScenePlayer
    {
        public uint et_player_rpcid;
        public string nickname;
        public bool is_bot;
        public float rate;
    }

    //-------------------------------------------------------------------------
    // 子弹信息
    public struct _tBullet
    {
        public int bullet_objid;
        public float turret_angle;
        public int turret_rate;
        public int locked_fish_objid;
    }

    //-------------------------------------------------------------------------
    // 鱼群创建鱼的方式
    public enum _eCreateFishStyle
    {
        Normal = 0,// 常规出鱼
        RedFishDie,// 红鱼死亡出鱼
    }

    //-------------------------------------------------------------------------
    // 鱼群信息
    public struct _tFishCrowd
    {
        public _eCreateFishStyle create_fish_style;// 鱼群创建鱼的方式
        public int fish_start_objid;// 鱼起始objid
        public _eFishType fish_type;// 出鱼类型
        public int fish_vibid;// 出鱼vib_id
        public int fish_count;// 鱼群中所有鱼的数量
        public float createfish_timespan;// 出鱼间隔
        public _eRouteType route_type;// 路径类型
        public int route_vibid;// 路径vib_id
    }
}

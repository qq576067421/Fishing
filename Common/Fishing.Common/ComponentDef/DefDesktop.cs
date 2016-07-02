using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //---------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class DesktopPlayerInfo
    {
        [ProtoMember(1)]
        public byte seat_index;
        [ProtoMember(2)]
        public string player_etguid;
        [ProtoMember(3)]
        public string nick_name;
        [ProtoMember(4)]
        public string icon;
        [ProtoMember(5)]
        public int chip;
    }

    //---------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class DesktopInfo
    {
        [ProtoMember(1)]
        public string desktop_etguid = "";
        [ProtoMember(2)]
        public int seat_num;
        [ProtoMember(4)]
        public bool is_vip;
        [ProtoMember(5)]
        public int desktop_tableid;
        [ProtoMember(6)]
        public List<DesktopPlayerInfo> list_seat_player;// 座位上所有玩家信息
        [ProtoMember(7)]
        public int seat_player_num = 0;// 座位上玩家数
        [ProtoMember(8)]
        public int all_player_num = 0;// 所有玩家数（座位+Ob）

        public bool isFull()
        {
            return seat_num == seat_player_num;
        }
    }

    //---------------------------------------------------------------------
    public class SeatInfo
    {
        public byte index;
        public uint et_player_rpcid;
        public Entity et_playermirror;
    }

    //---------------------------------------------------------------------
    public class DesktopPlayerLeaveInfo
    {
        public int stack = 0;// 玩家本桌携带的筹码
        public int game_total;// 玩家本桌总共游戏局数
        public int game_win;// 玩家本桌赢的游戏局数
        public int exp;// 玩家本桌打牌时所增加的经验值
    }

    // 桌子
    public class DefDesktop : ComponentDef
    {
        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
        }
    }
}

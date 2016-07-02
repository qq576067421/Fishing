using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerRankingRequestId : byte
    {
        None = 0,// 无效
        GetRankingList = 10,// c->s, 获取排行榜
    }

    //-------------------------------------------------------------------------
    public enum PlayerRankingResponseId : byte
    {
        None = 0,// 无效
        GetChipRankingList = 10,// s->c, 获取筹码排行榜
        GetVIPPointRankingList = 20,// s->c, 获取积分排行榜
    }

    //-------------------------------------------------------------------------
    public enum PlayerRankingNotifyId : byte
    {
        None = 0,// 无效
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerRankingRequest
    {
        [ProtoMember(1)]
        public PlayerRankingRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerRankingResponse
    {
        [ProtoMember(1)]
        public PlayerRankingResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerRankingNotify
    {
        [ProtoMember(1)]
        public PlayerRankingNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    // 排行榜类型
    public enum RankingListType
    {
        Chip = 0,// 筹码
        VIPPoint,// 积分
    }

    //-------------------------------------------------------------------------
    // 筹码榜数据
    [Serializable]
    [ProtoContract]
    public class RankingChip
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public string nick_name;
        [ProtoMember(3)]
        public string icon;
        [ProtoMember(4)]
        public int chip;
    }

    //-------------------------------------------------------------------------
    // 积分榜数据
    [Serializable]
    [ProtoContract]
    public class RankingVIPPoint
    {
        [ProtoMember(1)]
        public string player_etguid;
        [ProtoMember(2)]
        public string nick_name;
        [ProtoMember(3)]
        public string icon;
        [ProtoMember(4)]
        public int vip_point;
    }

    public class DefPlayerRanking : ComponentDef
    {
        //-------------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
        }
    }
}

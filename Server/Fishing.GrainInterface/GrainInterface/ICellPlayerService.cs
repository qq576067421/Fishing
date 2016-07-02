using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;

namespace Ps
{
    // 玩家无状态服务
    public interface ICellPlayerService : IGrainWithIntegerKey
    {
        //---------------------------------------------------------------------
        // 玩家昵称是否已存在
        Task<bool> playerNickNameExist(string nick_name);

        //---------------------------------------------------------------------
        // 玩家ID是否已存在
        Task<bool> playerIdExist(ulong player_id);

        //---------------------------------------------------------------------
        // 根据玩家Guid获取指定玩家信息
        Task<PlayerInfo> getPlayerInfo(string et_player_guid);

        //---------------------------------------------------------------------
        // 根据玩家Guid获取指定玩家信息
        Task<PlayerInfoFriend> getPlayerInfoFriend(string et_player_guid);

        //---------------------------------------------------------------------
        // 根据玩家Guid获取指定玩家信息
        Task<PlayerInfoOther> getPlayerInfoOther(string et_player_guid);

        //---------------------------------------------------------------------
        // 获取在线玩家数
        Task<int> getOnlinePlayerNum();

        //---------------------------------------------------------------------
        // 随机获取一组在线玩家
        Task<List<PlayerInfo>> getOnlinePlayers(string except_player_etguid);

        //---------------------------------------------------------------------
        // 搜索玩家，根据玩家Id或昵称的部分匹配
        Task<List<PlayerInfo>> findPlayers(string find_info);

        //---------------------------------------------------------------------
        // 获取筹码排行榜
        Task<List<RankingChip>> getChipRankingList();

        //---------------------------------------------------------------------
        // 获取积分排行榜
        Task<List<RankingVIPPoint>> getVIPPointRankingList();
    }
}

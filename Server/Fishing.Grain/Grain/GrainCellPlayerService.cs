using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Couchbase;
using Couchbase.Core;
using Couchbase.N1QL;
using GF.Common;

namespace Ps
{
    // 玩家无状态服务
    [Reentrant]
    [StatelessWorker]
    public class GrainCellPlayerService : Grain, ICellPlayerService
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }

        //---------------------------------------------------------------------
        // 玩家昵称是否已存在
        async Task<bool> ICellPlayerService.playerNickNameExist(string nick_name)
        {
            bool exist = false;

            string query = string.Format("SELECT {0} FROM Fishing WHERE entity_type='EtPlayer' and {1}=$1 LIMIT 1;"
               , "entity_guid"
               , "list_component[0].def_propset.NickName");
            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus)
                   .AddPositionalParameter(nick_name);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<dynamic>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                exist = true;
            }

            return exist;
        }

        //---------------------------------------------------------------------
        // 玩家ID是否已存在
        async Task<bool> ICellPlayerService.playerIdExist(ulong player_id)
        {
            return false;
            bool exist = false;

            string query = string.Format("SELECT {0} FROM Fishing WHERE entity_type='EtPlayer' and {1}=$1 LIMIT 1;"
               , "list_component[0].def_propset.ActorId"
               , "list_component[0].def_propset.ActorId");
            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus)
                   .AddPositionalParameter(player_id.ToString());
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<dynamic>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                exist = true;
            }

            return exist;
        }

        //---------------------------------------------------------------------
        // 根据玩家Guid获取指定玩家信息
        async Task<PlayerInfo> ICellPlayerService.getPlayerInfo(string player_etguid)
        {
            string query = string.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8} FROM Fishing
                   WHERE entity_type='EtPlayer' and {9}=$1 LIMIT 1;"
               , "entity_guid"
               , "list_component[0].def_propset.ActorId"
               , "list_component[0].def_propset.NickName"
               , "list_component[0].def_propset.Level"
               , "list_component[0].def_propset.Icon"
               , "list_component[0].def_propset.IndividualSignature"
               , "list_component[0].def_propset.Chip"
               , "list_component[0].def_propset.Gold"
               , "list_component[0].def_propset.ProfileSkinTableId"
               , "entity_guid");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus)
                   .AddPositionalParameter(player_etguid);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<PlayerInfo>(query_request);

            if (result.Success && result.Rows.Count > 0)
            {
                PlayerInfo q = result.Rows[0];
                if (q != null && !string.IsNullOrEmpty(q.player_etguid))
                {
                    if (string.IsNullOrEmpty(q.nick_name))
                        q.nick_name = EbTool.jsonDeserialize<string>(q.nick_name);

                    if (string.IsNullOrEmpty(q.icon))
                        q.icon = EbTool.jsonDeserialize<string>(q.icon);

                    return q;
                }
            }

            PlayerInfo player_info = new PlayerInfo();
            player_info.player_etguid = "";
            return player_info;
        }

        //---------------------------------------------------------------------
        // 根据玩家Guid获取指定玩家信息
        async Task<PlayerInfoFriend> ICellPlayerService.getPlayerInfoFriend(string et_player_guid)
        {
            string query = string.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} FROM Fishing
               WHERE entity_type='EtPlayer' and {11}=$1 LIMIT 1;"
               , "entity_guid"
               , "list_component[0].def_propset.ActorId"
               , "list_component[0].def_propset.NickName"
               , "list_component[0].def_propset.Level"
               , "list_component[0].def_propset.Experience"
               , "list_component[0].def_propset.Icon"
               , "list_component[0].def_propset.IpAddress"
               , "list_component[0].def_propset.IndividualSignature"
               , "list_component[0].def_propset.Chip"
               , "list_component[0].def_propset.Gold"
               , "list_component[0].def_propset.ProfileSkinTableId"
               , "entity_guid");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus)
                   .AddPositionalParameter(et_player_guid);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<PlayerInfoFriend>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                PlayerInfoFriend q = result.Rows[0];
                if (q != null && !string.IsNullOrEmpty(q.player_etguid))
                {
                    if (string.IsNullOrEmpty(q.nick_name))
                        q.nick_name = EbTool.jsonDeserialize<string>(q.nick_name);

                    if (string.IsNullOrEmpty(q.icon))
                        q.icon = EbTool.jsonDeserialize<string>(q.icon);

                    if (string.IsNullOrEmpty(q.ip_address))
                        q.ip_address = EbTool.jsonDeserialize<string>(q.ip_address);

                    if (string.IsNullOrEmpty(q.individual_signature))
                        q.individual_signature = EbTool.jsonDeserialize<string>(q.individual_signature);

                    return q;
                }
            }

            PlayerInfoFriend player_info_ex = new PlayerInfoFriend();
            return player_info_ex;
        }

        //---------------------------------------------------------------------
        // 根据玩家Guid获取指定玩家信息
        async Task<PlayerInfoOther> ICellPlayerService.getPlayerInfoOther(string et_player_guid)
        {
            string query = string.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12} FROM Fishing as PlayerInfoOther
               WHERE entity_type='EtPlayer' and {13}=$1 LIMIT 1;"
               , "entity_guid"
               , "list_component[0].def_propset.ActorId"
               , "list_component[0].def_propset.NickName"
               , "list_component[0].def_propset.Icon"
               , "list_component[0].def_propset.Level"
               , "list_component[0].def_propset.Experience"
               , "list_component[0].def_propset.Chip"
               , "list_component[0].def_propset.Gold"
               , "list_component[0].def_propset.IndividualSignature"
               , "list_component[0].def_propset.ProfileSkinTableId"
               , "list_component[0].def_propset.IpAddress"
               , "list_component[0].def_propset.IsVIP"
               , "list_component[0].def_propset.VIPPoint"
               , "entity_guid");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus)
                   .AddPositionalParameter(et_player_guid);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<PlayerInfoOther>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                PlayerInfoOther q = result.Rows[0];
                if (q != null && !string.IsNullOrEmpty(q.player_etguid))
                {
                    if (string.IsNullOrEmpty(q.nick_name))
                        q.nick_name = EbTool.jsonDeserialize<string>(q.nick_name);

                    if (string.IsNullOrEmpty(q.icon))
                        q.icon = EbTool.jsonDeserialize<string>(q.icon);

                    if (string.IsNullOrEmpty(q.ip_address))
                        q.ip_address = EbTool.jsonDeserialize<string>(q.ip_address);

                    if (string.IsNullOrEmpty(q.individual_signature))
                        q.individual_signature = EbTool.jsonDeserialize<string>(q.individual_signature);

                    return q;
                }
            }

            PlayerInfoOther player_info = new PlayerInfoOther();
            return player_info;
        }

        //---------------------------------------------------------------------
        // 获取在线玩家数
        async Task<int> ICellPlayerService.getOnlinePlayerNum()
        {
            string query = string.Format(@"SELECT COUNT(player_server_state) FROM Fishing;");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<JObject>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                JObject q = result.Rows[0];
                return q.Value<int>("$1");
            }

            return 0;
        }

        //---------------------------------------------------------------------
        // 随机获取一组在线玩家
        async Task<List<PlayerInfo>> ICellPlayerService.getOnlinePlayers(string except_player_etguid)
        {
            List<PlayerInfo> list_playerinfo = new List<PlayerInfo>();

            string query = string.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} FROM Fishing a
                WHERE entity_type='EtPlayer' AND
                ANY child IN
                (SELECT player_etguid, player_server_state FROM TexasPoker WHERE player_server_state=1 or player_server_state=2 LIMIT 5)
                SATISFIES child.player_etguid=a.entity_guid END;"
                , "entity_guid"
                , "list_component[0].def_propset.ActorId"
                , "list_component[0].def_propset.NickName"
                , "list_component[0].def_propset.Level"
                , "list_component[0].def_propset.Experience"
                , "list_component[0].def_propset.Icon"
                , "list_component[0].def_propset.IpAddress"
                , "list_component[0].def_propset.IndividualSignature"
                , "list_component[0].def_propset.Chip"
                , "list_component[0].def_propset.Gold"
                , "list_component[0].def_propset.ProfileSkinTableId");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<PlayerInfo>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                foreach (var i in result.Rows)
                {
                    PlayerInfo q = i;
                    if (q != null && !string.IsNullOrEmpty(q.player_etguid))
                    {
                        if (!string.IsNullOrEmpty(except_player_etguid) && except_player_etguid == q.player_etguid) continue;

                        if (!string.IsNullOrEmpty(q.nick_name))
                        {
                            q.nick_name = EbTool.jsonDeserialize<string>(q.nick_name);
                        }

                        if (!string.IsNullOrEmpty(q.icon))
                        {
                            q.icon = EbTool.jsonDeserialize<string>(q.icon);
                        }

                        //if (string.IsNullOrEmpty(q.IpAddress)) player_info.ip_address = "";
                        //else player_info.ip_address = EbTool.jsonDeserialize<string>(q.IpAddress);

                        //if (string.IsNullOrEmpty(q.IndividualSignature)) player_info.individual_signature = "";
                        //else player_info.individual_signature = EbTool.jsonDeserialize<string>(q.IndividualSignature);

                        //player_info.chip = q.Chip;
                        //player_info.gold = q.Gold;
                        //player_info.profileskin_tableid = q.ProfileSkinTableId;

                        list_playerinfo.Add(q);
                    }
                }
            }

            return list_playerinfo;
        }

        //---------------------------------------------------------------------
        // 搜索玩家，根据玩家Id或昵称的部分匹配
        async Task<List<PlayerInfo>> ICellPlayerService.findPlayers(string find_info)
        {
            List<PlayerInfo> list_playerinfo = new List<PlayerInfo>();
            if (string.IsNullOrEmpty(find_info)) return list_playerinfo;

            string query = string.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} FROM Fishing
                WHERE entity_type='EtPlayer' AND
                list_component[0].def_propset.NickName LIKE $1 OR
                list_component[0].def_propset.ActorId=$2 LIMIT 10;"
                , "entity_guid"
                , "list_component[0].def_propset.ActorId"
                , "list_component[0].def_propset.NickName"
                , "list_component[0].def_propset.Level"
                , "list_component[0].def_propset.Experience"
                , "list_component[0].def_propset.Icon"
                , "list_component[0].def_propset.IpAddress"
                , "list_component[0].def_propset.IndividualSignature"
                , "list_component[0].def_propset.Chip"
                , "list_component[0].def_propset.Gold"
                , "list_component[0].def_propset.ProfileSkinTableId");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus)
                   .AddPositionalParameter("%" + find_info + "%")
                   .AddPositionalParameter(find_info);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<PlayerInfo>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                foreach (var i in result.Rows)
                {
                    PlayerInfo q = i;
                    if (!string.IsNullOrEmpty(q.player_etguid))
                    {
                        if (!string.IsNullOrEmpty(q.nick_name))
                        {
                            q.nick_name = EbTool.jsonDeserialize<string>(q.nick_name);
                        }

                        if (!string.IsNullOrEmpty(q.icon))
                        {
                            q.icon = EbTool.jsonDeserialize<string>(q.icon);
                        }

                        //if (string.IsNullOrEmpty(q.IpAddress)) player_info.ip_address = "";
                        //else player_info.ip_address = EbTool.jsonDeserialize<string>(q.IpAddress);

                        //if (string.IsNullOrEmpty(q.IndividualSignature)) player_info.individual_signature = "";
                        //else player_info.individual_signature = EbTool.jsonDeserialize<string>(q.IndividualSignature);

                        list_playerinfo.Add(q);
                    }
                }
            }

            return list_playerinfo;
        }

        //---------------------------------------------------------------------
        // 获取筹码排行榜
        async Task<List<RankingChip>> ICellPlayerService.getChipRankingList()
        {
            List<RankingChip> list_rankingchip = new List<RankingChip>();

            string query = string.Format(@"SELECT {0}, {1}, {2}, {3} FROM Fishing
                WHERE entity_type='EtPlayer'
                ORDER BY {4} LIMIT 10;"
                    , "entity_guid"
                    , "list_component[0].def_propset.NickName"
                    , "list_component[0].def_propset.Icon"
                    , "list_component[0].def_propset.Chip"
                    , "list_component[0].def_propset.Chip");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<RankingChip>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                foreach (var i in result.Rows)
                {
                    RankingChip q = i;
                    if (q != null && !string.IsNullOrEmpty(q.player_etguid))
                    {
                        if (!string.IsNullOrEmpty(q.nick_name))
                        {
                            q.nick_name = EbTool.jsonDeserialize<string>(q.nick_name);
                        }

                        if (!string.IsNullOrEmpty(q.icon))
                        {
                            q.icon = EbTool.jsonDeserialize<string>(q.icon);
                        }

                        list_rankingchip.Add(q);
                    }
                }
            }

            return list_rankingchip;
        }

        //---------------------------------------------------------------------
        // 获取积分排行榜
        async Task<List<RankingVIPPoint>> ICellPlayerService.getVIPPointRankingList()
        {
            List<RankingVIPPoint> list_rankingvippoint = new List<RankingVIPPoint>();

            string query = string.Format(@"SELECT {0}, {1}, {2}, {3} FROM Fishing
                WHERE entity_type='EtPlayer'
                ORDER BY {4} LIMIT 10;"
                    , "entity_guid"
                    , "list_component[0].def_propset.NickName"
                    , "list_component[0].def_propset.Icon"
                    , "list_component[0].def_propset.VIPPoint"
                    , "list_component[0].def_propset.VIPPoint");

            var query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus);
            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<RankingVIPPoint>(query_request);
            if (result.Success && result.Rows.Count > 0)
            {
                foreach (var i in result.Rows)
                {
                    RankingVIPPoint q = i;
                    if (q != null && !string.IsNullOrEmpty(q.player_etguid))
                    {
                        if (!string.IsNullOrEmpty(q.nick_name))
                        {
                            q.nick_name = EbTool.jsonDeserialize<string>(q.nick_name);
                        }

                        if (string.IsNullOrEmpty(q.icon))
                        {
                            q.icon = EbTool.jsonDeserialize<string>(q.icon);
                        }

                        list_rankingvippoint.Add(q);
                    }
                }
            }

            return list_rankingvippoint;
        }
    }
}

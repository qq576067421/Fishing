using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Couchbase;
using Couchbase.Core;
using Orleans.Runtime;
using Couchbase.N1QL;
using GF.Common;

namespace Ps
{
    public class QueryDesktopPlayer : Document<QueryDesktopPlayer>
    {
        public byte seat_index { get; set; }
        public string player_etguid { get; set; }
        public string nick_name { get; set; }
        public string icon { get; set; }
        public int chip { get; set; }
    }

    public class QueryDesktop : Document<QueryDesktop>
    {
        public string desktop_etguid { get; set; }
        public int seat_num { get; set; }
        public int seat_player_num { get; set; }// 座位上玩家数
        public int all_player_num { get; set; }// 所有玩家数（座位+Ob）
        public List<QueryDesktopPlayer> list_seat_player { get; set; }// 座位上所有玩家信息
    }

    // 桌子无状态服务
    [Reentrant]
    [StatelessWorker]
    public class GrainCellDesktopService : Grain, ICellDesktopService
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }

        //---------------------------------------------------------------------
        // 请求根据查询条件查找桌子
        async Task<List<DesktopInfo>> ICellDesktopService.searchDesktop(DesktopSearchFilter desktop_search_filter)
        {
            Logger.Info("searchDesktop()");

            string query = string.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6} FROM Fishing as QueryDesktop 
                WHERE seat_num=$1 and game_speed=$2 and is_vip=$3 and desktop_tableid=$4 LIMIT 10;"
            , "desktop_etguid"
            , "seat_num"
            , "game_speed"
            , "desktop_tableid"
            , "list_seat_player"
            , "seat_player_num"
            , "all_player_num");

            IQueryRequest query_request = QueryRequest.Create(query)
                   .ScanConsistency(ScanConsistency.RequestPlus)
                   .AddPositionalParameter(desktop_search_filter.seat_num)
                   .AddPositionalParameter(desktop_search_filter.is_vip)
                   .AddPositionalParameter(desktop_search_filter.desktop_tableid);

            var result = await DbClientCouchbase.Instance.Bucket.QueryAsync<QueryDesktop>(query_request);

            List<DesktopInfo> list_desktop = new List<DesktopInfo>();
            if (result.Success)
            {
                // 对结果集进行处理
                foreach (var i in result.Rows)
                {
                    QueryDesktop query_desktop = i;
                    if (string.IsNullOrEmpty(query_desktop.desktop_etguid)) continue;

                    DesktopInfo desktop_info = new DesktopInfo();
                    desktop_info.desktop_etguid = query_desktop.desktop_etguid;
                    desktop_info.seat_num = query_desktop.seat_num;
                    desktop_info.is_vip = desktop_search_filter.is_vip;
                    desktop_info.desktop_tableid = desktop_search_filter.desktop_tableid;
                    desktop_info.seat_player_num = query_desktop.seat_player_num;
                    desktop_info.all_player_num = query_desktop.all_player_num;
                    desktop_info.list_seat_player = new List<DesktopPlayerInfo>();

                    if (query_desktop.list_seat_player != null)
                    {
                        foreach (var j in query_desktop.list_seat_player)
                        {
                            DesktopPlayerInfo info = new DesktopPlayerInfo();
                            info.seat_index = j.seat_index;
                            info.player_etguid = j.player_etguid;
                            info.nick_name = j.nick_name;
                            info.icon = j.icon;
                            info.chip = j.chip;
                            desktop_info.list_seat_player.Add(info);
                        }
                    }

                    list_desktop.Add(desktop_info);
                }
            }

            // Couchbase中没有找到桌子，则创建一批符合条件的桌子
            if (list_desktop.Count < 10)
            {
                //for (int i = 0; i < 10; i++)
                {
                    // 创建桌子
                    var grain_desktop = GrainFactory.GetGrain<ICellDesktop>(Guid.NewGuid());
                    string desktop_etguid = grain_desktop.GetPrimaryKey().ToString();
                    //GetLogger().Info("GrainCellDesktopService.searchDesktop() 创建新桌子，DesktopEtGuid={0}", desktop_etguid);

                    DesktopInfo desktop_info = new DesktopInfo();
                    desktop_info.desktop_etguid = desktop_etguid;
                    desktop_info.seat_num = desktop_search_filter.seat_num;
                    desktop_info.is_vip = desktop_search_filter.is_vip;
                    desktop_info.desktop_tableid = desktop_search_filter.desktop_tableid;
                    desktop_info.all_player_num = 0;
                    desktop_info.seat_player_num = 0;
                    desktop_info.list_seat_player = new List<DesktopPlayerInfo>();

                    list_desktop.Add(desktop_info);

                    grain_desktop.s2sSetupDesktop(desktop_info);
                }
            }

            return list_desktop;
        }

        //---------------------------------------------------------------------
        // 请求查询指定桌子
        async Task<DesktopInfo> ICellDesktopService.searchDesktop(string desktop_etguid, DesktopRequestPlayerEnter request_enter)
        {
            Logger.Info("searchDesktop()");

            DesktopInfo desktop_info = null;

            if (string.IsNullOrEmpty(desktop_etguid))
            {
                goto End;
            }

            // 首先查询桌子是否存在
            string key = "CacheDesktopData_" + desktop_etguid;
            bool exists = await DbClientCouchbase.Instance.asyncExists(key);
            if (!exists)
            {
                goto End;
            }

            // 进入桌子
            var grain_desktop = GrainFactory.GetGrain<ICellDesktop>(new Guid(desktop_etguid));
            desktop_info = await grain_desktop.s2sGetDesktopInfo();

            End:
            if (desktop_info == null)
            {
                desktop_info = new DesktopInfo();
            }

            return desktop_info;
        }

        //---------------------------------------------------------------------
        // 请求查询好友所在的牌桌
        async Task<List<DesktopInfo>> ICellDesktopService.searchDesktopFollowFriend(string desktop_etguid)
        {
            List<DesktopInfo> list_desktop = new List<DesktopInfo>();

            return list_desktop;
        }

        //---------------------------------------------------------------------
        // 请求查询可以立即玩的桌子
        async Task<DesktopInfo> ICellDesktopService.searchDesktopAuto(DesktopRequestPlayerEnter request_enter)
        {
            Logger.Info("searchDesktopAuto()");

            DesktopInfo desktop_info = null;

            // 根据玩家属性限定搜索条件
            DesktopSearchFilter desktop_search_filter = new DesktopSearchFilter();
            desktop_search_filter.is_vip = request_enter.is_vip;
            desktop_search_filter.seat_num = 6;
            desktop_search_filter.is_seat_full = false;
            desktop_search_filter.desktop_tableid = 1;

            var grain_desktopservice = GrainFactory.GetGrain<ICellDesktopService>(0);
            List<DesktopInfo> list_desktop = await grain_desktopservice.searchDesktop(desktop_search_filter);
            if (list_desktop == null || list_desktop.Count == 0)
            {
                goto End;
            }

            string desktop_etguid = list_desktop[0].desktop_etguid;

            // 进入桌子
            var grain_desktop = GrainFactory.GetGrain<ICellDesktop>(new Guid(desktop_etguid));
            desktop_info = await grain_desktop.s2sGetDesktopInfo();

            End:
            if (desktop_info == null)
            {
                desktop_info = new DesktopInfo();
            }

            return desktop_info;
        }

        //-------------------------------------------------------------------------
        // 请求创建私人牌桌
        Task<DesktopInfo> ICellDesktopService.createPrivateDesktop(PrivateDesktopCreateInfo desktop_createinfo)
        {
            Logger.Info("createPrivateDesktop()");

            var grain_desktop = GrainFactory.GetGrain<ICellDesktop>(Guid.NewGuid());
            string desktop_etguid = grain_desktop.GetPrimaryKey().ToString();

            DesktopInfo desktop_info = new DesktopInfo();
            desktop_info.desktop_etguid = desktop_etguid;
            desktop_info.seat_num = desktop_createinfo.seat_num;
            desktop_info.is_vip = desktop_createinfo.is_vip;
            desktop_info.desktop_tableid = desktop_createinfo.desktop_tableid;
            desktop_info.all_player_num = 0;
            desktop_info.seat_player_num = 0;
            desktop_info.list_seat_player = new List<DesktopPlayerInfo>();

            grain_desktop.s2sSetupDesktop(desktop_info);

            return Task.FromResult(desktop_info);
        }
    }
}

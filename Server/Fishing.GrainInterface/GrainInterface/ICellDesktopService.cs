using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using GF.Common;

namespace Ps
{
    // 桌子无状态服务
    public interface ICellDesktopService : IGrainWithIntegerKey
    {
        //---------------------------------------------------------------------
        // 请求根据查询条件查找桌子
        Task<List<DesktopInfo>> searchDesktop(DesktopSearchFilter desktop_search_filter);

        //---------------------------------------------------------------------
        // 请求查询指定桌子
        Task<DesktopInfo> searchDesktop(string desktop_etguid, DesktopRequestPlayerEnter request_enter);

        //---------------------------------------------------------------------
        // 请求查询好友所在的牌桌
        Task<List<DesktopInfo>> searchDesktopFollowFriend(string desktop_etguid);

        //---------------------------------------------------------------------
        // 请求查询可以立即玩的桌子
        Task<DesktopInfo> searchDesktopAuto(DesktopRequestPlayerEnter request_enter);

        //-------------------------------------------------------------------------
        // 请求创建私人牌桌
        Task<DesktopInfo> createPrivateDesktop(PrivateDesktopCreateInfo desktop_createinfo);
    }
}

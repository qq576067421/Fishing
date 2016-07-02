using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using GF.Common;

namespace Ps
{
    public interface ICellDesktop : IGrainWithGuidKey
    {
        //---------------------------------------------------------------------
        // 初始化桌子信息
        Task s2sSetupDesktop(DesktopInfo desktop_info);

        //---------------------------------------------------------------------
        // 获取桌子信息
        Task<DesktopInfo> s2sGetDesktopInfo();

        //---------------------------------------------------------------------
        // 玩家进入桌子
        Task<DesktopData> s2sPlayerEnter(DesktopRequestPlayerEnter request_enter, EntityData etdata_playermirror);

        //---------------------------------------------------------------------
        // 玩家离开桌子
        Task<DesktopPlayerLeaveInfo> s2sPlayerLeave(string player_etguid);

        //---------------------------------------------------------------------
        // 桌子内聊天广播
        Task s2sDesktopChat(ChatMsgRecv msg);

        //---------------------------------------------------------------------
        // 玩家本轮操作取消托管
        Task s2sPlayerCancelAutoAction(string player_etguid);

        //---------------------------------------------------------------------
        // 玩家请求
        //Task<DesktopResponse> s2sPlayerRequest(string player_etguid, DesktopRequest desktop_request);

        //---------------------------------------------------------------------
        // 玩家操作请求
        Task s2sPlayerActionRequest(string player_etguid, List<string> vec_param);
    }
}

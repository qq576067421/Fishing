using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Orleans;

namespace Ps
{
    public interface ICellPlayerProxy : IGrainWithGuidKey
    {
        //---------------------------------------------------------------------
        // 玩家在线状态改变
        Task s2sPlayerServerStateChange(PlayerServerState new_state);

        //---------------------------------------------------------------------
        // 收到好友进桌邀请
        Task s2sInvitePlayerEnterDesktop(string from_friend_etguid, string desktop_etguid,
            int sb, int bb, int player_num, int seat_num);

        //---------------------------------------------------------------------
        // 收到玩家赠送的筹码
        Task s2sGivePlayerChip(string from_player_etguid, int chip);

        //---------------------------------------------------------------------
        // 请求加好友
        Task s2sRequestAddFriend(string request_player_etguid);

        //---------------------------------------------------------------------
        // 响应加好友
        Task s2sResponseAddFriend(string response_player_etguid, bool agree);

        //---------------------------------------------------------------------
        // 删除好友
        Task s2sDeleteFriend(string friend_etguid);

        //---------------------------------------------------------------------
        // 好友聊天
        Task s2sRecvChatFromFriend(ChatMsgRecv msg_recv);

        //---------------------------------------------------------------------
        // 发送邮件到邮箱
        Task s2sRecvMail(MailData mail_data);
    }
}

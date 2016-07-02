using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using GF.Common;

public enum CouchbaseQueDataType
{
    None = 0,
    InvitePlayerEnterDesktop = 100,
    GivePlayerChip = 200,
    RequestAddFriend = 300,
    ResponseAddFriend = 400,
    DeleteFriend = 500,
    RecvChatFromFriend = 600,
    RecvMail = 700,
}

namespace Ps
{
    // 玩家代理Grain，有状态，可重入
    [Reentrant]
    public class GrainCellPlayerProxy : Grain, ICellPlayerProxy
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }
        public IGrainFactory GF { get { return this.GrainFactory; } }
        IDisposable TimerHandleUpdate { get; set; }
        PlayerServerState PlayerServerState { get; set; }// 玩家在线状态
        CouchbaseQue CouchbaseQue { get; set; }

        //---------------------------------------------------------------------
        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromMinutes(1));

            PlayerServerState = PlayerServerState.Offline;

            string que_id = this.GetPrimaryKey().ToString();
            CouchbaseQue = new CouchbaseQue("EtPlayer", que_id);

            TimerHandleUpdate = RegisterTimer((_) => _update(), null, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));

            return base.OnActivateAsync();
        }

        //---------------------------------------------------------------------
        public override Task OnDeactivateAsync()
        {
            TimerHandleUpdate.Dispose();

            Logger.Info("OnDeactivateAsync()");

            return base.OnDeactivateAsync();
        }

        //---------------------------------------------------------------------
        // 玩家在线状态改变
        Task ICellPlayerProxy.s2sPlayerServerStateChange(PlayerServerState new_state)
        {
            PlayerServerState = new_state;

            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        // 收到好友进桌邀请
        async Task ICellPlayerProxy.s2sInvitePlayerEnterDesktop(string from_friend_etguid, string desktop_etguid,
             int sb, int bb, int player_num, int seat_num)
        {
            CouchbaseQueData que_data = new CouchbaseQueData();
            que_data.seq = 0;
            que_data.type = (int)CouchbaseQueDataType.InvitePlayerEnterDesktop;
            que_data.map_data = new Dictionary<string, string>();
            que_data.map_data["from_friend_etguid"] = from_friend_etguid;
            que_data.map_data["desktop_etguid"] = desktop_etguid;
            que_data.map_data["sb"] = sb.ToString();
            que_data.map_data["bb"] = bb.ToString();
            que_data.map_data["player_num"] = player_num.ToString();
            que_data.map_data["seat_num"] = seat_num.ToString();
            await CouchbaseQue.pushData(que_data);
        }

        //---------------------------------------------------------------------
        // 收到玩家赠送的筹码
        async Task ICellPlayerProxy.s2sGivePlayerChip(string from_player_etguid, int chip)
        {
            CouchbaseQueData que_data = new CouchbaseQueData();
            que_data.seq = 0;
            que_data.type = (int)CouchbaseQueDataType.GivePlayerChip;
            que_data.map_data = new Dictionary<string, string>();
            que_data.map_data["from_player_etguid"] = from_player_etguid;
            que_data.map_data["chip"] = chip.ToString();
            await CouchbaseQue.pushData(que_data);
        }

        //---------------------------------------------------------------------
        // 请求加好友
        async Task ICellPlayerProxy.s2sRequestAddFriend(string request_player_etguid)
        {
            CouchbaseQueData que_data = new CouchbaseQueData();
            que_data.seq = 0;
            que_data.type = (int)CouchbaseQueDataType.RequestAddFriend;
            que_data.map_data = new Dictionary<string, string>();
            que_data.map_data["request_player_etguid"] = request_player_etguid;
            await CouchbaseQue.pushData(que_data);
        }

        //---------------------------------------------------------------------
        // 响应加好友
        async Task ICellPlayerProxy.s2sResponseAddFriend(string response_player_etguid, bool agree)
        {
            CouchbaseQueData que_data = new CouchbaseQueData();
            que_data.seq = 0;
            que_data.type = (int)CouchbaseQueDataType.ResponseAddFriend;
            que_data.map_data = new Dictionary<string, string>();
            que_data.map_data["response_player_etguid"] = response_player_etguid;
            que_data.map_data["agree"] = agree.ToString();
            await CouchbaseQue.pushData(que_data);
        }

        //---------------------------------------------------------------------
        // 删除好友
        async Task ICellPlayerProxy.s2sDeleteFriend(string friend_etguid)
        {
            CouchbaseQueData que_data = new CouchbaseQueData();
            que_data.seq = 0;
            que_data.type = (int)CouchbaseQueDataType.DeleteFriend;
            que_data.map_data = new Dictionary<string, string>();
            que_data.map_data["friend_etguid"] = friend_etguid;
            await CouchbaseQue.pushData(que_data);
        }

        //---------------------------------------------------------------------
        // 好友聊天
        async Task ICellPlayerProxy.s2sRecvChatFromFriend(ChatMsgRecv msg_recv)
        {
            CouchbaseQueData que_data = new CouchbaseQueData();
            que_data.seq = 0;
            que_data.type = (int)CouchbaseQueDataType.RecvChatFromFriend;
            que_data.map_data = new Dictionary<string, string>();
            que_data.map_data["msg_recv"] = EbTool.jsonSerialize(msg_recv);
            await CouchbaseQue.pushData(que_data);
        }

        //---------------------------------------------------------------------
        async Task ICellPlayerProxy.s2sRecvMail(MailData mail_data)
        {
            CouchbaseQueData que_data = new CouchbaseQueData();
            que_data.seq = 0;
            que_data.type = (int)CouchbaseQueDataType.RecvMail;
            que_data.map_data = new Dictionary<string, string>();
            que_data.map_data["mail_data"] = EbTool.jsonSerialize(mail_data);
            await CouchbaseQue.pushData(que_data);
        }

        //---------------------------------------------------------------------
        Task _update()
        {
            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        public IStreamProvider getStreamProvider()
        {
            IStreamProvider stream_provider = GetStreamProvider(StringDef.SMSProvider);
            return stream_provider;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using Ps;

public class CellPlayerObserver : ICellPlayerObserver
{
    //-------------------------------------------------------------------------
    BaseApp<DefApp> CoApp { get; set; }
    IRpcSession Session { get; set; }
    Dictionary<MethodType, Action<MethodData>> MapFunc { get; set; }

    //-------------------------------------------------------------------------
    public CellPlayerObserver(IComponent co_app, IRpcSession s)
    {
        CoApp = (BaseApp<DefApp>)co_app;
        Session = s;
        MapFunc = new Dictionary<MethodType, Action<MethodData>>();

        MapFunc[MethodType.s2cAccountNotify] = s2cAccountNotify;
        MapFunc[MethodType.s2cPlayerNotify] = s2cPlayerNotify;
        MapFunc[MethodType.s2cBagNotify] = s2cBagNotify;
        MapFunc[MethodType.s2cEquipNotify] = s2cEquipNotify;
        MapFunc[MethodType.s2cStatusNotify] = s2cStatusNotify;
        MapFunc[MethodType.s2cPlayerChatNotify] = s2cPlayerChatNotify;
        MapFunc[MethodType.s2cPlayerFriendNotify] = s2cPlayerFriendNotify;
        MapFunc[MethodType.s2cPlayerMailBoxNotify] = s2cPlayerMailBoxNotify;
        MapFunc[MethodType.s2cPlayerTaskNotify] = s2cPlayerTaskNotify;
        MapFunc[MethodType.s2cPlayerTradeNotify] = s2cPlayerTradeNotify;
        MapFunc[MethodType.s2cPlayerDesktopNotify] = s2cPlayerDesktopNotify;
        MapFunc[MethodType.s2cPlayerLobbyNotify] = s2cPlayerLobbyNotify;
        MapFunc[MethodType.s2cPlayerRankingNotify] = s2cPlayerRankingNotify;
    }

    //-------------------------------------------------------------------------
    void ICellPlayerObserver.s2cNotify(MethodData method_data)
    {
        Action<MethodData> func = null;
        MapFunc.TryGetValue(method_data.method_id, out func);
        if (func != null)
        {
            func(method_data);
        }
        else
        {
            EbLog.Error("CellPlayerObserver.s2cNotify() Not Found MethodId: " + method_data.method_id.ToString());
        }
    }

    //-------------------------------------------------------------------------
    // 帐号通知
    void s2cAccountNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var account_notify = EbTool.protobufDeserialize<AccountNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cAccountNotify, account_notify);
            Session.close();
        }
    }

    //-------------------------------------------------------------------------
    // 玩家通知
    void s2cPlayerNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var player_notify = EbTool.protobufDeserialize<PlayerNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerNotify, player_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 背包通知
    void s2cBagNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var bag_notify = EbTool.protobufDeserialize<BagNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cBagNotify, bag_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 装备通知
    void s2cEquipNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var equip_notify = EbTool.protobufDeserialize<EquipNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cEquipNotify, equip_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 状态通知
    void s2cStatusNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var status_notify = EbTool.protobufDeserialize<StatusNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cStatusNotify, status_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 聊天通知
    void s2cPlayerChatNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var playerchat_notify = EbTool.protobufDeserialize<PlayerChatNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerChatNotify, playerchat_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 好友通知
    void s2cPlayerFriendNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var playerfriend_notify = EbTool.protobufDeserialize<PlayerFriendNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerFriendNotify, playerfriend_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 玩家邮件通知
    void s2cPlayerMailBoxNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var mailbox_notify = EbTool.protobufDeserialize<PlayerMailBoxNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerMailBoxNotify, mailbox_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 任务通知
    void s2cPlayerTaskNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var playertask_notify = EbTool.protobufDeserialize<PlayerTaskNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerTaskNotify, playertask_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 交易通知事件
    void s2cPlayerTradeNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var player_trade_notify = EbTool.protobufDeserialize<PlayerTradeNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerTradeNotify, player_trade_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 桌子通知事件
    void s2cPlayerDesktopNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var desktop_notify = EbTool.protobufDeserialize<DesktopNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerDesktopNotify, desktop_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 大厅通知事件
    void s2cPlayerLobbyNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var lobby_notify = EbTool.protobufDeserialize<PlayerLobbyNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerLobbyNotify, lobby_notify);
        }
    }

    //-------------------------------------------------------------------------
    // 排行榜通知事件
    void s2cPlayerRankingNotify(MethodData method_data)
    {
        var client_info = CoApp.getClientInfo(Session);
        if (client_info == null) return;

        var ranking_notify = EbTool.protobufDeserialize<PlayerRankingNotify>(method_data.param1);
        lock (CoApp.RpcLock)
        {
            CoApp.rpcBySession(Session, (ushort)MethodType.s2cPlayerRankingNotify, ranking_notify);
        }
    }
}

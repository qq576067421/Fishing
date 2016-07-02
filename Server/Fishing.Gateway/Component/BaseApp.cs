using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class ClientInfo
{
    public IPEndPoint remote_endpoint;
    public string acc;
    public string et_player_guid;
    public ICellPlayerObserver player_watcher;
    public ICellPlayerObserver player_watcher_weak;
}

public class BaseApp<TDef> : Component<TDef> where TDef : DefApp, new()
{
    //-------------------------------------------------------------------------
    public Dictionary<IRpcSession, ClientInfo> MapClientInfo { get; set; }
    public object MapClientInfoLock { get; set; }
    public object RpcLock { get; set; }
    Entity EtLogin { get; set; }
    Entity EtPlayer { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        Dictionary<string, object> cache_data = new Dictionary<string, object>();
        cache_data["CoApp"] = this;

        MapClientInfo = new Dictionary<IRpcSession, ClientInfo>();
        MapClientInfoLock = new object();
        RpcLock = new object();

        var co_supersocket = EsEngine.Instance.CoSuperSocket;
        co_supersocket.OnSessionCreate += _onSuperSocketSessionCreate;
        co_supersocket.OnSessionDestroy += _onSuperSocketSessionDestroy;

        EtLogin = EntityMgr.Instance.genEntity<EtLogin>(cache_data);
        EtPlayer = EntityMgr.Instance.genEntity<EtPlayer>(cache_data);

        EbLog.Note("PsTexasPokerBase启动成功");
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        if (EtPlayer != null)
        {
            EtPlayer.close();
            EtPlayer = null;
        }

        if (EtLogin != null)
        {
            EtLogin.close();
            EtLogin = null;
        }

        EbLog.Note("PsTexasPokerBase停止成功");
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //-------------------------------------------------------------------------
    public ClientInfo getClientInfo(IRpcSession s)
    {
        ClientInfo client_info = null;
        lock (MapClientInfoLock)
        {
            MapClientInfo.TryGetValue(s, out client_info);
        }
        return client_info;
    }

    //-------------------------------------------------------------------------
    public ClientInfo getClientInfoThenRemove(IRpcSession s)
    {
        ClientInfo client_info = null;
        lock (MapClientInfoLock)
        {
            if (MapClientInfo.TryGetValue(s, out client_info))
            {
                MapClientInfo.Remove(s);
            }
        }
        return client_info;
    }

    //-------------------------------------------------------------------------
    void _onSuperSocketSessionCreate(IRpcSession s, IPEndPoint remote_endpoint)
    {
        if (remote_endpoint != null)
        {
            EbLog.Note("玩家连接成功，RemoteEndPoint=" + remote_endpoint.ToString());
        }

        ClientInfo client_info = new ClientInfo();
        client_info.remote_endpoint = remote_endpoint;
        client_info.acc = "";
        client_info.et_player_guid = "";
        client_info.player_watcher = null;
        client_info.player_watcher_weak = null;

        lock (MapClientInfoLock)
        {
            MapClientInfo[s] = client_info;
        }
    }

    //-------------------------------------------------------------------------
    async void _onSuperSocketSessionDestroy(IRpcSession s, SessionCloseReason reason)
    {
        ClientInfo client_info = getClientInfoThenRemove(s);
        if (client_info == null) return;

        EbLog.Note("玩家连接断开，EtPlayerGuid=" + client_info.et_player_guid);

        try
        {
            if (client_info.player_watcher_weak != null && !string.IsNullOrEmpty(client_info.et_player_guid))
            {
                var grain_player = GrainClient.GrainFactory.GetGrain<ICellPlayer>(new Guid(client_info.et_player_guid));
                await grain_player.unsubPlayer(client_info.player_watcher_weak);
                client_info.remote_endpoint = null;
                client_info.player_watcher = null;
                client_info.player_watcher_weak = null;
            }
        }
        catch (Exception ex)
        {
            EbLog.Error(ex.ToString());
        }
    }
}

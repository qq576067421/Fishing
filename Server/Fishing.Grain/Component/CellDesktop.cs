using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class _tInfoOfDestop
{
    public int CountOfPlayer = 0;
    public int CountOfBot = 0;
    public int CountOfOpenSeat = 0;
}

public class LogicListener : ILogicListener
{
    //-------------------------------------------------------------------------
    public CellDesktop<DefDesktop> CoDesktop { get; private set; }

    //-------------------------------------------------------------------------
    public LogicListener(IComponent co_desktop)
    {
        CoDesktop = (CellDesktop<DefDesktop>)co_desktop;
    }

    //-------------------------------------------------------------------------
    // 帮助scene从logic广播消息到所有render
    public void onLogicScene2RenderAll(List<string> vec_param)
    {
        CoDesktop.logicScene2RenderAll(vec_param);
    }

    //-------------------------------------------------------------------------
    // 帮助scene从logic发送消息到render
    public void onLogicScene2Render(uint et_player_rpcid, List<string> vec_param)
    {
        CoDesktop.logicScene2Render(et_player_rpcid, vec_param);
    }

    //-------------------------------------------------------------------------
    // 设置玩家金币
    public void onLogicSceneSetPlayerGold(uint et_player_rpcid, int new_gold, int fish_vibid, string reason, int turret_rate)
    {
        //mEtDesktop.onLogicSceneSetPlayerGold(et_player_rpcid, new_gold, fish_vibid, reason, turret_rate);

        for (int i = 0; i < 6; ++i)
        {
            if (CoDesktop.AllSeat[i].et_playermirror != null && CoDesktop.AllSeat[i].et_player_rpcid == et_player_rpcid)
            {
                //int old_gold = AllSeat[i].et_player.getEntityDef().mPropGold.get();
                //AllSeat[i].et_player.setPlayerGold(new_gold, reason);

                if (reason == "TurretFire")
                {
                    // 发炮支出
                    //int gold = old_gold - new_gold;
                    //mEtDesktopPumping.subjoinGold(mSeatList[i].et_player.getEntityRpcId(), i, gold, new_gold, fish_vibid, turret_rate);
                }
                else if (reason == "FishLord")
                {
                    // 打鱼收入
                    //int gold = new_gold - old_gold;
                    //mEtDesktopPumping.subtractGold(mSeatList[i].et_player.getEntityRpcId(), i, gold, new_gold, fish_vibid, turret_rate);
                }

                break;
            }
        }
    }

    //-------------------------------------------------------------------------
    // 获取玩家金币
    public int onLogicSceneGetPlayerGold(uint et_player_rpcid)
    {
        for (int i = 0; i < 6; ++i)
        {
            if (CoDesktop.AllSeat[i].et_playermirror != null && CoDesktop.AllSeat[i].et_player_rpcid == et_player_rpcid)
            {
                var co_actormirror = CoDesktop.AllSeat[i].et_playermirror.getComponent<CellActorMirror<DefActorMirror>>();
                return co_actormirror.Def.PropGold.get();
            }
        }

        return 100000;
    }

    //-------------------------------------------------------------------------
    // 鱼死亡
    public void onLogicSceneFishDie(uint et_player_rpcid, int fish_vibid, int total_score)
    {
        //mEtDesktop.onLogicSceneFishDie(et_player_rpcid, fish_vibid, total_score);

        for (int i = 0; i < 6; ++i)
        {
            if (CoDesktop.AllSeat[i].et_playermirror != null && CoDesktop.AllSeat[i].et_player_rpcid == et_player_rpcid)
            {
                var co_actormirror = CoDesktop.AllSeat[i].et_playermirror.getComponent<CellActorMirror<DefActorMirror>>();
                //AllSeat[i].et_player.getEtFishMgr().killFish(fish_vibid, total_score);
                break;
            }
        }
    }

    //-------------------------------------------------------------------------
    // 鱼命中（包含导致鱼死的那一炮）
    public void onLogicSceneFishHit(uint et_player_rpcid, int fish_vibid, int turret_rate)
    {
        for (int i = 0; i < 6; ++i)
        {
            if (CoDesktop.AllSeat[i].et_playermirror != null && CoDesktop.AllSeat[i].et_player_rpcid == et_player_rpcid)
            {
                //mEtDesktopPumping.onLogicSceneFishHit(AllSeat[i].et_player.getEntityRpcId(), i, fish_vibid, turret_rate);
                break;
            }
        }
    }
}

public class CellDesktop<TDef> : Component<TDef> where TDef : DefDesktop, new()
{
    //-------------------------------------------------------------------------
    public DesktopConfigData DesktopConfigData { get; private set; }
    public DesktopInfo DesktopInfo { get; private set; }
    public SeatInfo[] AllSeat { get; set; }
    public Dictionary<string, Entity> MapAllPlayer { get; set; }
    public Dictionary<uint, Entity> MapAllPlayer1 { get; set; }
    bool Init { get; set; }
    string CacheDesktopKey { get; set; }
    CLogicScene LogicScene { get; set; }
    Queue<_tAoIEvent> QueAoIEvent { get; set; }
    Queue<byte> QuePlayerId { get; set; }
    byte MaxPlayerId { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        Init = false;
        CacheDesktopKey = "CacheDesktopData_" + Entity.Guid;

        DesktopConfigData = new Ps.DesktopConfigData();
        DesktopConfigData.desktop_etguid = Entity.Guid;
        DesktopConfigData.seat_num = 6;
        DesktopConfigData.is_vip = false;
        DesktopConfigData.desktop_waitwhile_tm = 60;
        MapAllPlayer1 = new Dictionary<uint, Entity>();
        MapAllPlayer = new Dictionary<string, Entity>();

        byte index = 0;
        AllSeat = new SeatInfo[DesktopConfigData.seat_num];
        foreach (var i in AllSeat)
        {
            SeatInfo seat_info = new SeatInfo();
            seat_info.index = index;
            seat_info.et_player_rpcid = 0;
            seat_info.et_playermirror = null;
            AllSeat[index] = seat_info;
            index++;
        }

        DesktopInfo = new DesktopInfo();
        DesktopInfo.desktop_etguid = Entity.Guid;
        DesktopInfo.seat_num = 6;
        DesktopInfo.desktop_tableid = 1;
        DesktopInfo.is_vip = false;
        DesktopInfo.list_seat_player = new List<DesktopPlayerInfo>();
        DesktopInfo.all_player_num = 0;
        DesktopInfo.seat_player_num = 0;

        QueAoIEvent = new Queue<_tAoIEvent>();
        QuePlayerId = new Queue<byte>();
        MaxPlayerId = 0;
        for (int i = 0; i < 50; i++)
        {
            QuePlayerId.Enqueue(++MaxPlayerId);
        }

        TbDataRoom._eRoomType room_type = TbDataRoom._eRoomType.RT_Tenfold;
        bool fish_mustdie = true;
        bool is_single = false;
        float pumping_rate = 1f;
        string path_fishlord = Path.Combine(ServerPath.getPathMediaRoot(), "Fishing\\FishLord\\");
        string path_route = Path.Combine(ServerPath.getPathMediaRoot(), "Fishing\\Route\\");
        ILogicListener listener = new LogicListener(this);
        LogicScene = new CLogicScene();
        LogicScene.create(1, 1.0f, is_single, fish_mustdie, listener, pumping_rate, _getRateList(room_type),
            CellApp.Instance.jsonCfg.json_packet_list,
            CellApp.Instance.jsonCfg.route_json_packet_list);
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        if (LogicScene != null)
        {
            LogicScene.Dispose();
            LogicScene = null;
        }

        AllSeat = null;
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        if (LogicScene != null)
        {
            LogicScene.update(elapsed_tm);
        }

        // 更新座位上所有玩家
        foreach (var i in AllSeat)
        {
            if (i.et_playermirror != null) i.et_playermirror.update(elapsed_tm);
        }

        while (QueAoIEvent.Count > 0)
        {
            var ev_aoi = QueAoIEvent.Dequeue();

            DesktopNotify desktop_notify;
            desktop_notify.id = DesktopNotifyId.PlayerSceneAoIUpdate;
            desktop_notify.data = EbTool.protobufSerialize<_tAoIEvent>(ev_aoi);

            StreamData sd = new StreamData();
            sd.event_id = StreamEventId.DesktopStreamEvent;
            sd.param1 = desktop_notify;
            var grain_desktop = Entity.getUserData<GrainCellDesktop>();
            grain_desktop.AsyncStream.OnNextAsync(sd);
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //---------------------------------------------------------------------
    // 初始化桌子信息
    public Task s2sSetupDesktop(DesktopInfo desktop_info)
    {
        var tb_desktop = EbDataMgr.Instance.getData<TbDataDesktopInfo>(DesktopInfo.desktop_tableid);
        if (tb_desktop == null)
        {
            EbLog.Error("CellDesktop.s2sSetupDesktop() Error DesktopTableId=" + DesktopInfo.desktop_tableid);
            return TaskDone.Done;
        }

        Init = true;
        DesktopInfo.seat_num = desktop_info.seat_num;
        DesktopInfo.desktop_tableid = desktop_info.desktop_tableid;
        DesktopInfo.is_vip = desktop_info.is_vip;
        DesktopInfo.all_player_num = desktop_info.all_player_num;
        DesktopInfo.seat_player_num = desktop_info.seat_player_num;
        DesktopInfo.list_seat_player.Clear();

        DesktopConfigData.is_vip = DesktopInfo.is_vip;

        byte index = 0;
        AllSeat = new SeatInfo[DesktopConfigData.seat_num];
        foreach (var i in AllSeat)
        {
            SeatInfo seat_info = new SeatInfo();
            seat_info.index = index;
            seat_info.et_playermirror = null;
            AllSeat[index] = seat_info;
            index++;
        }

        string data = EbTool.jsonSerialize(DesktopInfo);
        DbClientCouchbase.Instance.asyncSave(CacheDesktopKey, data, TimeSpan.FromSeconds(15.0));

        return TaskDone.Done;
    }

    //-------------------------------------------------------------------------
    // 创建新桌子实例
    public Task<DesktopInfo> s2sGetDesktopInfo()
    {
        return Task.FromResult(DesktopInfo);
    }

    //---------------------------------------------------------------------
    // 桌子内聊天广播
    public Task s2sDesktopChat(ChatMsgRecv msg)
    {
        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.PlayerChat;
        desktop_notify.data = EbTool.protobufSerialize<ChatMsgRecv>(msg);

        StreamData sd = new StreamData();
        sd.event_id = StreamEventId.DesktopStreamEvent;
        sd.param1 = desktop_notify;
        var grain_desktop = Entity.getUserData<GrainCellDesktop>();
        grain_desktop.AsyncStream.OnNextAsync(sd);

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    // 玩家本轮操作取消托管
    public Task s2sPlayerCancelAutoAction(string player_etguid)
    {
        return TaskDone.Done;
    }

    //-------------------------------------------------------------------------
    // 玩家进入桌子
    public Task<DesktopData> s2sPlayerEnter(DesktopRequestPlayerEnter request_enter, EntityData etdata_playermirror)
    {
        byte seat_index = request_enter.seat_index;
        if (getPlayerCountInSeat() >= DesktopConfigData.seat_num)
        {
            // 没有空座位了，观战
            seat_index = 255;
        }

        if (!isValidSeatIndex(seat_index))
        {
            // 座位索引范围不合法，重新分配空座位
            foreach (var i in AllSeat)
            {
                if (i.et_playermirror == null)
                {
                    seat_index = i.index;
                    break;
                }
            }
        }

        if (isValidSeatIndex(seat_index) && AllSeat[seat_index].et_playermirror != null)
        {
            // 座位上已经有人坐了，重新分配空座位
            foreach (var i in AllSeat)
            {
                if (i.et_playermirror == null)
                {
                    seat_index = i.index;
                    break;
                }
            }
        }

        var et_playermirror = EntityMgr.genEntity<EtPlayerMirror, Entity>(etdata_playermirror, Entity);
        var co_actormirror = et_playermirror.getComponent<CellActorMirror<DefActorMirror>>();
        var co_ai = et_playermirror.getComponent<CellActorMirrorAi<DefActorMirrorAi>>();

        byte actorid_indesktop = _genPlayerId();
        co_actormirror.Def.PropActorIdInDesktop.set(actorid_indesktop);

        EbLog.Note("CellDesktop.s2sPlayerEnter() PlayerEtGuid=" + et_playermirror.Guid);

        MapAllPlayer1[actorid_indesktop] = et_playermirror;
        MapAllPlayer[et_playermirror.Guid] = et_playermirror;
        if (isValidSeatIndex(seat_index)) AllSeat[seat_index].et_playermirror = et_playermirror;

        co_actormirror.onEnterDesktop(seat_index);

        // 更新DesktopInfo
        refreshDesktopInfo();

        // 广播玩家进入桌子
        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.PlayerEnter;
        desktop_notify.data = EbTool.protobufSerialize<EntityData>(et_playermirror.genEntityData4All());

        StreamData sd = new StreamData();
        sd.event_id = StreamEventId.DesktopStreamEvent;
        sd.param1 = desktop_notify;
        var grain_desktop = Entity.getUserData<GrainCellDesktop>();
        grain_desktop.AsyncStream.OnNextAsync(sd);

        // 通知场景玩家坐下
        LogicScene.scenePlayerEnter(actorid_indesktop, 1, "aabb", false, -1, TbDataTurret.TurretType.NormalTurret);
        float player_rate = 1.0f;// mEtDesktopPumping.getPlayerUpgradeRate();// 抽水率
        LogicScene.scenePlayerRateChanged(1, player_rate);

        DesktopData desktop_data = _getDesktopData();
        return Task.FromResult(desktop_data);
    }

    //-------------------------------------------------------------------------
    // 玩家离开桌子
    public Task<DesktopPlayerLeaveInfo> s2sPlayerLeave(string player_etguid)
    {
        EbLog.Note("CellDesktop.s2sPlayerLeave() PlayerEtGuid=" + player_etguid);

        DesktopPlayerLeaveInfo leave_info = new DesktopPlayerLeaveInfo();

        // 玩家不在座位上
        SeatInfo seat_cur = getSeat(player_etguid);
        if (seat_cur == null)
        {
            goto End;
        }

        End:
        CellActorMirror<DefActorMirror> co_playermirror = null;
        Entity et_playermirror = null;

        // 清空座位
        if (MapAllPlayer.TryGetValue(player_etguid, out et_playermirror))
        {
            co_playermirror = et_playermirror.getComponent<CellActorMirror<DefActorMirror>>();
            leave_info.stack = co_playermirror.Def.PropGold.get();
            byte actorid_indesktop = co_playermirror.Def.PropActorIdInDesktop.get();
            _freePlayerId(actorid_indesktop);

            et_playermirror.close();
            MapAllPlayer.Remove(player_etguid);
            MapAllPlayer1.Remove(actorid_indesktop);
        }

        foreach (var i in AllSeat)
        {
            if (i.et_playermirror != null && i.et_playermirror.Guid == player_etguid)
            {
                et_playermirror = i.et_playermirror;
                i.et_playermirror = null;
            }
        }

        // 更新DesktopInfo
        refreshDesktopInfo();

        // 广播玩家离开桌子
        {
            DesktopNotify desktop_notify;
            desktop_notify.id = DesktopNotifyId.PlayerLeave;
            desktop_notify.data = EbTool.protobufSerialize<string>(player_etguid);

            StreamData sd = new StreamData();
            sd.event_id = StreamEventId.DesktopStreamEvent;
            sd.param1 = desktop_notify;
            var grain_desktop = Entity.getUserData<GrainCellDesktop>();
            grain_desktop.AsyncStream.OnNextAsync(sd);
        }

        return Task.FromResult(leave_info);
    }

    //---------------------------------------------------------------------
    // 玩家操作请求
    public Task s2sPlayerActionRequest(string player_etguid, List<string> vec_param)
    {
        LogicScene.sceneOnRecvFromRender(vec_param);

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    public void onPlayerActionWaitingTimeEnd(string player_etguid)
    {
        SeatInfo seat_cur = getSeat(player_etguid);
        if (seat_cur == null) return;

        EbLog.Note("CellDesktop.onPlayerActionWaitingTimeEnd() PlayerEtGuid=" + player_etguid);
    }

    //-------------------------------------------------------------------------
    public void logicScene2RenderAll(List<string> vec_param)
    {
        _tAoIEvent aoi_ev;
        aoi_ev.id = _eAoIEvent.SceneBroadcast;
        aoi_ev.vec_param = vec_param;
        QueAoIEvent.Enqueue(aoi_ev);
    }

    //-------------------------------------------------------------------------
    public void logicScene2Render(uint et_player_rpcid, List<string> vec_param)
    {
        Entity et_actormirror = null;
        MapAllPlayer1.TryGetValue(1, out et_actormirror);
        if (et_actormirror == null) return;

        var grain_desktop = Entity.getUserData<GrainCellDesktop>();
        var grain_player = grain_desktop.GF.GetGrain<ICellPlayer>(new Guid(et_actormirror.Guid));
        grain_player.s2sDesktop2Player(vec_param);
    }

    //-------------------------------------------------------------------------
    // 桌子广播
    public void broadcast(ref DesktopNotify desktop_notify)
    {
        StreamData sd = new StreamData();
        sd.event_id = StreamEventId.DesktopStreamEvent;
        sd.param1 = desktop_notify;
        var grain_desktop = Entity.getUserData<GrainCellDesktop>();
        grain_desktop.AsyncStream.OnNextAsync(sd);
    }

    //-------------------------------------------------------------------------
    // 标记为可销毁
    public void signDestroy()
    {
        var grain_desktop = Entity.getUserData<GrainCellDesktop>();
        grain_desktop.signDestroy();
    }

    //-------------------------------------------------------------------------
    public Entity getEtPlayerMirrorByGuid(string et_player_guid)
    {
        Entity et_playermirror = null;
        MapAllPlayer.TryGetValue(et_player_guid, out et_playermirror);
        return et_playermirror;
    }

    //-------------------------------------------------------------------------
    public Entity getEtPlayerMirrorById(byte actorid_indesktop)
    {
        Entity et_playermirror = null;
        MapAllPlayer1.TryGetValue(actorid_indesktop, out et_playermirror);
        return et_playermirror;
    }

    //-------------------------------------------------------------------------
    public SeatInfo getSeat(string player_etguid)
    {
        foreach (var i in AllSeat)
        {
            if (i.et_playermirror != null && i.et_playermirror.Guid == player_etguid)
            {
                return i;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------
    public SeatInfo getSeat(byte seat_index_cur)
    {
        if (!isValidSeatIndex(seat_index_cur)) return null;

        return AllSeat[seat_index_cur];
    }

    //-------------------------------------------------------------------------
    public SeatInfo getNextSeat(string et_player_guid_cur)
    {
        SeatInfo seat_cur = null;
        foreach (var i in AllSeat)
        {
            if (i.et_playermirror != null && i.et_playermirror.Guid == et_player_guid_cur)
            {
                seat_cur = i;
                break;
            }
        }

        if (seat_cur == null) return null;

        return getNextSeat(seat_cur.index);
    }

    //-------------------------------------------------------------------------
    public SeatInfo getNextSeat(byte seat_index_cur)
    {
        seat_index_cur++;
        if (seat_index_cur >= DesktopConfigData.seat_num)
        {
            seat_index_cur = 0;
        }
        return AllSeat[seat_index_cur];
    }

    //-------------------------------------------------------------------------
    public bool isValidSeatIndex(byte seat_index)
    {
        if (seat_index < 0 || seat_index >= DesktopConfigData.seat_num) return false;
        else return true;
    }

    //-------------------------------------------------------------------------
    public void refreshDesktopInfo()
    {
        DesktopInfo.all_player_num = MapAllPlayer.Count;
        DesktopInfo.seat_player_num = getPlayerCountInSeat();

        DesktopInfo.list_seat_player.Clear();
        foreach (var i in AllSeat)
        {
            if (i.et_playermirror == null) continue;

            var co_playermirror = i.et_playermirror.getComponent<CellActorMirror<DefActorMirror>>();
            if (co_playermirror == null) continue;
            DesktopPlayerInfo player_info = new DesktopPlayerInfo();
            player_info.player_etguid = i.et_playermirror.Guid;
            player_info.seat_index = i.index;
            player_info.nick_name = co_playermirror.Def.mPropNickName.get();
            player_info.icon = co_playermirror.Def.mPropIcon.get();
            player_info.chip = co_playermirror.Def.PropGold.get();

            DesktopInfo.list_seat_player.Add(player_info);
        }

        string data = EbTool.jsonSerialize(DesktopInfo);
        DbClientCouchbase.Instance.asyncSave(CacheDesktopKey, data, TimeSpan.FromSeconds(15.0));
    }

    //-------------------------------------------------------------------------
    // 所有坐着的玩家
    public int getPlayerCountInSeat()
    {
        int player_num = 0;
        foreach (var i in AllSeat)
        {
            if (i.et_playermirror != null) player_num++;
        }
        return player_num;
    }

    //-------------------------------------------------------------------------
    public Task save()
    {
        if (Init)
        {
            DbClientCouchbase.Instance.asyncTouch(CacheDesktopKey, TimeSpan.FromSeconds(15.0));
        }

        return TaskDone.Done;
    }

    //-------------------------------------------------------------------------
    DesktopData _getDesktopData()
    {
        DesktopData desktop_data = new DesktopData();

        desktop_data.desktop_cfg_data = DesktopConfigData;
        desktop_data.list_actormirror = new List<EntityData>();
        foreach (var i in AllSeat)
        {
            if (i.et_playermirror == null) continue;

            var et_data = i.et_playermirror.genEntityData4All();
            desktop_data.list_actormirror.Add(et_data);
        }

        return desktop_data;
    }

    //-------------------------------------------------------------------------
    List<int> _getRateList(TbDataRoom._eRoomType room_type)
    {
        List<int> list_rate = new List<int>();

        switch (room_type)
        {
            case TbDataRoom._eRoomType.RT_Tenfold:
                return _getFixedRateList(10, 30, 50, 70, 100);
            case TbDataRoom._eRoomType.RT_Hundredfold:
                return _getFixedRateList(100, 200, 300, 400, 500);
            case TbDataRoom._eRoomType.RT_Thousandfold:
                return _getFixedRateList(500, 1000, 1500, 2000, 2500);
            case TbDataRoom._eRoomType.RT_Times:
                return _getFixedRateList(2500, 3000, 3500, 4000, 5000);
        }

        return list_rate;
    }

    //-------------------------------------------------------------------------
    List<int> _getFixedRateList(params int[] values)
    {
        List<int> list_rate = new List<int>();

        foreach (int it in values)
        {
            list_rate.Add(it);
        }

        return list_rate;
    }

    //-------------------------------------------------------------------------
    byte _genPlayerId()
    {
        if (QuePlayerId.Count == 0)
        {
            return ++MaxPlayerId;
        }
        else return QuePlayerId.Dequeue();
    }

    //-------------------------------------------------------------------------
    void _freePlayerId(byte player_id)
    {
        QuePlayerId.Enqueue(player_id);
    }
}

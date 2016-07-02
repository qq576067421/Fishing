using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GF.Common;
using Ps;

public class GameMonitor
{
    //-------------------------------------------------------------------------
    public int GameTotal { get; private set; }// 本桌完成的游戏局数
    public int GameWin { get; private set; }// 本桌赢的游戏局数（用于统计胜率）
    public int Exp { get; private set; }// 打牌所增加的经验
    bool InGame { get; set; }
    string PlayerEtGuid { get; set; }

    //-------------------------------------------------------------------------
    public GameMonitor(string player_etguid)
    {
        PlayerEtGuid = player_etguid;
        InGame = false;
        GameTotal = 0;
        GameWin = 0;
    }

    //-------------------------------------------------------------------------
    public void beginGame()
    {
        InGame = true;
    }
}

public class CellActorMirror<TDef> : Component<TDef> where TDef : DefActorMirror, new()
{
    //-------------------------------------------------------------------------
    GameMonitor mGameMonitor = null;

    //-------------------------------------------------------------------------
    public CellDesktop<DefDesktop> CoDesktop { get; private set; }
    public float ActionWaitingTime { get; private set; }// 等待玩家操作时间（单位：秒）
    public int GameTotal { get { return mGameMonitor.GameTotal; } }// 本桌完成的游戏局数
    public int GameWin { get { return mGameMonitor.GameWin; } }// 本桌赢的游戏局数（用于统计胜率）
    public int Exp { get { return mGameMonitor.Exp; } }// 打牌所增加的经验

    //-------------------------------------------------------------------------
    public override void init()
    {
        EnableSave2Db = false;
        EnableUpdate = false;

        var et_desktop = Entity.getUserData<Entity>();
        CoDesktop = et_desktop.getComponent<CellDesktop<DefDesktop>>();

        mGameMonitor = new GameMonitor(Entity.Guid);
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        // 暂离超时，自动Ob
        //var state = Def.mPropDesktopPlayerState.get();
        //if (state == DesktopPlayerState.WaitWhile)
        //{
        //    float wait_while_tm = Def.mPropWaitWhileTime.get();
        //    wait_while_tm -= elapsed_tm;
        //    Def.mPropWaitWhileTime.set(wait_while_tm);
        //    if (wait_while_tm <= 0f)
        //    {
        //        wait_while_tm = 60f;
        //        Def.mPropWaitWhileTime.set(wait_while_tm);

        //        if (c2sRequestOb())
        //        {
        //            SeatInfo seat = CoDesktop.getSeat(Entity.Guid);
        //            if (seat != null) seat.et_playermirror = null;
        //        }
        //    }
        //}
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //-------------------------------------------------------------------------
    // 玩家请求Ob
    public bool c2sRequestOb()
    {
        //var state = Def.mPropDesktopPlayerState.get();
        //if (state == DesktopPlayerState.Ob)
        //{
        //    return false;
        //}

        //c2sRequestAction(PlayerActionType.Fold);

        //Def.mPropDesktopPlayerState.set(DesktopPlayerState.Ob);

        // 刷新DesktopInfo
        CoDesktop.refreshDesktopInfo();

        // 广播玩家Ob
        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.PlayerOb;
        desktop_notify.data = EbTool.protobufSerialize(Entity.Guid);

        CoDesktop.broadcast(ref desktop_notify);

        return true;
    }

    //-------------------------------------------------------------------------
    // 玩家请求坐下
    public bool c2sRequestSitdown(byte seat_index)
    {
        //var state = Def.mPropDesktopPlayerState.get();
        //if (state != DesktopPlayerState.Ob)
        //{
        //    return false;
        //}

        if (!CoDesktop.isValidSeatIndex(seat_index))
        {
            // 座位索引范围不合法，坐下失败
            return false;
        }

        if (CoDesktop.isValidSeatIndex(seat_index) && CoDesktop.AllSeat[seat_index].et_playermirror != null)
        {
            // 座位上已经有人坐了，坐下失败
            return false;
        }

        onEnterDesktop(seat_index);

        // 刷新DesktopInfo
        CoDesktop.refreshDesktopInfo();

        // 广播玩家坐下
        PlayerSitdownData sitdown_data = new PlayerSitdownData();
        sitdown_data.player_etguid = Entity.Guid;
        sitdown_data.seat_index = seat_index;
        //sitdown_data.stack = Def.mPropStack.get();
        //sitdown_data.state = Def.mPropDesktopPlayerState.get();
        //sitdown_data.action = Def.mPropPlayerActionType.get();

        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.PlayerSitdown;
        desktop_notify.data = EbTool.protobufSerialize(sitdown_data);

        CoDesktop.broadcast(ref desktop_notify);

        return true;
    }

    //-------------------------------------------------------------------------
    // 玩家请求暂离
    public bool c2sRequestWaitWhile()
    {
        //var state = Def.mPropDesktopPlayerState.get();
        //if (state == DesktopPlayerState.WaitWhile || state == DesktopPlayerState.Ob)
        //{
        //    return false;
        //}

        //c2sRequestAction(PlayerActionType.Fold);

        //Def.mPropDesktopPlayerState.set(DesktopPlayerState.WaitWhile);

        PlayerWaitWhileData waitwhile_data = new PlayerWaitWhileData();
        waitwhile_data.player_etguid = Entity.Guid;
        waitwhile_data.wait_while_tm = CoDesktop.DesktopConfigData.desktop_waitwhile_tm;

        // 广播玩家暂离
        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.PlayerWaitWhile;
        desktop_notify.data = EbTool.protobufSerialize(waitwhile_data);

        CoDesktop.broadcast(ref desktop_notify);

        return true;
    }

    //-------------------------------------------------------------------------
    // 玩家请求继续
    public bool c2sRequestReturn()
    {
        //var state = Def.mPropDesktopPlayerState.get();
        //if (state != DesktopPlayerState.WaitWhile)
        //{
        //    return false;
        //}

        //Def.mPropDesktopPlayerState.set(DesktopPlayerState.WaitWhile);

        // 广播玩家Return
        PlayerReturnData return_data = new PlayerReturnData();
        return_data.player_etguid = Entity.Guid;
        //return_data.stack = Def.mPropStack.get();
        //return_data.state = Def.mPropDesktopPlayerState.get();
        //return_data.action = Def.mPropPlayerActionType.get();

        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.PlayerReturn;
        desktop_notify.data = EbTool.protobufSerialize(return_data);

        CoDesktop.broadcast(ref desktop_notify);

        return true;
    }

    //-------------------------------------------------------------------------
    // 玩家请求取消本轮托管
    public void c2sRequestCancelAutoAction()
    {
    }

    //-------------------------------------------------------------------------
    public void onEnterDesktop(byte seat_index)
    {
        // 如果该玩家是机器人，且未设定过Ai，则设定机器人Ai
        var co_ai = Entity.getComponent<CellActorMirrorAi<DefActorMirrorAi>>();
        string bt_name = co_ai.Def.mPropBtName.get();
        if (Def.mPropIsBot.get() && string.IsNullOrEmpty(bt_name))
        {
            co_ai.Def.mPropBtName.set("BtBot");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using GF.Common;
using GF.Server;
using Ps;

internal class CellPlayerDesktopConsumerObserver<StreamData> : IAsyncObserver<Ps.StreamData>
{
    //---------------------------------------------------------------------
    CellPlayerDesktop<DefPlayerDesktop> CoPlayerDesktop { get; set; }

    //---------------------------------------------------------------------
    internal CellPlayerDesktopConsumerObserver(IComponent co_playerteam)
    {
        this.CoPlayerDesktop = (CellPlayerDesktop<DefPlayerDesktop>)co_playerteam;
    }

    //---------------------------------------------------------------------
    public Task OnNextAsync(Ps.StreamData item, StreamSequenceToken token = null)
    {
        return CoPlayerDesktop.onDesktopStreamEvent(item);
    }

    //---------------------------------------------------------------------
    public Task OnCompletedAsync()
    {
        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    public Task OnErrorAsync(Exception ex)
    {
        return TaskDone.Done;
    }
}

public class CellPlayerDesktop<TDef> : Component<TDef> where TDef : DefPlayerDesktop, new()
{
    //-------------------------------------------------------------------------
    StreamSubscriptionHandle<StreamData> ConsumerHandle { get; set; }
    CellActor<DefActor> CoActor { get; set; }
    CellPlayer<DefPlayer> CoPlayer { get; set; }
    public string DesktopEtGuid { get; set; }
    public bool IsEntering { get; set; }// 进桌子保护，丢弃客户端的重复请求
    public float EnteringElapsedTm { get; set; }// 进桌子保护超时，30秒

    //-------------------------------------------------------------------------
    public override void init()
    {
        EnableSave2Db = false;
        EnableNetSync = false;

        CoActor = Entity.getComponent<CellActor<DefActor>>();
        CoPlayer = Entity.getComponent<CellPlayer<DefPlayer>>();
        IsEntering = false;
        EnteringElapsedTm = 30f;
    }

    //-------------------------------------------------------------------------
    public override async void release()
    {
        await leaveDesktop();
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        if (IsEntering)
        {
            EnteringElapsedTm -= elapsed_tm;
            if (EnteringElapsedTm < 0f)
            {
                IsEntering = false;
                EnteringElapsedTm = 30f;
            }
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //-------------------------------------------------------------------------
    public Task<MethodData> c2sPlayerDesktopRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        if (string.IsNullOrEmpty(DesktopEtGuid))
        {
            goto End;
        }

        var desktop_request = EbTool.protobufDeserialize<DesktopRequest>(method_data.param1);

        switch (desktop_request.id)
        {
            case DesktopRequestId.PlayerSceneAction:
                {
                    var vec_param = EbTool.protobufDeserialize<List<string>>(desktop_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_desktop = grain.GF.GetGrain<ICellDesktop>(new Guid(DesktopEtGuid));
                    grain_desktop.s2sPlayerActionRequest(Entity.Guid, vec_param);
                }
                break;
        }

        End:
        return Task.FromResult(result);
    }

    //---------------------------------------------------------------------
    // Cell->Cell的请求
    public Task s2sDesktop2Player(List<string> vec_param)
    {
        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.PlayerSceneAction;
        desktop_notify.data = EbTool.protobufSerialize<List<string>>(vec_param);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerDesktopNotify;
        notify_data.param1 = EbTool.protobufSerialize<DesktopNotify>(desktop_notify);

        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    public async Task onDesktopStreamEvent(StreamData data)
    {
        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.None;

        if (string.IsNullOrEmpty(DesktopEtGuid))
        {
            goto End;
        }

        switch (data.event_id)
        {
            case StreamEventId.DesktopStreamEvent:// 通知成员更新
                {
                    var desktop_notify = (DesktopNotify)data.param1;
                    switch (desktop_notify.id)
                    {
                        case DesktopNotifyId.PlayerChat:
                            {
                                var msg_recv = EbTool.protobufDeserialize<ChatMsgRecv>(desktop_notify.data);

                                CoPlayer.CoPlayerChat.s2sRecvChatFromDesktop(msg_recv);
                            }
                            break;
                        default:
                            {
                                notify_data.method_id = MethodType.s2cPlayerDesktopNotify;
                                notify_data.param1 = EbTool.protobufSerialize(desktop_notify);
                            }
                            break;
                    }
                }
                break;
            default:
                break;
        }

        End:
        if (notify_data.method_id != MethodType.None)
        {
            var grain = Entity.getUserData<GrainCellPlayer>();
            var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
            await grain_player.s2sNotify(notify_data);
        }
    }

    //---------------------------------------------------------------------
    // 立即玩
    public async Task enterDesktopPlayNow()
    {
        if (IsEntering)
        {
            EbLog.Note("CellPlayerDesktop.enterDesktopPlayNow() 错误：正在进入中");
            return;
        }
        IsEntering = true;

        if (!string.IsNullOrEmpty(DesktopEtGuid))
        {
            await leaveDesktop();
        }

        DesktopRequestPlayerEnter request_enter;
        request_enter.desktop_etguid = "";
        request_enter.seat_index = 255;
        request_enter.player_clip = 0;
        request_enter.player_gold = CoActor.Def.PropGold.get();
        request_enter.is_vip = CoActor.Def.mPropIsVIP.get();

        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_desktopservice = grain.GF.GetGrain<ICellDesktopService>(0);
        DesktopInfo desktop_info = await grain_desktopservice.searchDesktopAuto(request_enter);

        if (string.IsNullOrEmpty(desktop_info.desktop_etguid))
        {
            EbLog.Note("CellPlayerDesktop.enterDesktopPlayNow() 获取桌子信息出错");
        }
        else
        {
            EbLog.Note("CellPlayerDesktop.enterDesktopPlayNow() 获取桌子信息成功");
        }

        // Step1：监听桌子广播
        await _createDesktopStreamConsumer(desktop_info.desktop_etguid);

        // 自动下注
        int bet_chip = 2000;
        int left_chip = CoActor.Def.PropGold.get();
        if (left_chip < bet_chip) bet_chip = left_chip;
        left_chip -= bet_chip;
        CoPlayer.CoActor.Def.PropGold.set(left_chip);

        // Step2：进入桌子并获取桌子初始化信息
        var grain_desktop = grain.GF.GetGrain<ICellDesktop>(new Guid(desktop_info.desktop_etguid));
        DesktopData desktop_data = await grain_desktop.s2sPlayerEnter(request_enter, CoPlayer.exportEtPlayerMirror());

        IsEntering = false;
        DesktopEtGuid = desktop_data.desktop_cfg_data.desktop_etguid;

        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.DesktopInit;
        desktop_notify.data = EbTool.protobufSerialize<DesktopData>(desktop_data);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerDesktopNotify;
        notify_data.param1 = EbTool.protobufSerialize<DesktopNotify>(desktop_notify);
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //---------------------------------------------------------------------
    public async Task enterDesktopPrivate(PrivateDesktopCreateInfo desktop_createinfo)
    {
        if (IsEntering)
        {
            EbLog.Note("CellPlayerDesktop.enterDesktopPrivate() 错误：正在进入中");
            return;
        }
        IsEntering = true;

        if (!string.IsNullOrEmpty(DesktopEtGuid))
        {
            await leaveDesktop();
        }

        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_desktopservice = grain.GF.GetGrain<ICellDesktopService>(0);
        var desktop_info = await grain_desktopservice.createPrivateDesktop(desktop_createinfo);

        // Step1：监听桌子广播
        await _createDesktopStreamConsumer(desktop_info.desktop_etguid);

        // 自动下注
        int bet_chip = 2000;
        int left_chip = CoActor.Def.PropGold.get();
        if (left_chip < bet_chip) bet_chip = left_chip;
        left_chip -= bet_chip;
        CoPlayer.CoActor.Def.PropGold.set(left_chip);

        DesktopRequestPlayerEnter request_enter;
        request_enter.desktop_etguid = desktop_info.desktop_etguid;
        request_enter.seat_index = 0;
        request_enter.player_clip = 0;
        request_enter.player_gold = CoActor.Def.PropGold.get();
        request_enter.is_vip = CoActor.Def.mPropIsVIP.get();

        // Step2：进入桌子并获取桌子初始化信息
        var grain_desktop = grain.GF.GetGrain<ICellDesktop>(new Guid(desktop_info.desktop_etguid));
        DesktopData desktop_data = await grain_desktop.s2sPlayerEnter(request_enter, CoPlayer.exportEtPlayerMirror());

        IsEntering = false;
        DesktopEtGuid = desktop_data.desktop_cfg_data.desktop_etguid;

        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.DesktopInit;
        desktop_notify.data = EbTool.protobufSerialize<DesktopData>(desktop_data);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerDesktopNotify;
        notify_data.param1 = EbTool.protobufSerialize<DesktopNotify>(desktop_notify);
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //---------------------------------------------------------------------
    // 进入桌子
    public async Task enterDesktop(string desktop_etguid)
    {
        if (IsEntering)
        {
            EbLog.Note("CellPlayerDesktop.enterDesktop() 错误：正在进入中");
            return;
        }
        IsEntering = true;

        if (!string.IsNullOrEmpty(DesktopEtGuid))
        {
            await leaveDesktop();
        }

        DesktopRequestPlayerEnter request_enter;
        request_enter.desktop_etguid = desktop_etguid;
        request_enter.seat_index = 255;
        request_enter.player_clip = 0;
        request_enter.player_gold = CoActor.Def.PropGold.get();
        request_enter.is_vip = CoActor.Def.mPropIsVIP.get();

        var grain = Entity.getUserData<GrainCellPlayer>();

        // Step1：监听桌子广播
        await _createDesktopStreamConsumer(desktop_etguid);

        // 自动下注
        int bet_chip = 2000;
        int left_chip = CoActor.Def.PropGold.get();
        if (left_chip < bet_chip) bet_chip = left_chip;
        left_chip -= bet_chip;
        CoPlayer.CoActor.Def.PropGold.set(left_chip);

        // Step2：进入桌子并获取桌子初始化信息
        var grain_desktop = grain.GF.GetGrain<ICellDesktop>(new Guid(desktop_etguid));
        DesktopData desktop_data = await grain_desktop.s2sPlayerEnter(request_enter, CoPlayer.exportEtPlayerMirror());

        IsEntering = false;
        DesktopEtGuid = desktop_data.desktop_cfg_data.desktop_etguid;

        DesktopNotify desktop_notify;
        desktop_notify.id = DesktopNotifyId.DesktopInit;
        desktop_notify.data = EbTool.protobufSerialize<DesktopData>(desktop_data);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerDesktopNotify;
        notify_data.param1 = EbTool.protobufSerialize<DesktopNotify>(desktop_notify);
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //---------------------------------------------------------------------
    // 离开桌子
    public async Task leaveDesktop()
    {
        if (string.IsNullOrEmpty(DesktopEtGuid)) return;

        // Step1：离开桌子
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_desktop = grain.GF.GetGrain<ICellDesktop>(new Guid(DesktopEtGuid));
        DesktopEtGuid = null;

        var leave_info = await grain_desktop.s2sPlayerLeave(Entity.Guid);

        // 收回筹码
        int chip = CoActor.Def.PropGold.get();
        chip += leave_info.stack;
        CoPlayer.CoActor.Def.PropGold.set(chip);

        // 更新游戏局数统计信息
        if (leave_info.game_total > 0)
        {
            int game_total = CoActor.Def.mPropGameTotal.get();
            game_total += leave_info.game_total;
            CoActor.Def.mPropGameTotal.set(game_total);
        }

        if (leave_info.game_win > 0)
        {
            int game_win = CoActor.Def.mPropGameWin.get();
            game_win += leave_info.game_win;
            CoActor.Def.mPropGameWin.set(game_win);
        }

        if (leave_info.exp > 0)
        {
            int exp = CoActor.Def.mPropExperience.get();
            exp += leave_info.exp;
            CoActor.Def.mPropExperience.set(exp);
        }

        // Step2：取消监听桌子广播
        await _destroyDesktopStreamConsumer();
    }

    //---------------------------------------------------------------------
    public Task c2sSendDesktopChat(ChatMsgRecv msg_recv)
    {
        if (!string.IsNullOrEmpty(DesktopEtGuid))
        {
            var grain = Entity.getUserData<GrainCellPlayer>();
            var grain_desktop = grain.GF.GetGrain<ICellDesktop>(new Guid(DesktopEtGuid));
            grain_desktop.s2sDesktopChat(msg_recv);
        }

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    async Task _createDesktopStreamConsumer(string et_desktop_guid)
    {
        if (ConsumerHandle == null)
        {
            var grain = Entity.getUserData<GrainCellPlayer>();
            var consumer_observer = new CellPlayerDesktopConsumerObserver<StreamData>(this);
            IStreamProvider stream_provider = grain.getStreamProvider();
            IAsyncObservable<StreamData> async_observable = stream_provider.GetStream<StreamData>(new Guid(et_desktop_guid), null);
            ConsumerHandle = await async_observable.SubscribeAsync(consumer_observer);
        }
    }

    //---------------------------------------------------------------------
    async Task _destroyDesktopStreamConsumer()
    {
        if (ConsumerHandle != null)
        {
            await ConsumerHandle.UnsubscribeAsync();
            ConsumerHandle = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;
using GF.Common;
using GF.Server;
using Ps;

internal class CellPlayerFriendConsumerObserver<StreamData> : IAsyncObserver<Ps.StreamData>
{
    //---------------------------------------------------------------------
    CellPlayerFriend<DefPlayerFriend> CoPlayerFriend { get; set; }

    //---------------------------------------------------------------------
    internal CellPlayerFriendConsumerObserver(IComponent co_playerfriend)
    {
        this.CoPlayerFriend = (CellPlayerFriend<DefPlayerFriend>)co_playerfriend;
    }

    //---------------------------------------------------------------------
    public Task OnNextAsync(Ps.StreamData item, StreamSequenceToken token = null)
    {
        return CoPlayerFriend.onFriendStreamEvent(item);
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

public class CellPlayerFriend<TDef> : Component<TDef> where TDef : DefPlayerFriend, new()
{
    //-------------------------------------------------------------------------
    public float GetOnlinePlayersElapsedTm { get; set; }// 定时推荐玩家30秒
    CellPlayer<DefPlayer> CoPlayer { get; set; }
    Dictionary<string, StreamSubscriptionHandle<StreamData>> MapFriendConsumerHandle { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        GetOnlinePlayersElapsedTm = 30f;
        CoPlayer = Entity.getComponent<CellPlayer<DefPlayer>>();
        MapFriendConsumerHandle = new Dictionary<string, StreamSubscriptionHandle<StreamData>>();

        Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();
        foreach (var i in map_friend)
        {
            _createFriendStreamConsumer(i.Key);
        }

        PlayerFriendNotify friend_notify;
        friend_notify.id = PlayerFriendNotifyId.None;
        friend_notify.data = null;// EbTool.protobufSerialize<ChatMsgRecv>(msg);
        _broadcastFriendNotify(friend_notify);
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        _destroyAllFriendStreamConsumer();
    }

    //-------------------------------------------------------------------------
    public override async void update(float elapsed_tm)
    {
        GetOnlinePlayersElapsedTm += elapsed_tm;
        if (GetOnlinePlayersElapsedTm > 30f)
        {
            GetOnlinePlayersElapsedTm = 0f;

            var grain = Entity.getUserData<GrainCellPlayer>();
            var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
            List<PlayerInfo> list_recommend = await grain_playerservice.getOnlinePlayers(Entity.Guid);

            PlayerFriendNotify friend_notify;
            friend_notify.id = PlayerFriendNotifyId.RecommendPlayerList;
            friend_notify.data = EbTool.protobufSerialize<List<PlayerInfo>>(list_recommend);

            MethodData notify_data = new MethodData();
            notify_data.method_id = MethodType.s2cPlayerFriendNotify;
            notify_data.param1 = EbTool.protobufSerialize<PlayerFriendNotify>(friend_notify);
            grain.Subscribers.Notify((s) => s.s2cNotify(notify_data));
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //-------------------------------------------------------------------------
    public async Task<MethodData> c2sPlayerFriendRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var playerfriend_request = EbTool.protobufDeserialize<PlayerFriendRequest>(method_data.param1);
        switch (playerfriend_request.id)
        {
            case PlayerFriendRequestId.GetPlayerInfoFriend:// 请求获取好友玩家信息
                {
                    var player_etguid = EbTool.protobufDeserialize<string>(playerfriend_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    var player_info = await grain_playerservice.getPlayerInfoFriend(player_etguid);

                    PlayerFriendResponse playerfriend_response;
                    playerfriend_response.id = PlayerFriendResponseId.GetPlayerInfoFriend;
                    playerfriend_response.data = EbTool.protobufSerialize<PlayerInfoFriend>(player_info);

                    result.method_id = MethodType.s2cPlayerFriendResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerFriendResponse>(playerfriend_response);
                }
                break;
            case PlayerFriendRequestId.RequestAddFriend:// 请求添加为好友
                {
                    var et_player_guid = EbTool.protobufDeserialize<string>(playerfriend_request.data);

                    ProtocolResult r = ProtocolResult.Failed;

                    // 不可以添加自己为好友
                    if (et_player_guid == Entity.Guid)
                    {
                        r = ProtocolResult.FriendIsMe;
                        goto End;
                    }

                    // 不可以重复添加好友
                    PlayerInfo current_friend = null;
                    Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();
                    if (map_friend.TryGetValue(et_player_guid, out current_friend))
                    {
                        r = ProtocolResult.FriendExistFriend;
                        goto End;
                    }

                    // 请求添加好友。由于加好友是双向的，需要告知被加的好友
                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(et_player_guid));
                    grain_playerproxy.s2sRequestAddFriend(Entity.Guid);
                    r = ProtocolResult.Success;

                    End:
                    PlayerFriendResponse playerfriend_response;
                    playerfriend_response.id = PlayerFriendResponseId.RequestAddFriend;
                    playerfriend_response.data = EbTool.protobufSerialize<ProtocolResult>(r);

                    result.method_id = MethodType.s2cPlayerFriendResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerFriendResponse>(playerfriend_response);
                }
                break;
            case PlayerFriendRequestId.AgreeAddFriend:// 请求是否同意添加好友
                {
                    var addfriend_agree = EbTool.protobufDeserialize<AddFriendAgree>(playerfriend_request.data);

                    ProtocolResult r = ProtocolResult.Failed;
                    PlayerInfo friend_item = null;
                    Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    PlayerInfo player_info = await grain_playerservice.getPlayerInfo(addfriend_agree.player_etguid);

                    if (player_info != null && !string.IsNullOrEmpty(player_info.player_etguid))
                    {
                        //friend_item = new FriendItem();
                        //friend_item.et_guid = player_info.player_etguid;
                        //friend_item.nick_name = player_info.nick_name;
                        //friend_item.icon = player_info.icon;
                        //friend_item.level = player_info.level;
                        //friend_item.exp = player_info.exp;
                        //friend_item.chip = player_info.chip;
                        //friend_item.gold = player_info.gold;
                        //friend_item.individual_signature = player_info.individual_signature;// 个性签名
                        //friend_item.ip_address = player_info.ip_address;// ip所在地
                        //friend_item.is_vip = false;// 是否是VIP
                        //friend_item.vip_level = 0;// VIP等级
                        //friend_item.vip_point = 0;// VIP积分
                        //friend_item.status = FriendOnlineStatus.Offline;// 好友在线状态
                        //friend_item.last_online_dt = DateTime.Now;// 好友最后在线时间
                        //friend_item.desktop_etguid = "";// 如果好友正在牌桌中，该牌桌的EtGuid

                        r = ProtocolResult.Success;

                        if (addfriend_agree.agree)
                        {
                            // 好友数据添加到好友列表
                            map_friend[addfriend_agree.player_etguid] = friend_item;

                            // 通知本地客户端添加好友
                            PlayerFriendNotify friend_notify;
                            friend_notify.id = PlayerFriendNotifyId.AddFriend;
                            friend_notify.data = EbTool.protobufSerialize(friend_item);
                            _onFriendNotify(friend_notify);

                            // 由于加好友是双向的，需要告知被加的好友
                            var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(addfriend_agree.player_etguid));
                            grain_playerproxy.s2sResponseAddFriend(Entity.Guid, addfriend_agree.agree);
                        }
                    }

                    End:
                    PlayerFriendResponse playerfriend_response;
                    playerfriend_response.id = PlayerFriendResponseId.AgreeAddFriend;
                    playerfriend_response.data = EbTool.protobufSerialize<ProtocolResult>(r);

                    result.method_id = MethodType.s2cPlayerFriendResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerFriendResponse>(playerfriend_response);
                }
                break;
            case PlayerFriendRequestId.DeleteFriend:// 请求删除好友
                {
                    var et_player_guid = EbTool.protobufDeserialize<string>(playerfriend_request.data);

                    ProtocolResult r = ProtocolResult.Failed;
                    PlayerInfo current_friend = null;
                    Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();
                    if (map_friend.TryGetValue(et_player_guid, out current_friend))
                    {
                        map_friend.Remove(current_friend.player_etguid);
                        r = ProtocolResult.Success;
                    }
                    else
                    {
                        goto End;
                    }

                    // 通知本地客户端删除好友
                    PlayerFriendNotify friend_notify;
                    friend_notify.id = PlayerFriendNotifyId.DeleteFriend;
                    friend_notify.data = EbTool.protobufSerialize(et_player_guid);
                    _onFriendNotify(friend_notify);

                    // 告知被删的好友
                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(et_player_guid));
                    grain_playerproxy.s2sDeleteFriend(Entity.Guid);

                    End:
                    PlayerFriendResponse playerfriend_response;
                    playerfriend_response.id = PlayerFriendResponseId.DeleteFriend;
                    playerfriend_response.data = EbTool.protobufSerialize<ProtocolResult>(r);

                    result.method_id = MethodType.s2cPlayerFriendResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerFriendResponse>(playerfriend_response);
                }
                break;
            case PlayerFriendRequestId.FindFriend:// 请求查找好友
                {
                    var find_info = EbTool.protobufDeserialize<string>(playerfriend_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);

                    var list_playerinfo = await grain_playerservice.findPlayers(find_info);

                    //List<FriendItem> list_frienditem = new List<FriendItem>();
                    //foreach (var i in list_playerinfo)
                    //{
                    //    PlayerInfo player_info = i;
                    //    if (string.IsNullOrEmpty(player_info.player_etguid)) continue;
                    //    FriendItem friend_item = new FriendItem();
                    //    friend_item.et_guid = player_info.player_etguid;
                    //    friend_item.nick_name = player_info.nick_name;
                    //    friend_item.icon = player_info.icon;
                    //    friend_item.level = player_info.level;
                    //    friend_item.exp = player_info.exp;
                    //    friend_item.chip = player_info.chip;
                    //    friend_item.gold = player_info.gold;
                    //    friend_item.individual_signature = player_info.individual_signature;// 个性签名
                    //    friend_item.ip_address = player_info.ip_address;// ip所在地
                    //    friend_item.is_vip = false;// 是否是VIP
                    //    friend_item.vip_level = 0;// VIP等级
                    //    friend_item.vip_point = 0;// VIP积分
                    //    friend_item.status = FriendOnlineStatus.Offline;// 好友在线状态
                    //    friend_item.last_online_dt = DateTime.Now;// 好友最后在线时间
                    //    friend_item.desktop_etguid = "";// 如果好友正在牌桌中，该牌桌的EtGuid

                    //    list_frienditem.Add(friend_item);
                    //}

                    PlayerFriendResponse playerfriend_response;
                    playerfriend_response.id = PlayerFriendResponseId.FindFriend;
                    playerfriend_response.data = EbTool.protobufSerialize<List<PlayerInfo>>(list_playerinfo);

                    result.method_id = MethodType.s2cPlayerFriendResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerFriendResponse>(playerfriend_response);
                }
                break;
            default:
                break;
        }

        return result;
    }

    //---------------------------------------------------------------------
    public async Task onFriendStreamEvent(StreamData data)
    {
        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.None;

        switch (data.event_id)
        {
            case StreamEventId.FriendStreamEvent:
                {
                    var friend_notify = (PlayerFriendNotify)data.param1;
                    switch (friend_notify.id)
                    {
                        case PlayerFriendNotifyId.OnFriendLogin:
                            {
                                //var d = EbTool.protobufDeserialize<DesktopNotifyDesktopPreFlopS>(desktop_notify.data);

                                //DesktopNotify dn;
                                //dn.id = DesktopNotifyId.DesktopPreFlop;
                                //dn.data = EbTool.protobufSerialize(desktop_preflop);
                                //notify_data.method_id = MethodType.s2cPlayerDesktopNotify;
                                //notify_data.param1 = EbTool.protobufSerialize(dn);
                            }
                            break;
                        default:
                            {
                                notify_data.method_id = MethodType.s2cPlayerDesktopNotify;
                                notify_data.param1 = EbTool.protobufSerialize(friend_notify);
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
    // 请求加好友
    public Task s2sProxyRequestAddFriend(PlayerInfo player_info)
    {
        return CoPlayer.CoPlayerMailBox.s2sProxyRequestAddFriend(player_info);
    }

    //---------------------------------------------------------------------
    // 响应加好友
    public Task s2sProxyResponseAddFriend(PlayerInfo player_info, bool agree)
    {
        if (!agree) return TaskDone.Done;

        s2sAddFriend(player_info);

        return CoPlayer.CoPlayerMailBox.s2sProxyResponseAddFriend(player_info, agree);
    }

    //---------------------------------------------------------------------
    // 删除好友
    public Task s2sProxyDeleteFriend(string friend_etguid)
    {
        Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();
        map_friend.Remove(friend_etguid);

        // 通知本地客户端删除好友
        PlayerFriendNotify friend_notify;
        friend_notify.id = PlayerFriendNotifyId.DeleteFriend;
        friend_notify.data = EbTool.protobufSerialize(friend_etguid);
        _onFriendNotify(friend_notify);

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    public Task s2sAddFriend(PlayerInfo player_info)
    {
        //var grain = Entity.getUserData<GrainCellPlayer>();
        //var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
        //PlayerInfo player_info = await grain_playerservice.getPlayerInfo(friend_etguid);

        //if (player_info != null && !string.IsNullOrEmpty(player_info.player_etguid))
        //{
        // 好友数据添加到好友列表
        Dictionary<string, PlayerInfo> map_friend = Def.mPropMapFriend.get();
        map_friend[player_info.player_etguid] = player_info;

        // 通知本地客户端添加好友
        PlayerFriendNotify friend_notify;
        friend_notify.id = PlayerFriendNotifyId.AddFriend;
        friend_notify.data = EbTool.protobufSerialize(player_info);
        _onFriendNotify(friend_notify);
        //}

        return TaskDone.Done;
    }

    //-------------------------------------------------------------------------
    void _onFriendNotify(PlayerFriendNotify friend_notify)
    {
        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerFriendNotify;
        notify_data.param1 = EbTool.protobufSerialize<PlayerFriendNotify>(friend_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);
    }

    //-------------------------------------------------------------------------
    void _broadcastFriendNotify(PlayerFriendNotify friend_notify)
    {
        StreamData sd = new StreamData();
        sd.event_id = StreamEventId.FriendStreamEvent;
        sd.param1 = friend_notify;
        var grain = Entity.getUserData<GrainCellPlayer>();
        grain.AsyncStream.OnNextAsync(sd);
    }

    //---------------------------------------------------------------------
    async Task _createFriendStreamConsumer(string friend_etguid)
    {
        StreamSubscriptionHandle<StreamData> consumer_handle = null;
        MapFriendConsumerHandle.TryGetValue(friend_etguid, out consumer_handle);
        if (consumer_handle != null) return;

        var grain = Entity.getUserData<GrainCellPlayer>();
        var consumer_observer = new CellPlayerFriendConsumerObserver<StreamData>(this);
        IStreamProvider stream_provider = grain.getStreamProvider();
        IAsyncObservable<StreamData> async_observable = stream_provider.GetStream<StreamData>(new Guid(friend_etguid), null);
        consumer_handle = await async_observable.SubscribeAsync(consumer_observer);

        MapFriendConsumerHandle[friend_etguid] = consumer_handle;

        // 主动查询好友信息
    }

    //---------------------------------------------------------------------
    Task _destroyFriendStreamConsumer(string friend_etguid)
    {
        StreamSubscriptionHandle<StreamData> consumer_handle = null;
        MapFriendConsumerHandle.TryGetValue(friend_etguid, out consumer_handle);
        if (consumer_handle != null)
        {
            consumer_handle.UnsubscribeAsync();
            MapFriendConsumerHandle.Remove(friend_etguid);
        }

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    Task _destroyAllFriendStreamConsumer()
    {
        foreach (var i in MapFriendConsumerHandle)
        {
            i.Value.UnsubscribeAsync();
        }
        MapFriendConsumerHandle.Clear();

        return TaskDone.Done;
    }
}

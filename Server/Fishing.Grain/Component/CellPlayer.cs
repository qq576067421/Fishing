using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

class IPAdderssData
{
    public string country = null;
    public string country_id = null;
    public string area = null;
    public string area_id = null;
    public string region = null;
    public string region_id = null;
    public string city = null;
    public string city_id = null;
    public string county = null;
    public string county_id = null;
    public string isp = null;
    public string isp_id = null;
    public string ip = null;
}

class IPCheckResult
{
    public int code = 0;
    public IPAdderssData data = null;
}

// CellPlayer消息，挂机，手动状态变更
public class EvCellPlayerSetAFK : EntityEvent
{
    public EvCellPlayerSetAFK() : base() { }
    public bool is_afk;
}

public class CellPlayer<TDef> : Component<TDef> where TDef : DefPlayer, new()
{
    //-------------------------------------------------------------------------
    public CellActor<DefActor> CoActor { get; private set; }
    public CellEquip<DefEquip> CoEquip { get; private set; }
    public CellBag<DefBag> CoBag { get; private set; }
    public CellStatus<DefStatus> CoStatus { get; private set; }
    public CellPlayerChat<DefPlayerChat> CoPlayerChat { get; private set; }
    public CellPlayerFriend<DefPlayerFriend> CoPlayerFriend { get; private set; }
    public CellPlayerTask<DefPlayerTask> CoPlayerTask { get; private set; }
    public CellPlayerDesktop<DefPlayerDesktop> CoPlayerDesktop { get; private set; }
    public CellPlayerLobby<DefPlayerLobby> CoPlayerLobby { get; private set; }
    public CellPlayerRanking<DefPlayerRanking> CoPlayerRanking { get; private set; }
    public CellPlayerMailBox<DefPlayerMailBox> CoPlayerMailBox { get; private set; }
    public bool IsNewPlayer { get; private set; }
    public string CachePlayerKey { get; private set; }
    public CachePlayerData CachePlayerData { get; private set; }// 玩家状态：在线，托管，离线
    Dictionary<MethodType, Func<MethodData, Task<MethodData>>> MapFunc { get; set; }
    IPEndPoint RemoteEndPoint { get; set; }
    float PlayerProxyTouchElapsedTm { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        EbLog.Note("CellPlayer.init() PlayerEtGuid=" + Entity.Guid);

        PlayerProxyTouchElapsedTm = 0f;

        MapFunc = new Dictionary<MethodType, Func<MethodData, Task<MethodData>>>();
        MapFunc[MethodType.c2sPlayerRequest] = _c2sPlayerRequest;
        MapFunc[MethodType.c2sBagRequest] = _c2sBagRequest;
        MapFunc[MethodType.c2sEquipRequest] = _c2sEquipRequest;
        MapFunc[MethodType.c2sStatusRequest] = _c2sStatusRequest;
        MapFunc[MethodType.c2sPlayerChatRequest] = _c2sPlayerChatRequest;
        MapFunc[MethodType.c2sPlayerFriendRequest] = _c2sPlayerFriendRequest;
        MapFunc[MethodType.c2sPlayerTaskRequest] = _c2sPlayerTaskRequest;
        MapFunc[MethodType.c2sPlayerMailBoxRequest] = _c2sPlayerMailBoxRequest;
        MapFunc[MethodType.c2sPlayerTradeRequest] = _c2sPlayerTradeRequest;
        MapFunc[MethodType.c2sPlayerDesktopRequest] = _c2sPlayerDesktopRequest;
        MapFunc[MethodType.c2sPlayerLobbyRequest] = _c2sPlayerLobbyRequest;
        MapFunc[MethodType.c2sPlayerRankingRequest] = _c2sPlayerRankingRequest;

        // 是否是新号
        object o = Entity.getCacheData("NewPlayer");
        if (o != null)
        {
            IsNewPlayer = (bool)o;
        }

        // CellActor组件
        CoActor = Entity.getComponent<CellActor<DefActor>>();
        CoActor.initActor();
        if (IsNewPlayer)
        {
            ulong player_id = (ulong)Entity.getCacheData("NewPlayerId");
            CoActor.Def.mPropActorId.set(player_id);
            CoActor.Def.mPropJoinDateTime.set(DateTime.Now);

            // 随机Icon
            int icon = CellApp.Instance.Rd.next(100, 126);
            CoActor.Def.mPropIcon.set(icon.ToString());

            var new_player_info = (NewPlayerInfo)Entity.getCacheData("NewPlayerInfo");
            CoActor.Def.mPropAccountId.set(new_player_info.account_id);
            CoActor.Def.mPropAccountName.set(new_player_info.account_name);
            CoActor.Def.mPropNickName.set(new_player_info.account_name);
            CoActor.Def.mPropGender.set(new_player_info.gender);
            CoActor.Def.mPropIsBot.set(new_player_info.is_bot);
        }

        // 20级之前，不可以挂机
        int actor_level = CoActor.Def.mPropLevel.get();
        if (actor_level <= CellApp.Instance.Cfg.GlobalAFKLevelLimit && CoActor.Def.mPropIsAFK.get())
        {
            CoActor.Def.mPropIsAFK.set(false);
        }

        // CellBag组件
        CoBag = Entity.getComponent<CellBag<DefBag>>();

        // CellEquip组件
        CoEquip = Entity.getComponent<CellEquip<DefEquip>>();

        // CellStatus组件
        CoStatus = Entity.getComponent<CellStatus<DefStatus>>();

        // CellPlayerChat组件
        CoPlayerChat = Entity.getComponent<CellPlayerChat<DefPlayerChat>>();

        // CellPlayerFriend组件
        CoPlayerFriend = Entity.getComponent<CellPlayerFriend<DefPlayerFriend>>();

        // CellPlayerTask组件
        CoPlayerTask = Entity.getComponent<CellPlayerTask<DefPlayerTask>>();

        // CellPlayerDesktop组件
        CoPlayerDesktop = Entity.getComponent<CellPlayerDesktop<DefPlayerDesktop>>();

        // CellPlayerLobby组件
        CoPlayerLobby = Entity.getComponent<CellPlayerLobby<DefPlayerLobby>>();

        // CellPlayerRanking组件
        CoPlayerRanking = Entity.getComponent<CellPlayerRanking<DefPlayerRanking>>();

        // CellPlayerMailBox组件
        CoPlayerMailBox = Entity.getComponent<CellPlayerMailBox<DefPlayerMailBox>>();

        CachePlayerData = new Ps.CachePlayerData();
        CachePlayerData.player_etguid = Entity.Guid;
        CachePlayerData.player_server_state = PlayerServerState.Hosting;

        CachePlayerKey = "CachePlayerData_" + Entity.Guid;

        string data = EbTool.jsonSerialize(CachePlayerData);
        DbClientCouchbase.Instance.asyncSave(CachePlayerKey, data, TimeSpan.FromSeconds(15.0));
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        CoActor = null;
        CoEquip = null;
        CoBag = null;
        CoStatus = null;
        CoPlayerChat = null;
        CoPlayerFriend = null;
        CoPlayerTask = null;
        CoPlayerDesktop = null;
        CoPlayerLobby = null;

        DbClientCouchbase.Instance.asyncRemove(CachePlayerKey);

        EbLog.Note("CellPlayer.release() PlayerEtGuid=" + Entity.Guid);
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        // 定时通知PlayerProxy，玩家在线
        if (CachePlayerData.player_server_state == PlayerServerState.Online)
        {
            PlayerProxyTouchElapsedTm += elapsed_tm;
            if (PlayerProxyTouchElapsedTm > 30f)
            {
                var grain = Entity.getUserData<GrainCellPlayer>();
                var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(Entity.Guid));
                grain_playerproxy.s2sPlayerServerStateChange(CachePlayerData.player_server_state);
            }
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
        if (e is EvCellActorPropDirty)
        {
            // CellActor，角色属性集变脏
            var ev = (EvCellActorPropDirty)e;

            // 通知客户端Actor应用脏属性
            if (!CoActor.Def.mPropIsBot.get())
            {
                PlayerNotify player_notify;
                player_notify.id = PlayerNotifyId.ActorMapPropDirty;
                player_notify.data = EbTool.protobufSerialize<Dictionary<string, string>>(ev.map_prop_dirty);

                MethodData notify_data = new MethodData();
                notify_data.method_id = MethodType.s2cPlayerNotify;
                notify_data.param1 = EbTool.protobufSerialize<PlayerNotify>(player_notify);
                var grain = Entity.getUserData<GrainCellPlayer>();
                var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
                grain_player.s2sNotify(notify_data);
            }
        }
        else if (e is EvCellActorLevelup)
        {
            // CellActor消息，角色升级
            var ev = (EvCellActorLevelup)e;

            // 通知客户端Actor应用脏属性
            if (!CoActor.Def.mPropIsBot.get())
            {
                PlayerNotify player_notify;
                player_notify.id = PlayerNotifyId.Levelup;
                player_notify.data = EbTool.protobufSerialize<int>(ev.level_new);

                MethodData notify_data = new MethodData();
                notify_data.method_id = MethodType.s2cPlayerNotify;
                notify_data.param1 = EbTool.protobufSerialize<PlayerNotify>(player_notify);
                var grain = Entity.getUserData<GrainCellPlayer>();
                var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
                grain_player.s2sNotify(notify_data);
            }
        }
        else if (e is EvCellBagAddItem)
        {
            // CellBag，添加道具消息
            var ev = (EvCellBagAddItem)e;

            // 通知客户端添加道具
            if (!CoActor.Def.mPropIsBot.get())
            {
                CoBag.s2cBagNotifyAddItem(ev.item.ItemData);
            }
        }
        else if (e is EvCellBagDeleteItem)
        {
            // CellBag，删除道具消息
            var ev = (EvCellBagDeleteItem)e;

            // 通知客户端删除道具
            if (!CoActor.Def.mPropIsBot.get())
            {
                CoBag.s2cBagNotifyDeleteItem(ev.item_objid);
            }
        }
        else if (e is EvCellBagUpdateItem)
        {
            // CellBag，更新道具消息
            var ev = (EvCellBagUpdateItem)e;

            // 通知客户端更新道具
            if (!CoActor.Def.mPropIsBot.get())
            {
                CoBag.s2cBagNotifyUpdateItem(ev.item.ItemData);
            }
        }
        else if (e is EvCellEquipTakeonEquip)
        {
            // CellEquip，穿装备消息
            var ev = (EvCellEquipTakeonEquip)e;

            // 通知客户端穿上装备
            if (!CoActor.Def.mPropIsBot.get())
            {
                CoEquip.s2cEquipNotifyTakeon(ev.item.ItemData);
            }
        }
        else if (e is EvCellEquipTakeoffEquip)
        {
            // CellEquip，脱装备消息
            var ev = (EvCellEquipTakeoffEquip)e;

            // 通知客户端脱下装备
            if (!CoActor.Def.mPropIsBot.get())
            {
                CoEquip.s2cEquipNotifyTakeoff(ev.equip_slot);
            }
        }
        else if (e is EvCellStatusCreateStatus)
        {
            // CellStatus，创建状态消息
            var ev = (EvCellStatusCreateStatus)e;

            // 通知客户端创建Status
            if (!CoActor.Def.mPropIsBot.get())
            {
                CoStatus.s2cStatusNotifyCreateStatus(ev.item.ItemData);
            }
        }
    }

    //-------------------------------------------------------------------------
    public EntityData exportEtPlayerMirror()
    {
        EntityData et_data = new EntityData();
        et_data.entity_type = typeof(EtPlayerMirror).Name;
        et_data.entity_guid = Entity.Guid;
        et_data.list_component = new List<ComponentData>();

        // CoActorMirror
        {
            ComponentData component_data = new ComponentData();
            component_data.component_name = "DefActorMirror";
            component_data.def_propset = new Dictionary<string, string>();
            component_data.def_propset["ActorId"] = CoActor.Def.mPropActorId.toJsonString();
            component_data.def_propset["IsBot"] = CoActor.Def.mPropIsBot.toJsonString();
            component_data.def_propset["NickName"] = CoActor.Def.mPropNickName.toJsonString();
            component_data.def_propset["Icon"] = CoActor.Def.mPropIcon.toJsonString();
            component_data.def_propset["IpAddress"] = CoActor.Def.mPropIpAddress.toJsonString();
            component_data.def_propset["Gold"] = CoActor.Def.PropGold.toJsonString();
            et_data.list_component.Add(component_data);
        }

        return et_data;
    }

    //-------------------------------------------------------------------------
    // 检查游戏币是否足够
    public bool hasEnoughCoin(ulong silver_coin)
    {
        return false;
    }

    //-------------------------------------------------------------------------
    public Dictionary<string, string> getMapData4SwitchDesktop()
    {
        Dictionary<string, string> m = new Dictionary<string, string>();
        m["IsAFK"] = CoActor.Def.mPropIsAFK.toJsonString();
        return m;
    }

    //-------------------------------------------------------------------------
    // 收到玩家本人Couchbase队列中的消息
    public async Task recvCouchbaseQueData(CouchbaseQueData que_data)
    {
        CouchbaseQueDataType que_data_type = (CouchbaseQueDataType)que_data.type;
        EbLog.Note("CellPlayer.recvCouchbaseQueData() DataType=" + que_data_type);
        switch (que_data_type)
        {
            case CouchbaseQueDataType.InvitePlayerEnterDesktop:// 收到好友进桌邀请
                {
                    string from_friend_etguid = que_data.map_data["from_friend_etguid"];
                    string desktop_etguid = que_data.map_data["desktop_etguid"];
                    int sb = int.Parse(que_data.map_data["sb"]);
                    int bb = int.Parse(que_data.map_data["bb"]);
                    int player_num = int.Parse(que_data.map_data["player_num"]);
                    int seat_num = int.Parse(que_data.map_data["seat_num"]);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    var player_info = await grain_playerservice.getPlayerInfo(from_friend_etguid);

                    s2sProxyInvitePlayerEnterDesktop(player_info, desktop_etguid, sb, bb, player_num, seat_num);
                }
                break;
            case CouchbaseQueDataType.GivePlayerChip:// 收到玩家赠送的筹码
                {
                    string from_player_etguid = que_data.map_data["from_player_etguid"];
                    int chip = int.Parse(que_data.map_data["chip"]);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    var player_info = await grain_playerservice.getPlayerInfo(from_player_etguid);

                    s2sProxyGivePlayerChip(player_info, chip);
                }
                break;
            case CouchbaseQueDataType.RequestAddFriend:// 请求加好友
                {
                    string request_player_etguid = que_data.map_data["request_player_etguid"];

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    var player_info = await grain_playerservice.getPlayerInfo(request_player_etguid);

                    var co_friend = Entity.getComponent<CellPlayerFriend<DefPlayerFriend>>();
                    co_friend.s2sProxyRequestAddFriend(player_info);
                }
                break;
            case CouchbaseQueDataType.ResponseAddFriend:// 响应加好友
                {
                    string response_player_etguid = que_data.map_data["response_player_etguid"];
                    bool agree = bool.Parse(que_data.map_data["agree"]);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    var player_info = await grain_playerservice.getPlayerInfo(response_player_etguid);

                    var co_friend = Entity.getComponent<CellPlayerFriend<DefPlayerFriend>>();
                    co_friend.s2sProxyResponseAddFriend(player_info, agree);
                }
                break;
            case CouchbaseQueDataType.DeleteFriend:// 删除好友
                {
                    string friend_etguid = que_data.map_data["friend_etguid"];

                    var co_friend = Entity.getComponent<CellPlayerFriend<DefPlayerFriend>>();
                    co_friend.s2sProxyDeleteFriend(friend_etguid);
                }
                break;
            case CouchbaseQueDataType.RecvChatFromFriend:// 好友聊天
                {
                    string s = que_data.map_data["msg_recv"];
                    ChatMsgRecv msg_recv = EbTool.jsonDeserialize<ChatMsgRecv>(s);

                    var co_chat = Entity.getComponent<CellPlayerChat<DefPlayerChat>>();
                    co_chat.s2sProxyRecvChatFromFriend(msg_recv);
                }
                break;
            case CouchbaseQueDataType.RecvMail:// 收到邮件
                {
                    string s = que_data.map_data["mail_data"];
                    MailData mail_data = EbTool.jsonDeserialize<MailData>(s);

                    var co_mailbox = Entity.getComponent<CellPlayerMailBox<DefPlayerMailBox>>();
                    co_mailbox.s2sProxyRecvMail(mail_data);
                }
                break;
            default:
                break;
        }
    }

    //---------------------------------------------------------------------
    // 收到玩家赠送的筹码
    public Task s2sProxyGivePlayerChip(PlayerInfo player_info, int chip)
    {
        GivePlayerChip give_chip = new GivePlayerChip();
        give_chip.player_info = player_info;
        give_chip.chip = chip;

        int chip_cur = CoActor.Def.PropGold.get();
        if (chip >= 100000)
        {
            chip_cur += chip;
            CoActor.Def.PropGold.set(chip_cur);
        }

        PlayerNotify player_notify;
        player_notify.id = PlayerNotifyId.GivePlayerChip;
        player_notify.data = EbTool.protobufSerialize<GivePlayerChip>(give_chip);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerNotify;
        notify_data.param1 = EbTool.protobufSerialize<PlayerNotify>(player_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);

        return TaskDone.Done;
    }

    //---------------------------------------------------------------------
    // 收到好友进桌邀请
    public Task s2sProxyInvitePlayerEnterDesktop(PlayerInfo player_info, string desktop_etguid,
            int sb, int bb, int player_num, int seat_num)
    {
        InvitePlayerEnterDesktop invite = new InvitePlayerEnterDesktop();
        invite.player_info = player_info;
        invite.desktop_etguid = desktop_etguid;
        invite.sb = sb;
        invite.bb = bb;
        invite.player_num = player_num;
        invite.seat_num = seat_num;

        PlayerNotify player_notify;
        player_notify.id = PlayerNotifyId.InvitePlayerEnterDesktop;
        player_notify.data = EbTool.protobufSerialize<InvitePlayerEnterDesktop>(invite);

        MethodData notify_data = new MethodData();
        notify_data.method_id = MethodType.s2cPlayerNotify;
        notify_data.param1 = EbTool.protobufSerialize<PlayerNotify>(player_notify);
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
        grain_player.s2sNotify(notify_data);

        return TaskDone.Done;
    }

    //-------------------------------------------------------------------------
    // 客户端连接
    public Task c2sClientAttach(IPEndPoint remote_endpoint)
    {
        EbLog.Note("CellPlayer.c2sClientAttach() EtGuid=" + Entity.Guid);

        PlayerProxyTouchElapsedTm = 0f;

        RemoteEndPoint = remote_endpoint;
        EbLog.Note("RemoteIp=" + RemoteEndPoint.Address.ToString());

        CachePlayerData.player_server_state = PlayerServerState.Online;

        string data = EbTool.jsonSerialize(CachePlayerData);
        DbClientCouchbase.Instance.asyncSave(CachePlayerKey, data, TimeSpan.FromSeconds(15.0));

        // 通知PlayerProxy，玩家上线
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(Entity.Guid));
        grain_playerproxy.s2sPlayerServerStateChange(CachePlayerData.player_server_state);

        return TaskDone.Done;

        // 获取天气信息
        //try
        //{
        //    WebClient client = new WebClient();
        //    string uri = "http://i.tianqi.com/index.php?c=code&id=1&icon=1&wind=1&num=2";
        //    string result_data = await client.DownloadStringTaskAsync(uri);
        //    //IPCheckResult result = EbTool.jsonDeserialize<IPCheckResult>(result_data);
        //    if (!string.IsNullOrEmpty(result_data))
        //    {
        //        EbLog.Note(result_data);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    EbLog.Note(ex.ToString());
        //}
    }

    //-------------------------------------------------------------------------
    // 客户端断开连接
    public async Task c2sClientDeattach()
    {
        EbLog.Note("CellPlayer.c2sClientDeattach() EtGuid=" + Entity.Guid);

        CachePlayerData.player_server_state = PlayerServerState.Offline;

        // 通知PlayerProxy，玩家离线
        var grain = Entity.getUserData<GrainCellPlayer>();
        var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(Entity.Guid));
        grain_playerproxy.s2sPlayerServerStateChange(CachePlayerData.player_server_state);

        string data = EbTool.jsonSerialize(CachePlayerData);
        DbClientCouchbase.Instance.asyncSave(CachePlayerKey, data, TimeSpan.FromSeconds(15.0));

        await CoPlayerDesktop.leaveDesktop();
    }

    //-------------------------------------------------------------------------
    // 玩家请求
    public Task<MethodData> c2sRequest(MethodData method_data)
    {
        Func<MethodData, Task<MethodData>> func = null;
        MapFunc.TryGetValue(method_data.method_id, out func);
        if (func != null)
        {
            return func(method_data);
        }
        else
        {
            // log error
            string error_msg = string.Format("CellPlayer.s2sPlayerRequest() EtGuid={0} Not Found MethodId={1}",
                Entity.Guid, method_data.method_id.ToString());
            EbLog.Error(error_msg);

            MethodData result = new MethodData();
            result.method_id = MethodType.None;
            return Task.FromResult(result);
        }
    }

    //-------------------------------------------------------------------------
    async Task<MethodData> _c2sPlayerRequest(MethodData method_data)
    {
        var player_request = EbTool.protobufDeserialize<PlayerRequest>(method_data.param1);
        switch (player_request.id)
        {
            case PlayerRequestId.PlayNow:// 立即玩
                {
                    EbLog.Note("CellPlayer._c2sPlayerRequest PlayNow() EtGuid=" + Entity.Guid);

                    // 通过桌子服务请求立即玩
                    await CoPlayerDesktop.enterDesktopPlayNow();
                }
                break;
            case PlayerRequestId.CreatePrivateDesktop:// c->s，创建私有桌子
                {
                    EbLog.Note("CellPlayer.c2sPlayerRequest() CreatePrivateDesktop");

                    // 正在进入桌子中
                    if (CoPlayerDesktop.IsEntering)
                    {
                        goto End;
                    }

                    // 检测玩家是否已在桌子中
                    if (!string.IsNullOrEmpty(CoPlayerDesktop.DesktopEtGuid))
                    {
                        goto End;
                    }

                    // 创建私人桌并入座
                    var desktop_createinfo = EbTool.protobufDeserialize<PrivateDesktopCreateInfo>(player_request.data);
                    await CoPlayerDesktop.enterDesktopPrivate(desktop_createinfo);

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.CreatePrivateDesktop;
                    player_response.data = null;

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.EnterDesktopAny:
                {
                    EbLog.Note("CellPlayer.c2sPlayerRequest EnterDesktopAny() EtGuid=" + Entity.Guid);

                    await CoPlayerDesktop.enterDesktopPlayNow();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.EnterDesktopAny;
                    player_response.data = null;

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.EnterDesktop:// 进入桌子
                {
                    EbLog.Note("CellPlayer.c2sPlayerRequest EnterDesktop() EtGuid=" + Entity.Guid);

                    var desktop_etguid = EbTool.protobufDeserialize<string>(player_request.data);
                    if (!string.IsNullOrEmpty(desktop_etguid))
                    {
                        await CoPlayerDesktop.enterDesktop(desktop_etguid);
                    }

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.EnterDesktop;
                    player_response.data = null;

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.LeaveDesktop:// 离开桌子
                {
                    EbLog.Note("CellPlayer.c2sPlayerRequest LeaveDesktop() EtGuid=" + Entity.Guid);

                    await CoPlayerDesktop.leaveDesktop();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.LeaveDesktop;
                    player_response.data = null;

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.GetOnlinePlayerNum:// 获取在线玩家数量
                {
                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    var num = await grain_playerservice.getOnlinePlayerNum();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.GetOnlinePlayerNum;
                    player_response.data = EbTool.protobufSerialize<int>(num);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.DevConsoleCmd:// 请求执行控制台命令
                {
                    var map_param = EbTool.protobufDeserialize<Dictionary<byte, string>>(player_request.data);
                    string cmd = (string)map_param[0];

                    if (cmd == "AddItem")
                    {
                        int item_id = int.Parse(map_param[1]);
                        int count = int.Parse(map_param[2]);
                        Item item = null;
                        CoBag.newItem(item_id, count, out item);
                    }
                    else if (cmd == "OperateItem")
                    {
                        string operate_id = map_param[1];
                        string item_objid = map_param[2];
                        ItemOperate item_operate = new ItemOperate();
                        item_operate.operate_id = operate_id;
                        item_operate.item_objid = item_objid;
                        CoBag.operateItem(item_operate);
                    }
                    else if (cmd == "SetLevel")
                    {
                        int level = int.Parse(map_param[1]);
                        int exp = int.Parse(map_param[2]);
                        CoActor.Def.mPropExperience.set(exp);
                        CoActor.Def.mPropLevel.set(level);
                    }
                }
                break;
            case PlayerRequestId.GetPlayerInfoOther:// 请求获取其他玩家信息
                {
                    var player_etguid = EbTool.protobufDeserialize<string>(player_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);
                    var player_info = await grain_playerservice.getPlayerInfoOther(player_etguid);

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.GetPlayerInfoOther;
                    player_response.data = EbTool.protobufSerialize<PlayerInfoOther>(player_info);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.ChangeProfileSkin:// 请求换肤
                {
                    var profileskin_tableid = EbTool.protobufDeserialize<int>(player_request.data);

                    var profileskin = EbDataMgr.Instance.getData<TbDataPlayerProfileSkin>(profileskin_tableid);
                    if (profileskin == null)
                    {
                        goto End;
                    }

                    CoActor.Def.mPropProfileSkinTableId.set(profileskin_tableid);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    grain.asyncSaveState();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.ChangeProfileSkin;
                    player_response.data = EbTool.protobufSerialize<int>(profileskin_tableid);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.ChangeNickName:// 请求改昵称
                {
                    var nick_name = EbTool.protobufDeserialize<string>(player_request.data);

                    CoActor.Def.mPropNickName.set(nick_name);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    grain.asyncSaveState();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.ChangeNickName;
                    player_response.data = EbTool.protobufSerialize<string>(nick_name);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.ChangeIndividualSignature:// 请求改签名
                {
                    var sign = EbTool.protobufDeserialize<string>(player_request.data);

                    CoActor.Def.mPropIndividualSignature.set(sign);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    grain.asyncSaveState();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.ChangeIndividualSignature;
                    player_response.data = EbTool.protobufSerialize<string>(sign);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.RefreshIpAddress:// 请求刷新Ip所在地
                {
                    _refreshIpAddress();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.RefreshIpAddress;
                    player_response.data = EbTool.protobufSerialize<string>(CoActor.Def.mPropIpAddress.get());

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.ReportPlayer:// 举报玩家
                {
                    var report = EbTool.protobufDeserialize<ReportPlayer>(player_request.data);

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.ReportPlayer;
                    player_response.data = EbTool.protobufSerialize<ReportPlayer>(report);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.InvitePlayerEnterDesktop:// 邀请玩家进桌
                {
                    var invite = EbTool.protobufDeserialize<InvitePlayerEnterDesktop>(player_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(invite.player_info.player_etguid));
                    grain_playerproxy.s2sInvitePlayerEnterDesktop(Entity.Guid, invite.desktop_etguid,
                        invite.sb, invite.bb, invite.player_num, invite.seat_num);

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.InvitePlayerEnterDesktop;
                    player_response.data = EbTool.protobufSerialize<ProtocolResult>(ProtocolResult.Success);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.GivePlayerChip:// 赠送玩家筹码
                {
                    var give_chip = EbTool.protobufDeserialize<GivePlayerChip>(player_request.data);

                    ProtocolResult re = ProtocolResult.Failed;
                    int chip_cur = CoActor.Def.PropGold.get();

                    if (chip_cur >= 100000 && give_chip.chip >= 100000 && chip_cur >= give_chip.chip)
                    {
                        var grain = Entity.getUserData<GrainCellPlayer>();
                        var grain_playerproxy = grain.GF.GetGrain<ICellPlayerProxy>(new Guid(give_chip.player_info.player_etguid));
                        await grain_playerproxy.s2sGivePlayerChip(Entity.Guid, give_chip.chip);

                        re = ProtocolResult.Success;
                        chip_cur -= give_chip.chip;
                        CoActor.Def.PropGold.set(chip_cur);
                    }
                    else
                    {
                        re = ProtocolResult.Failed;
                    }

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.GivePlayerChip;
                    player_response.data = EbTool.protobufSerialize<ProtocolResult>(re);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.SetVip4Test:// 请求设置是否为Vip
                {
                    EbLog.Note("CellPlayer.c2sPlayerRequest LeaveDesktop() EtGuid=" + Entity.Guid);

                    var is_vip = EbTool.protobufDeserialize<bool>(player_request.data);
                    CoActor.Def.mPropIsVIP.set(is_vip);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    grain.asyncSaveState();

                    PlayerResponse player_response;
                    player_response.id = PlayerResponseId.SetVip4Test;
                    player_response.data = EbTool.protobufSerialize(is_vip);

                    MethodData result = new MethodData();
                    result.method_id = MethodType.s2cPlayerResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerResponse>(player_response);
                    return result;
                }
            case PlayerRequestId.SetAFK:// 请求设置是否挂机
                {
                    var afk = EbTool.protobufDeserialize<bool>(player_request.data);
                    if (CoActor.Def.mPropIsBot.get())
                    {
                        // 机器人总是挂机状态，无需设置
                        goto End;
                    }

                    if (CoActor.Def.mPropIsAFK.get() != afk)
                    {
                        CoActor.Def.mPropIsAFK.set(afk);

                        PlayerNotify player_notify;
                        player_notify.id = PlayerNotifyId.SetAFK;
                        player_notify.data = EbTool.protobufSerialize<bool>(afk);

                        MethodData notify_data = new MethodData();
                        notify_data.method_id = MethodType.s2cPlayerNotify;
                        notify_data.param1 = EbTool.protobufSerialize<PlayerNotify>(player_notify);
                        var grain = Entity.getUserData<GrainCellPlayer>();
                        var grain_player = grain.GF.GetGrain<ICellPlayer>(new Guid(Entity.Guid));
                        grain_player.s2sNotify(notify_data);
                    }
                }
                break;
            default:
                break;
        }

        End:
        MethodData r = new MethodData();
        r.method_id = MethodType.None;
        return r;
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sBagRequest(MethodData method_data)
    {
        var co_bag = Entity.getComponent<CellBag<DefBag>>();
        return co_bag.c2sBagRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sEquipRequest(MethodData method_data)
    {
        var co_equip = Entity.getComponent<CellEquip<DefEquip>>();
        return co_equip.c2sEquipRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sStatusRequest(MethodData method_data)
    {
        var co_status = Entity.getComponent<CellStatus<DefStatus>>();
        return co_status.c2sStatusRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerChatRequest(MethodData method_data)
    {
        var co_chat = Entity.getComponent<CellPlayerChat<DefPlayerChat>>();
        return co_chat.c2sPlayerChatRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerFriendRequest(MethodData method_data)
    {
        var co_friend = Entity.getComponent<CellPlayerFriend<DefPlayerFriend>>();
        return co_friend.c2sPlayerFriendRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerTaskRequest(MethodData method_data)
    {
        var co_task = Entity.getComponent<CellPlayerTask<DefPlayerTask>>();
        return co_task.c2sPlayerTaskRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerMailBoxRequest(MethodData method_data)
    {
        var co_mailbox = Entity.getComponent<CellPlayerMailBox<DefPlayerMailBox>>();
        return co_mailbox.c2sPlayerMailBoxRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerTradeRequest(MethodData method_data)
    {
        var co_trade = Entity.getComponent<CellPlayerTrade<DefPlayerTrade>>();
        return co_trade.c2sPlayerTradeRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerDesktopRequest(MethodData method_data)
    {
        return CoPlayerDesktop.c2sPlayerDesktopRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerLobbyRequest(MethodData method_data)
    {
        return CoPlayerLobby.c2sPlayerLobbyRequest(method_data);
    }

    //-------------------------------------------------------------------------
    Task<MethodData> _c2sPlayerRankingRequest(MethodData method_data)
    {
        return CoPlayerRanking.c2sPlayerRankingRequest(method_data);
    }

    //-------------------------------------------------------------------------
    async Task _refreshIpAddress()
    {
        if (RemoteEndPoint != null)
        {
            try
            {
                WebClient client = new WebClient();
                string uri = @"http://ip.taobao.com/service/getIpInfo.php?ip=" + RemoteEndPoint.Address.ToString();//+ "139.196.105.80";
                string result_data = await client.DownloadStringTaskAsync(uri);
                IPCheckResult result = EbTool.jsonDeserialize<IPCheckResult>(result_data);
                if (result.code == 0)
                {
                    EbLog.Note("Country=" + result.data.country + " Area=" + result.data.area);
                    EbLog.Note("Region=" + result.data.region + " City=" + result.data.city + " County=" + result.data.county);

                    string ip_address = result.data.country + "." + result.data.city;
                    CoActor.Def.mPropIpAddress.set(ip_address);
                }
            }
            catch (Exception ex)
            {
                EbLog.Note(ex.ToString());
            }
        }
    }
}

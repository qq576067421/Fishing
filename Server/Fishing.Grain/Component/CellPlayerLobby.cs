using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellPlayerLobby<TDef> : Component<TDef> where TDef : DefPlayerLobby, new()
{
    //-------------------------------------------------------------------------
    CellPlayer<DefPlayer> CoPlayer { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        EnableSave2Db = false;
        EnableNetSync = false;

        CoPlayer = Entity.getComponent<CellPlayer<DefPlayer>>();
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
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
    public async Task<MethodData> c2sPlayerLobbyRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var lobby_request = EbTool.protobufDeserialize<PlayerLobbyRequest>(method_data.param1);
        switch (lobby_request.id)
        {
            case PlayerLobbyRequestId.SearchDesktop:// 搜索桌子
                {
                    EbLog.Note("CellPlayerLobby.c2sPlayerLobbyRequest() SearchDesktop");

                    var search_filter = EbTool.protobufDeserialize<DesktopSearchFilter>(lobby_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_desktopservice = grain.GF.GetGrain<ICellDesktopService>(0);
                    var list_desktop_info = await grain_desktopservice.searchDesktop(search_filter);

                    PlayerLobbyResponse lobby_response;
                    lobby_response.id = PlayerLobbyResponseId.SearchDesktop;
                    lobby_response.data = EbTool.protobufSerialize<List<DesktopInfo>>(list_desktop_info);

                    result.method_id = MethodType.s2cPlayerLobbyResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerLobbyResponse>(lobby_response);
                }
                break;
            case PlayerLobbyRequestId.SearchDesktopFollowFriend:// 搜索好友所在的牌桌
                {
                    EbLog.Note("CellPlayerLobby.c2sPlayerLobbyRequest() SearchDesktopFollowFriend");

                    var desktop_etguid = EbTool.protobufDeserialize<string>(lobby_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_desktopservice = grain.GF.GetGrain<ICellDesktopService>(0);
                    var list_desktop_info = await grain_desktopservice.searchDesktopFollowFriend(desktop_etguid);

                    PlayerLobbyResponse lobby_response;
                    lobby_response.id = PlayerLobbyResponseId.SearchDesktopFollowFriend;
                    lobby_response.data = EbTool.protobufSerialize<List<DesktopInfo>>(list_desktop_info);

                    result.method_id = MethodType.s2cPlayerLobbyResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerLobbyResponse>(lobby_response);
                }
                break;
            default:
                break;
        }

        return result;
    }
}

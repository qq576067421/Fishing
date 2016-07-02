using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellPlayerRanking<TDef> : Component<TDef> where TDef : DefPlayerRanking, new()
{
    //-------------------------------------------------------------------------
    CellPlayer<DefPlayer> CoPlayer { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
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
    public async Task<MethodData> c2sPlayerRankingRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var ranking_request = EbTool.protobufDeserialize<PlayerRankingRequest>(method_data.param1);
        switch (ranking_request.id)
        {
            case PlayerRankingRequestId.GetRankingList:
                {
                    EbLog.Note("CellPlayerRanking.c2sPlayerRankingRequest() GetRankingList");

                    var ranking_list_type = EbTool.protobufDeserialize<RankingListType>(ranking_request.data);

                    var grain = Entity.getUserData<GrainCellPlayer>();
                    var grain_playerservice = grain.GF.GetGrain<ICellPlayerService>(0);

                    PlayerRankingResponse ranking_response;
                    ranking_response.id = PlayerRankingResponseId.None;
                    ranking_response.data = null;

                    if (ranking_list_type == RankingListType.Chip)
                    {
                        var list_rankingchip = await grain_playerservice.getChipRankingList();
                        ranking_response.id = PlayerRankingResponseId.GetChipRankingList;
                        ranking_response.data = EbTool.protobufSerialize<List<RankingChip>>(list_rankingchip);
                    }
                    else if (ranking_list_type == RankingListType.VIPPoint)
                    {
                        var list_rankingvippoint = await grain_playerservice.getVIPPointRankingList();
                        ranking_response.id = PlayerRankingResponseId.GetVIPPointRankingList;
                        ranking_response.data = EbTool.protobufSerialize<List<RankingVIPPoint>>(list_rankingvippoint);
                    }

                    result.method_id = MethodType.s2cPlayerRankingResponse;
                    result.param1 = EbTool.protobufSerialize<PlayerRankingResponse>(ranking_response);
                }
                break;
            default:
                break;

        }

        return result;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayerRanking<TDef> : Component<TDef> where TDef : DefPlayerRanking, new()
    {
        //-------------------------------------------------------------------------
        public List<RankingChip> ListRankingChip { get; set; }
        public List<RankingVIPPoint> ListRankingVIPPoint { get; set; }
        ClientApp<DefApp> CoApp { get; set; }
        ClientPlayer<DefPlayer> CoPlayer { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientPlayerRanking.init()");

            defNodeRpcMethod<PlayerRankingResponse>(
                (ushort)MethodType.s2cPlayerRankingResponse, s2cPlayerRankingResponse);
            defNodeRpcMethod<PlayerRankingNotify>(
                (ushort)MethodType.s2cPlayerRankingNotify, s2cPlayerRankingNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientPlayerRanking.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            if (e is EvUiGetRankingChip)
            {
                requestGetRankingList(RankingListType.Chip);
            }
            else if (e is EvUiGetRankingVIPPoint)
            {
                requestGetRankingList(RankingListType.VIPPoint);
            }
        }

        //-------------------------------------------------------------------------
        // 排行榜响应
        void s2cPlayerRankingResponse(PlayerRankingResponse ranking_response)
        {
            switch (ranking_response.id)
            {
                case PlayerRankingResponseId.GetChipRankingList:// 获取筹码排行榜
                    {
                        var list_chip = EbTool.protobufDeserialize<List<RankingChip>>(ranking_response.data);
                        ListRankingChip = list_chip;

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityGetRankingChip>();
                        e.list_rankingchip = ListRankingChip;
                        e.send(null);
                    }
                    break;
                case PlayerRankingResponseId.GetVIPPointRankingList:// 获取积分排行榜
                    {
                        var list_vippoint = EbTool.protobufDeserialize<List<RankingVIPPoint>>(ranking_response.data);
                        ListRankingVIPPoint = list_vippoint;

                        var e = EntityMgr.getDefaultEventPublisher().genEvent<EvEntityGetRankingVIPPoint>();
                        e.list_rankingvippoint = ListRankingVIPPoint;
                        e.send(null);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 排行榜通知
        void s2cPlayerRankingNotify(PlayerRankingNotify ranking_notify)
        {
            switch (ranking_notify.id)
            {
                //case PlayerRankingNotifyId.SetupWarehouse:
                //    {
                //    }
                //    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 请求获取排行榜
        public void requestGetRankingList(RankingListType ranking_list_type)
        {
            PlayerRankingRequest ranking_request;
            ranking_request.id = PlayerRankingRequestId.GetRankingList;
            ranking_request.data = EbTool.protobufSerialize<RankingListType>(ranking_list_type);

            CoApp.rpc(MethodType.c2sPlayerRankingRequest, ranking_request);
        }

        //-------------------------------------------------------------------------
        public void createRankingUi()
        {
            //UiMbRanking mb_rank = UiMgr.Instance.createUi<UiMbRanking>();
            //mb_rank.setRankChip(ListRankingChip);
        }
    }
}

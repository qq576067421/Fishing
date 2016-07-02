using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellPlayerTrade<TDef> : Component<TDef> where TDef : DefPlayerTrade, new()
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
    public Task<MethodData> c2sPlayerTradeRequest(MethodData method_data)
    {
        MethodData result = new MethodData();
        result.method_id = MethodType.None;

        var playertrade_request = EbTool.protobufDeserialize<PlayerTradeRequest>(method_data.param1);
        switch (playertrade_request.id)
        {
            case PlayerTradeRequestId.MallGetItemList:// c->s, 商会，请求获取商品列表
                {
                    EbLog.Note("CellPlayerTrade.c2sPlayerTradeRequest() MallGetItemList");

                    //var request = EbTool.protobufDeserialize<PlayerWorkRoomRequestCompound>(playertrade_request.data);

                    //PlayerWorkRoomResponseCompound data;
                    //data.result = ProtocolResult.Success;
                    //data.item_data = null;

                    //PlayerWorkRoomResponse workroom_response;
                    //workroom_response.id = PlayerWorkRoomResponseId.Compound;
                    //workroom_response.data = EbTool.protobufSerialize<PlayerWorkRoomResponseCompound>(data);

                    //result.method_id = MethodType.s2cPlayerWorkRoomResponse;
                    //result.param1 = EbTool.protobufSerialize<PlayerWorkRoomResponse>(workroom_response);
                }
                break;
            case PlayerTradeRequestId.MallBuyItem:// c->s, 商会，请求购买商品
                {
                }
                break;
            case PlayerTradeRequestId.MallSellItem:// c->s, 商会，请求出售商品
                {
                }
                break;
            case PlayerTradeRequestId.MallGetItemPrice:// c->s, 商会，请求获取商品价格
                {
                }
                break;
            default:
                break;

        }

        return Task.FromResult(result);
    }
}

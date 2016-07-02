using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum PlayerTradeRequestId : byte
    {
        None = 0,// 无效

        MallGetItemList = 10,// c->s, 商会，请求获取商品列表
        MallBuyItem = 20,// c->s, 商会，请求购买商品
        MallSellItem = 30,// c->s, 商会，请求出售商品
        MallGetItemPrice = 40,// c->s, 商会，请求获取商品价格
    }

    //-------------------------------------------------------------------------
    public enum PlayerTradeResponseId : byte
    {
        None = 0,// 无效

        MallGetItemList = 10,// s->c, 商会，响应获取商品列表
        MallBuyItem = 20,// s->c, 商会，响应购买商品
        MallSellItem = 30,// s->c, 商会，响应出售商品
        MallGetItemPrice = 40,// s->c, 商会，响应获取商品价格
    }

    //-------------------------------------------------------------------------
    public enum PlayerTradeNotifyId : byte
    {
        None = 0,// 无效
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerTradeRequest
    {
        [ProtoMember(1)]
        public PlayerTradeRequestId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerTradeResponse
    {
        [ProtoMember(1)]
        public PlayerTradeResponseId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public struct PlayerTradeNotify
    {
        [ProtoMember(1)]
        public PlayerTradeNotifyId id;
        [ProtoMember(2)]
        public byte[] data;
    }

    //-------------------------------------------------------------------------
    // 商会商品
    [Serializable]
    [ProtoContract]
    public class MallItem
    {
        [ProtoMember(1)]
        public ItemData item_data;
        [ProtoMember(2)]
        public int price;
        [ProtoMember(3)]
        public float wave;
    }

    // 玩家交易
    public class DefPlayerTrade : ComponentDef
    {
        //-------------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
        }
    }
}

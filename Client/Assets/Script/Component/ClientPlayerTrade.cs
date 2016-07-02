using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayerTrade<TDef> : Component<TDef> where TDef : DefPlayerTrade, new()
    {
        //-------------------------------------------------------------------------
        ClientApp<DefApp> CoApp { get; set; }
        ClientPlayer<DefPlayer> CoPlayer { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientPlayerTrade.init()");

            defNodeRpcMethod<PlayerTradeResponse>(
                (ushort)MethodType.s2cPlayerTradeResponse, s2cPlayerTradeResponse);
            defNodeRpcMethod<PlayerTradeNotify>(
                (ushort)MethodType.s2cPlayerTradeNotify, s2cPlayerTradeNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientPlayerTrade.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            //if (e is EvEntityResponseGetShopChipList)
            //{
            //    //请求获取商店筹码列表
            //    //UiMbShopChip shop_chip = UiMgr.Instance.getCurrentUi<UiMbShopChip>();
            //    //if (shop_chip != null)
            //    //{
            //    //    //shop_chip.setShopChip();
            //    //}
            //}
            //else if (e is EvEntityResponseGetShopCoinList)
            //{
            //    //请求获取商店金币列表
            //    //UiMbShopCoin shop_coin = UiMgr.Instance.getCurrentUi<UiMbShopCoin>();
            //    //if (shop_coin != null)
            //    //{
            //    //    //shop_coin.setShopCoin();
            //    //}
            //}
            if (e is EvUiRequestBuyChip)
            {
                //请求购买筹码
                var ev = (EvUiRequestBuyChip)e;
                int id = ev.buy_chipid;

            }
            else if (e is EvUiRequestBuyCoin)
            {
                //请求购买金币
                var ev = (EvUiRequestBuyCoin)e;
                int id = ev.buy_coinid;

            }
            else if (e is EvUiClickShop)
            {
                //创建商店
                //UiMbShop shop = UiMgr.Instance.createUi<UiMbShop>();
                //shop.createShipChip();
            }
            else if (e is EvEntityBuyVIP)
            {
                //购买VIP
                var ev = (EvEntityBuyVIP)e;
                int buy_id = ev.buy_id;//配置的id

            }
        }

        //-------------------------------------------------------------------------
        // 交易响应
        void s2cPlayerTradeResponse(PlayerTradeResponse player_trade_response)
        {
            switch (player_trade_response.id)
            {
                case PlayerTradeResponseId.MallGetItemList:// s->c, 商会，响应获取商品列表
                    {
                    }
                    break;
                case PlayerTradeResponseId.MallBuyItem:// s->c, 商会，响应购买商品
                    {
                    }
                    break;
                case PlayerTradeResponseId.MallSellItem:// s->c, 商会，响应出售商品
                    {
                    }
                    break;
                case PlayerTradeResponseId.MallGetItemPrice:// s->c, 商会，响应获取商品价格
                    {
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        // 交易通知
        void s2cPlayerTradeNotify(PlayerTradeNotify player_trade_notify)
        {
            switch (player_trade_notify.id)
            {
                //case WarehouseResponseId.SetupWarehouse:// 响应初始化仓库
                //    {
                //    }
                //    break;
                default:
                    break;
            }
        }
    }
}

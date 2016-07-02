using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    // 客户端无需操作DefBag中的数据
    public class ClientBag<TDef> : Component<TDef> where TDef : DefBag, new()
    {
        //-------------------------------------------------------------------------
        Dictionary<string, Item> mMapItem = new Dictionary<string, Item>();

        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; set; }
        public Dictionary<string, Item> MapItem { get { return mMapItem; } }
        public ClientPlayer<DefPlayer> CoPlayer { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            defNodeRpcMethod<BagResponse>(
                (ushort)MethodType.s2cBagResponse, s2cBagResponse);
            defNodeRpcMethod<BagNotify>(
                (ushort)MethodType.s2cBagNotify, s2cBagNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoPlayer = Entity.getComponent<ClientPlayer<DefPlayer>>();

            // 请求初始化背包
            requestSetupBag();
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
            //if (e is EvUiToolTipClickBtnObj)
            //{
            //    var ev = (EvUiToolTipClickBtnObj)e;
            //    ItemOperateInfo operate_info = ev.operate_info;
            //    if (ev.item_parent == _eUiItemParent.Bag && !operate_info.IsCompoundType)
            //    {
            //        object o = ev.item.operate(operate_info.OperateId);
            //        if (o == null)
            //        {
            //            // 本地没有效果执行，则请求服务端使用道具            
            //            string obj_id = ev.item.ItemData.item_objid;
            //            requestOperateItem(operate_info.OperateId, obj_id);
            //        }
            //    }
            //}
            //else if (e is EvUiCleanUpBag)
            //{
            //    var ev = (EvUiCleanUpBag)e;

            //    // 整理背包
            //}
        }

        //-------------------------------------------------------------------------
        public int getItemNumByItemId(int item_id)
        {
            int count = 0;
            foreach (var i in mMapItem)
            {
                if (i.Value.TbDataItem.Id == item_id)
                {
                    if (i.Value.TbDataItem.MaxOverlapNum == 1)
                    {
                        count++;
                    }
                    else
                    {
                        count += i.Value.ItemData.n;
                    }
                }
            }
            return count;
        }

        //-------------------------------------------------------------------------
        void s2cBagResponse(BagResponse bag_response)
        {
            switch (bag_response.id)
            {
                case BagResponseId.SetupBag:
                    {
                        EbLog.Note("ClientBag.s2cBagResponse() SetupBag");

                        var list_itemdata = EbTool.protobufDeserialize<List<ItemData>>(bag_response.data);
                        foreach (var i in list_itemdata)
                        {
                            //Item item = new Item(Entity, CoPlayer.CoActor.Def.EffectMgr, i);
                            //mMapItem[item.ItemData.item_objid] = item;
                        }
                    }
                    break;
                case BagResponseId.OperateItem:
                    {
                        //EbLog.Note("ClientBag.s2cBagResponse() OperateItem");

                        var result = EbTool.protobufDeserialize<ItemOperateResponseData>(bag_response.data);

                        _s2cOnOperateItem(result);
                    }
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cBagNotify(BagNotify bag_notify)
        {
            switch (bag_notify.id)
            {
                case BagNotifyId.AddItem:
                    {
                        var item_data = EbTool.protobufDeserialize<ItemData>(bag_notify.data);

                        EbLog.Note("ClientBag.s2cBagNotify() AddItem  " + item_data.item_objid);

                        s2cOnAddItem(item_data);
                    }
                    break;
                case BagNotifyId.DeleteItem:
                    {
                        var item_objid = EbTool.protobufDeserialize<string>(bag_notify.data);

                        EbLog.Note("ClientBag.s2cBagNotify() DeleteItem  " + item_objid);

                        s2cOnDeleteItem(item_objid);
                    }
                    break;
                case BagNotifyId.UpdateItem:
                    {
                        var item_data = EbTool.protobufDeserialize<ItemData>(bag_notify.data);

                        EbLog.Note("ClientBag.s2cBagNotify() UpdateItem  " + item_data.item_objid);

                        s2cOnUpdateItem(item_data);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void _s2cOnOperateItem(ItemOperateResponseData result)
        {
            if (result.result != ProtocolResult.Success)
            {
                //string msg = "使用道具失败！";
                //FloatMsgInfo f_info;
                //f_info.msg = msg;
                //f_info.color = Color.green;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
                return;
            }

            Item item = null;
            mMapItem.TryGetValue(result.item_objid, out item);
            if (item == null) return;

            // 使用道具成功提示
            if (item.TbDataItem.MainOperateInfo.OperateId == result.operate_id)
            {
                //string msg = item.TbDataItem.MainOperateInfo.OperateName + "：" + item.TbDataItem.Name;
                //FloatMsgInfo f_info;
                //f_info.msg = msg;
                //f_info.color = Color.green;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
            }

            if (result.overlap_num <= 0) mMapItem.Remove(result.item_objid);
            item.ItemData.n = result.overlap_num;

            // 广播消息通知Ui更新
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityBagOperateItem>();
            //e.item = item;
            //e.send(null);
        }

        //-------------------------------------------------------------------------
        public void s2cOnAddItem(ItemData item_data)
        {
            //Item item = new Item(Entity, CoPlayer.CoActor.Def.EffectMgr, item_data);
            //mMapItem[item.ItemData.item_objid] = item;

            //// FloatMsg
            //string msg = string.Format("获得道具：{0}", item.TbDataItem.Name);
            //FloatMsgInfo f_info;
            //f_info.msg = msg;
            //f_info.color = Color.green;
            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

            //// 广播消息通知Ui更新
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityBagAddItem>();
            //e.item = item;
            //e.send(null);
        }

        //-------------------------------------------------------------------------
        public void s2cOnDeleteItem(string item_objid)
        {
            mMapItem.Remove(item_objid);

            // 广播消息通知Ui更新
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityBagDeleteItem>();
            //e.item_objid = item_objid;
            //e.send(null);
        }

        //-------------------------------------------------------------------------
        public Item s2cOnUpdateItem(ItemData item_data)
        {
            //Item item = new Item(Entity, CoPlayer.CoActor.Def.EffectMgr, item_data);
            //mMapItem[item.ItemData.item_objid] = item;

            //// FloatMsg
            //string msg = string.Format("更新道具：{0}", item.TbDataItem.Name);
            //FloatMsgInfo f_info;
            //f_info.msg = msg;
            //f_info.color = Color.green;
            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);

            //// 广播消息通知Ui更新
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityBagUpdateItem>();
            //e.item = item;
            //e.send(null);

            //return item;

            return null;
        }

        //-------------------------------------------------------------------------
        public Item updateItem(ItemData item_data)
        {
            return s2cOnUpdateItem(item_data);
        }

        //-------------------------------------------------------------------------
        public Item getThenRemoveItem(string item_objid)
        {
            Item item = null;
            mMapItem.TryGetValue(item_objid, out item);
            if (item == null) return item;

            s2cOnDeleteItem(item_objid);

            return item;
        }

        //-------------------------------------------------------------------------
        public void requestSetupBag()
        {
            BagRequest bag_request;
            bag_request.id = BagRequestId.SetupBag;
            bag_request.data = null;

            CoApp.rpc(MethodType.c2sBagRequest, bag_request);
        }

        //-------------------------------------------------------------------------
        public void requestOperateItem(string operate_id, string item_objid)
        {
            ItemOperate item_operate = new ItemOperate();
            item_operate.operate_id = operate_id;
            item_operate.item_objid = item_objid;

            BagRequest bag_request;
            bag_request.id = BagRequestId.OperateItem;
            bag_request.data = EbTool.protobufSerialize<ItemOperate>(item_operate);

            CoApp.rpc(MethodType.c2sBagRequest, bag_request);
        }

        //-------------------------------------------------------------------------
        public List<Item> getAlreadyHaveItem(int tb_item_id)
        {
            List<Item> list_item = new List<Item>();
            foreach (var i in MapItem)
            {
                if (i.Value.TbDataItem.Id == tb_item_id)
                {
                    list_item.Add(i.Value);
                }
            }

            return list_item;
        }

        //-------------------------------------------------------------------------
        public List<Item> getCanSellToMall()
        {
            List<Item> list_item = new List<Item>();
            foreach (var i in MapItem)
            {
                TbDataTrade tb_trade = EbDataMgr.Instance.getData<TbDataTrade>(i.Value.TbDataItem.Id);

                if (tb_trade == null)
                {
                    continue;
                }

                if (tb_trade.MallItemInfo.MallShelves)
                {
                    list_item.Add(i.Value);
                }
            }

            return list_item;
        }

        //-------------------------------------------------------------------------
        public List<Item> getCanSellToMarcket()
        {
            List<Item> list_item = new List<Item>();
            foreach (var i in MapItem)
            {
                TbDataTrade tb_trade = EbDataMgr.Instance.getData<TbDataTrade>(i.Value.TbDataItem.Id);

                if (tb_trade == null)
                {
                    continue;
                }

                if (tb_trade.MarketItemInfo.Shelves)
                {
                    list_item.Add(i.Value);
                }
            }

            return list_item;
        }
    }
}

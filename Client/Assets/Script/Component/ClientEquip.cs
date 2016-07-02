using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientEquip<TDef> : Component<TDef> where TDef : DefEquip, new()
    {
        //-------------------------------------------------------------------------
        Dictionary<EquipSlot, Item> mMapItem = new Dictionary<EquipSlot, Item>();

        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }
        public ClientActor<DefActor> CoActor { get; private set; }
        public Dictionary<EquipSlot, Item> MapItem { get { return mMapItem; } }

        //-------------------------------------------------------------------------
        public override void init()
        {
            defNodeRpcMethod<EquipResponse>(
                (ushort)MethodType.s2cEquipResponse, s2cEquipResponse);
            defNodeRpcMethod<EquipNotify>(
                (ushort)MethodType.s2cEquipNotify, s2cEquipNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoActor = Entity.getComponent<ClientActor<DefActor>>();

            // 请求初始化装备
            requestSetEquip();
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
            //if (Entity.Guid == EntityMgr.findFirstEntityByType<EtPlayer>().Guid)
            //{
            //    if (e is EvUiToolTipClickTabkeOffEquip)
            //    {
            //        var ev = (EvUiToolTipClickTabkeOffEquip)e;
            //        EquipSlot equip_slot = ev.equip_slot;
            //        Debug.Log("ClientEquip.handleEvent() EvUiToolTipEquipClickBtn   " + equip_slot);
            //        requestTakeoffEquip(equip_slot);
            //        UiMgr.Instance.destroyCurrentUi<UiMbToolTipObj>();// todo，删除此处调用
            //    }
            //}
        }

        //-------------------------------------------------------------------------
        void s2cEquipResponse(EquipResponse equip_response)
        {
            switch (equip_response.id)
            {
                case EquipResponseId.SetupEquip:
                    {
                        EbLog.Note("ClientEquip.s2cEquipResponse() SetupEquip");

                        //var list_itemdata = EbTool.protobufDeserialize<List<ItemData>>(equip_response.data);
                        //foreach (var i in list_itemdata)
                        //{
                        //    Item item = new Item(Entity, CoActor.Def.EffectMgr, i);
                        //    EquipSlot equip_slot = Def.getEquipSlot(item);
                        //    mMapItem[equip_slot] = item;
                        //}
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cEquipNotify(EquipNotify equip_notify)
        {
            switch (equip_notify.id)
            {
                case EquipNotifyId.TakeonEquip:
                    {
                        EbLog.Note("ClientEquip.s2cEquipNotify() TakeonEquip");

                        var item_data = EbTool.protobufDeserialize<ItemData>(equip_notify.data);

                        s2cOnTakeonEquip(item_data);
                    }
                    break;
                case EquipNotifyId.TakeoffEquip:
                    {
                        EbLog.Note("ClientEquip.s2cEquipNotify() TakeoffEquip");

                        var equip_slot = EbTool.protobufDeserialize<EquipSlot>(equip_notify.data);

                        s2cOnTakeoffEquip(equip_slot);
                    }
                    break;
            }
        }

        //-------------------------------------------------------------------------
        public void s2cOnTakeoffEquip(EquipSlot equip_slot)
        {
            Item item = null;
            mMapItem.TryGetValue(equip_slot, out item);
            if (item != null)
            {
                mMapItem.Remove(equip_slot);
            }

            // 通知Ui更新装备显示
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityEquipTakeoffEquip>();
            //e.item = item;
            //e.send(null);
        }

        //-------------------------------------------------------------------------
        public void s2cOnTakeonEquip(ItemData item_data)
        {
            //Item item = new Item(Entity, CoActor.Def.EffectMgr, item_data);
            //EquipSlot equip_slot = Def.getEquipSlot(item);
            //mMapItem[equip_slot] = item;

            // 通知Ui更新装备显示
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityEquipTakeonEquip>();
            //e.item = item;
            //e.send(null);
        }

        //-------------------------------------------------------------------------
        public void requestSetEquip()
        {
            EquipRequest equip_request;
            equip_request.id = EquipRequestId.SetupEquip;
            equip_request.data = null;

            CoApp.rpc(MethodType.c2sEquipRequest, equip_request);
        }

        //-------------------------------------------------------------------------
        public void requestTakeoffEquip(EquipSlot equip_slot)
        {
            EquipRequest equip_request;
            equip_request.id = EquipRequestId.TakeoffEquip;
            equip_request.data = EbTool.protobufSerialize<EquipSlot>(equip_slot);

            CoApp.rpc(MethodType.c2sEquipRequest, equip_request);
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
        public List<Item> getCanInsetGemEquip()
        {
            List<Item> list_item = new List<Item>();
            foreach (var i in MapItem)
            {
                //UnitEquip equip = (UnitEquip)i.Value.UnitLink;
                //if (equip.ListEquipSlot.Count > 0)
                //{
                //    list_item.Add(i.Value);
                //}
            }

            return list_item;
        }
    }
}

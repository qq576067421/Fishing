using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientStatus<TDef> : Component<TDef> where TDef : DefStatus, new()
    {
        //-------------------------------------------------------------------------
        Dictionary<int, Item> mMapItem = new Dictionary<int, Item>();// item_id
        Queue<int> mItemSignDestroy = new Queue<int>();

        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }
        public ClientActor<DefActor> CoActor { get; private set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            defNodeRpcMethod<StatusResponse>(
                (ushort)MethodType.s2cStatusResponse, s2cStatusResponse);
            defNodeRpcMethod<StatusNotify>(
                (ushort)MethodType.s2cStatusNotify, s2cStatusNotify);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
            CoActor = Entity.getComponent<ClientActor<DefActor>>();

            // 请求初始化状态
            requestSetupStatus();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            // 更新所有状态
            foreach (var i in mMapItem)
            {
                //UnitStatus unit_status = (UnitStatus)i.Value.UnitLink;
                //unit_status.update(elapsed_tm);

                //if (unit_status.Finished)
                //{
                //    unit_status.destroy();
                //    mItemSignDestroy.Enqueue(i.Key);
                //}
            }

            // 删除已经标记为销毁的Status
            if (mItemSignDestroy.Count > 0)
            {
                foreach (var i in mItemSignDestroy)
                {
                    mMapItem.Remove(i);

                    // 广播创建Status消息
                    //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityStatusDestroyStatus>();
                    //e.et_guid = Entity.Guid;
                    //e.item_id = i;
                    //e.send(null);
                }
            }
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
        }

        //-------------------------------------------------------------------------
        void s2cStatusResponse(StatusResponse status_response)
        {
            switch (status_response.id)
            {
                case StatusResponseId.SetupStatus:
                    {
                        EbLog.Note("ClientStatus.s2cStatusResponse() SetupStatus");

                        var list_itemdata = EbTool.protobufDeserialize<List<ItemData>>(status_response.data);

                        _s2cOnSetupStatus(list_itemdata);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void s2cStatusNotify(StatusNotify status_notify)
        {
            switch (status_notify.id)
            {
                case StatusNotifyId.CreateStatus:
                    {
                        EbLog.Note("ClientStatus.s2cStatusNotify() CreateStatus");

                        var item_data = EbTool.protobufDeserialize<ItemData>(status_notify.data);

                        _s2cCreateStatus(item_data);
                    }
                    break;
                default:
                    break;
            }
        }

        //-------------------------------------------------------------------------
        void _s2cOnSetupStatus(List<ItemData> list_itemdata)
        {
            // 状态背包中所有状态
            foreach (var i in list_itemdata)
            {
                //Item item = new Item(Entity, CoActor.Def.EffectMgr, i);
                //mMapItem[item.TbDataItem.Id] = item;
            }

            //foreach (var i in mMapItem)
            //{
            //    Debug.LogError("item_id=" + i.Value.getData4Db().id + " name:: " + i.Value.TbDataItem.Name);
            //}

            // 获取buffer
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityStatusInit>();
            //e.map_item = mMapItem;
            //e.et_guid = Entity.Guid;
            //e.send(null);
        }

        //-------------------------------------------------------------------------
        void _s2cCreateStatus(ItemData item_data)
        {
            //Item item = new Item(Entity, CoActor.Def.EffectMgr, item_data);
            //mMapItem[item.TbDataItem.Id] = item;

            //// 广播创建Status消息
            //var e = UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntityStatusCreateStatus>();
            //e.et_guid = Entity.Guid;
            //e.item = item;
            //e.send(null);
        }

        //-------------------------------------------------------------------------
        public void requestSetupStatus()
        {
            StatusRequest status_request;
            status_request.id = StatusRequestId.SetupStatus;
            status_request.data = null;

            CoApp.rpc(MethodType.c2sStatusRequest, status_request);
        }
    }
}

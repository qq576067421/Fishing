using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientApp<TDef> : Component<TDef> where TDef : DefApp, new()
    {
        //-------------------------------------------------------------------------
        Dictionary<string, BtFactory> mMapBtFactory = new Dictionary<string, BtFactory>();

        //-------------------------------------------------------------------------
        public ClientSound<DefSound> CoSound { get; private set; }
        public ClientDataEye<DefDataEye> CoDataEye { get; private set; }
        public ClientNetMonitor<DefNetMonitor> CoNetMonitor { get; private set; }
        public ClientSysNotice<DefSysNotice> CoSysNotice { get; private set; }
        public ClientUCenterSDK<DefUCenterSDK> CoUCenter { get; private set; }
        public string BundleVersion { get; set; }
        public string DataVersion { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientApp.init()");

            EntityMgr.getDefaultEventPublisher().addHandler(Entity);

            // 创建EtFishing
            var et_fishing = EntityMgr.createEntity<EtFishing>(null, Entity);
            EntityMgr.getDefaultEventPublisher().addHandler(et_fishing);

            // ClientSound组件
            CoSound = et_fishing.getComponent<ClientSound<DefSound>>();

            // ClientDataEye组件
            CoDataEye = et_fishing.getComponent<ClientDataEye<DefDataEye>>();

            // ClientNetMonitor组件
            CoNetMonitor = et_fishing.getComponent<ClientNetMonitor<DefNetMonitor>>();

            // ClientSysNotice组件
            CoSysNotice = et_fishing.getComponent<ClientSysNotice<DefSysNotice>>();

            //UiMgr.Instance.CoApp = Entity.getComponent<ClientApp<DefApp>>();

            // 创建EtUCenter
            var et_ucenter = EntityMgr.createEntity<EtUCenterSDK>(null, Entity);
            CoUCenter = et_ucenter.getComponent<ClientUCenterSDK<DefUCenterSDK>>();
            CoUCenter.UCenterDomain = MbMain.Instance.ClientConfig.UCenterDomain;

            // 创建EtLaunch
            EntityMgr.createEntity<EtLaunch>(null, Entity);
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientApp.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            // 检测是否请求退出
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 退出游戏
                //UiMgr.Instance.createUi<UiMbQuitOrBack>(_eUiLayer.QuitGame);
            }
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
            //UiMgr.Instance.handleEvent(sender, e);
        }

        //-------------------------------------------------------------------------
        public void captureScreenshot()
        {
            Application.CaptureScreenshot("Screenshot.png");
        }

        //-------------------------------------------------------------------------
        public Bt createBt(string bt_name, Entity self)
        {
            BtFactory bt_factory = null;
            mMapBtFactory.TryGetValue(bt_name, out bt_factory);

            if (bt_factory == null) return null;
            else return bt_factory.createBt(self);
        }

        //-------------------------------------------------------------------------
        void _regBtFactory(BtFactory bt_factory)
        {
            mMapBtFactory[bt_factory.getName()] = bt_factory;
        }

        //---------------------------------------------------------------------
        public void rpc(MethodType method_id)
        {
            rpcBySession(CoNetMonitor.Session, (ushort)method_id);
        }

        //---------------------------------------------------------------------
        public void rpc<T1>(MethodType method_id, T1 obj1)
        {
            rpcBySession(CoNetMonitor.Session, (ushort)method_id, obj1);
        }

        //---------------------------------------------------------------------
        public void rpc<T1, T2>(MethodType method_id, T1 obj1, T2 obj2)
        {
            rpcBySession(CoNetMonitor.Session, (ushort)method_id, obj1, obj2);
        }

        //---------------------------------------------------------------------
        public void rpc<T1, T2, T3>(MethodType method_id, T1 obj1, T2 obj2, T3 obj3)
        {
            rpcBySession(CoNetMonitor.Session, (ushort)method_id, obj1, obj2, obj3);
        }

        //---------------------------------------------------------------------
        public void rpc<T1, T2, T3, T4>(MethodType method_id, T1 obj1, T2 obj2, T3 obj3, T4 obj4)
        {
            rpcBySession(CoNetMonitor.Session, (ushort)method_id, obj1, obj2, obj3, obj4);
        }
    }
}

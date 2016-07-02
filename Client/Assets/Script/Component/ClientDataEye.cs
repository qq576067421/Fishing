using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientDataEye<TDef> : Component<TDef> where TDef : DefDataEye, new()
    {
        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientDataEye.init()");

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientDataEye.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public void setupDataEye()
        {
            // 初始化DataEye
            //string app_id = "CA79CCBBD789FBF970627DE92D851BFB";
            //string channel_id = "";// ClientMain.Instance.ClientConfig.ChannelName;
            //DCAgent.setReportMode(DCReportMode.DC_AFTER_LOGIN);
            //DCAgent.getInstance().initWithAppIdAndChannelId(app_id, channel_id);
        }

        //-------------------------------------------------------------------------
        public void login(string acc_name, ulong acc_id)
        {
            //DCAccount.login(acc_name + "_" + acc_id, "Android");
        }

        //-------------------------------------------------------------------------
        public void logout()
        {
            //DCAccount.logout();
        }

        //-------------------------------------------------------------------------
        public void onApplicationPause(bool pause_status)
        {
        }
    }
}

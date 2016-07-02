using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientLaunch<TDef> : Component<TDef> where TDef : DefLaunch, new()
    {
        //-------------------------------------------------------------------------
        public ClientApp<DefApp> CoApp { get; private set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientLaunch.init()");

            EntityMgr.getDefaultEventPublisher().addHandler(Entity);

            Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
            CoApp = et_app.getComponent<ClientApp<DefApp>>();

            AutoPatcherConfig autopatcher_cfg = new AutoPatcherConfig();
            autopatcher_cfg.RemoteVersionInfoUrl = MbMain.Instance.ClientConfig.RemoteVersionInfoUrl;

            var et_autopatcher = EntityMgr.createEntity<EtAutoPatcher>(null, Entity);
            var co_autopatcher = et_autopatcher.getComponent<ClientAutoPatcher<DefAutoPatcher>>();

            UiLoading ui_loading = UiMgr.Instance.createUi<UiLoading>("Loading", "Loading");
            ui_loading.OnFinished = () =>
            {
                // 创建EtLogin
                EntityMgr.createEntity<EtLogin>(null, CoApp.Entity);
                EntityMgr.destroyEntity(et_autopatcher);
            };

            co_autopatcher.OnAutoPatcherGetServerVersionCfg =
                () =>
                {
                    ui_loading.setTip("请求获取版本信息");
                };

            co_autopatcher.OnAutoPatcherGetServerVersionCfgResult =
                (r, error) =>
                {
                    if (r == AutoPatcherResult.Success)
                    {
                        ui_loading.setTip("获取版本信息成功");
                    }
                    else
                    {
                        ui_loading.setTip("获取版本信息失败！ Error=" + error);
                    }
                };

            co_autopatcher.OnAutoPatcherIsNeedBundlePatcher =
                (is_need, local_bundle_version, remote_bundle_version) =>
                {
                    if (!is_need)
                    {
                        ui_loading.setTip("Bundle版本相同，无需更新");
                    }
                    else
                    {
                        ui_loading.setTip("更新Bundle");

                        string info = string.Format("Bundle版本不同，从{0}更新到{1}", local_bundle_version, remote_bundle_version);
                        //UiMbUpdate update = UiMgr.Instance.createUi<UiMbUpdate>(_eUiLayer.Waiting);
                        //Action delOk = () => { co_autopatcher.confirmPatcherBundle(); };
                        //Action delCancel = () => { Application.Quit(); };
                        //update.setUpdateInfo(info, delOk, delCancel);
                    }
                };

            co_autopatcher.OnAutoPatcherIsNeedDataPatcher =
                (is_need, local_data_version, remote_data_version) =>
                {
                    if (!is_need)
                    {
                        ui_loading.setTip("Data版本相同，无需更新");
                    }
                    else
                    {
                        //ui_loading.setTips("更新数据");

                        string info = string.Format("Data版本不同，从{0}更新到{1}", local_data_version, remote_data_version);
                        //UiMbUpdate update = UiMgr.Instance.createUi<UiMbUpdate>(_eUiLayer.Waiting);
                        //Action delOk = () => { co_autopatcher.confirmPatcherData(); };
                        //Action delCancel = () => { Application.Quit(); };
                        //update.setUpdateInfo(info, delOk, delCancel);
                    }
                };

            co_autopatcher.OnAutoPatcherGetRemoteDataFileList =
                () =>
                {
                    ui_loading.setTip("请求获取数据文件列表");
                };

            co_autopatcher.OnAutoPatcherGetRemoteDataFileListResult =
                (r, error) =>
                {
                    if (r == AutoPatcherResult.Success)
                    {
                        ui_loading.setTip("获取数据文件列表成功");
                    }
                    else
                    {
                        ui_loading.setTip("获取数据文件列表失败！ Error=" + error);
                    }
                };

            co_autopatcher.OnAutoPatcherDataPatcher =
                (info) =>
                {
                    ui_loading.setTip(info);
                };

            co_autopatcher.OnAutoPatcherFinished =
                () =>
                {
                    ui_loading.setTip("自动更新结束");

                    CoApp.BundleVersion = co_autopatcher.VersionInfo.LocalBundleVersion;
                    CoApp.DataVersion = co_autopatcher.VersionInfo.LocalVersionInfo.data_version;

                    // 创建EtLogin
                    EntityMgr.createEntity<EtLogin>(null, CoApp.Entity);
                    EntityMgr.destroyEntity(et_autopatcher);
                };

            
            //co_autopatcher.launchAutoPatcher(autopatcher_cfg);
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientLaunch.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GF.Common;

public delegate void OnAutoPatcherGetServerVersionCfg();// 从自动更新服务器获取版本配置信息
public delegate void OnAutoPatcherGetServerVersionCfgResult(AutoPatcherResult r, string error);// 从自动更新服务器获取版本配置信息的结果
public delegate void OnAutoPatcherIsNeedBundlePatcher(bool is_need, string local_bundle_version, string remote_bundle_version);// 判定是否需要Bundle包更新
public delegate void OnAutoPatcherIsNeedDataPatcher(bool is_need, string local_data_version, string remote_data_version);// 判定是否需要Data包更新
public delegate void OnAutoPatcherGetRemoteDataFileList();// 从自动更新服务器获取数据文件列表
public delegate void OnAutoPatcherGetRemoteDataFileListResult(AutoPatcherResult r, string error);// 从自动更新服务器获取数据文件列表的结果
public delegate void OnAutoPatcherDataPatcher(string info);// 更新数据文件
public delegate void OnAutoPatcherFinished();// 从自动更新服务器结束

public class ClientAutoPatcher<TDef> : Component<TDef> where TDef : DefAutoPatcher, new()
{
    //-------------------------------------------------------------------------
    Dictionary<string, string> mMapNeedDeleteAsset;
    Dictionary<string, string> mMapSameAsset;
    Queue<string> mQueueAlreadyLoadAsset;
    AutoPatcherDataFileList mAutoPatcherDataFileList;
    WWW mWWWGetServerVersionCfg;
    WWW mUpdateDataWWW;
    string mRemoteRootDir;
    byte[] mRemoteDataFileList;

    //-------------------------------------------------------------------------
    public OnAutoPatcherGetServerVersionCfg OnAutoPatcherGetServerVersionCfg { get; set; }
    public OnAutoPatcherGetServerVersionCfgResult OnAutoPatcherGetServerVersionCfgResult { get; set; }
    public OnAutoPatcherIsNeedBundlePatcher OnAutoPatcherIsNeedBundlePatcher { get; set; }
    public OnAutoPatcherIsNeedDataPatcher OnAutoPatcherIsNeedDataPatcher { get; set; }
    public OnAutoPatcherGetRemoteDataFileList OnAutoPatcherGetRemoteDataFileList { get; set; }
    public OnAutoPatcherGetRemoteDataFileListResult OnAutoPatcherGetRemoteDataFileListResult { get; set; }
    public OnAutoPatcherDataPatcher OnAutoPatcherDataPatcher { get; set; }
    public OnAutoPatcherFinished OnAutoPatcherFinished { get; set; }
    public VersionInfo VersionInfo { get; private set; }
    public AutoPatcherConfig AutoPatcherConfig { get; set; }// 自动更新各项配置
    bool StepGetRemoteVersionInfo { get; set; }// 从自动更新服务器获取版本配置信息
    bool StepIsNeedBundlePatcher { get; set; }// 判定是否需要Bundle包更新
    bool StepBundlePatcher { get; set; }// Bundle包更新
    bool StepIsNeedDataPatcher { get; set; }// 判定是否需要数据包更新
    bool StepGetRemoteDataFileList { get; set; }// 从自动更新服务器获取数据清单信息
    bool StepDataPatcher { get; set; }// 数据包更新
    bool StepFinished { get; set; }// 完成自动更新

    //-------------------------------------------------------------------------
    public override void init()
    {
        EbLog.Note("ClientAutoPatcher.init()");

        StepGetRemoteVersionInfo = false;
        StepIsNeedBundlePatcher = false;
        StepBundlePatcher = false;
        StepIsNeedDataPatcher = false;
        StepDataPatcher = false;
        StepFinished = false;

        AutoPatcherConfig = new AutoPatcherConfig();
        VersionInfo = new VersionInfo(AutoPatcherPath.combinePersistentDataPath("/DataFileList.txt"));
        mMapNeedDeleteAsset = new Dictionary<string, string>();
        mMapSameAsset = new Dictionary<string, string>();
        mQueueAlreadyLoadAsset = new Queue<string>();
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        EbLog.Note("ClientAutoPatcher.release()");
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        // 步骤：从自动更新服务器获取版本配置信息
        if (StepGetRemoteVersionInfo)
        {
            if (mWWWGetServerVersionCfg == null)
            {
                mWWWGetServerVersionCfg = new WWW(AutoPatcherConfig.RemoteVersionInfoUrl);

                if (OnAutoPatcherGetServerVersionCfg != null)
                {
                    OnAutoPatcherGetServerVersionCfg();
                }
            }
            else if (mWWWGetServerVersionCfg.isDone)
            {
                AutoPatcherResult r = AutoPatcherResult.Success;
                if (!string.IsNullOrEmpty(mWWWGetServerVersionCfg.error))
                {
                    r = AutoPatcherResult.Failed;
                }
                else
                {
                    VersionInfo.parseRemoteVersionInfo(mWWWGetServerVersionCfg.text);
                }

                if (OnAutoPatcherGetServerVersionCfgResult != null)
                {
                    OnAutoPatcherGetServerVersionCfgResult(r, mWWWGetServerVersionCfg.error);
                }

                StepGetRemoteVersionInfo = false;
                StepIsNeedBundlePatcher = true;
                mWWWGetServerVersionCfg = null;
            }
        }

        // 步骤：判定是否需要Bundle包更新
        if (StepIsNeedBundlePatcher)
        {
            bool is_need = false;
            if (!VersionInfo.remoteEqualsLocalBundleVersion())
            {
                is_need = true;
            }

            if (is_need)
            {
                if (OnAutoPatcherIsNeedBundlePatcher != null)
                {
                    OnAutoPatcherIsNeedBundlePatcher(is_need,
                        VersionInfo.LocalBundleVersion,
                        VersionInfo.RemoteVersionInfo.bundle_version);
                }
            }
            else
            {
                StepIsNeedDataPatcher = true;
            }

            StepIsNeedBundlePatcher = false;
        }

        // 步骤：Bundle包更新
        if (StepBundlePatcher)
        {
            Application.OpenURL(VersionInfo.RemoteVersionInfo.bundle_url);
            StepBundlePatcher = false;
        }

        // 步骤：判定是否需要数据包更新
        if (StepIsNeedDataPatcher)
        {
            bool is_need = false;
            if (!VersionInfo.remoteEqualsLocalDataVersion())
            {
                is_need = true;
            }

            if (is_need)
            {
                if (OnAutoPatcherIsNeedDataPatcher != null)
                {
                    OnAutoPatcherIsNeedDataPatcher(is_need,
                        VersionInfo.LocalVersionInfo.data_version,
                        VersionInfo.RemoteVersionInfo.data_version);
                }
            }
            else
            {
                StepFinished = true;
            }

            StepIsNeedDataPatcher = false;
        }

        // 步骤：从自动更新服务器获取数据清单信息
        if (StepGetRemoteDataFileList)
        {
            if (mUpdateDataWWW == null)
            {
                Caching.CleanCache();

                mUpdateDataWWW = new WWW(VersionInfo.RemoteVersionInfo.data_url);
                mRemoteRootDir = VersionInfo.RemoteVersionInfo.data_url.Substring(
                    0, VersionInfo.RemoteVersionInfo.data_url.LastIndexOf('/'));

                if (OnAutoPatcherGetRemoteDataFileList != null)
                {
                    OnAutoPatcherGetRemoteDataFileList();
                }
            }
            else if (mUpdateDataWWW.isDone)
            {
                AutoPatcherResult r = AutoPatcherResult.Success;
                if (string.IsNullOrEmpty(mUpdateDataWWW.error))
                {
                    mRemoteDataFileList = mUpdateDataWWW.bytes;
                    VersionInfo.parseRemoteDataFileList(mUpdateDataWWW.text);
                    var map_need_loadasset = _getNeedLoadAsset();
                    _deleteOldAsset();

                    if (map_need_loadasset.Count == 0)
                    {
                        VersionInfo.changeLocalDataVersionToRemote();

                        using (FileStream fs = new FileStream(AutoPatcherPath.combinePersistentDataPath("/DataFileList.txt"), FileMode.Create))
                        {
                            fs.Write(mRemoteDataFileList, 0, mRemoteDataFileList.Length);
                        }

                        StepFinished = true;
                    }
                    else
                    {
                        mAutoPatcherDataFileList = new AutoPatcherDataFileList();
                        mAutoPatcherDataFileList.addFile4Download(mRemoteRootDir, map_need_loadasset);

                        StepDataPatcher = true;
                    }
                }
                else
                {
                    r = AutoPatcherResult.Failed;
                }

                if (OnAutoPatcherGetRemoteDataFileListResult != null)
                {
                    OnAutoPatcherGetRemoteDataFileListResult(r, mUpdateDataWWW.error);
                }

                mUpdateDataWWW = null;
                StepGetRemoteDataFileList = false;
            }
        }

        // 步骤：数据包更新
        if (StepDataPatcher)
        {
            mAutoPatcherDataFileList.update(elapsed_tm);

            if (mAutoPatcherDataFileList.Finished)
            {
                VersionInfo.changeLocalDataVersionToRemote();

                using (FileStream fs = new FileStream(AutoPatcherPath.combinePersistentDataPath("/DataFileList.txt"), FileMode.Create))
                {
                    fs.Write(mRemoteDataFileList, 0, mRemoteDataFileList.Length);
                }

                mRemoteDataFileList = null;
                mAutoPatcherDataFileList = null;

                StepDataPatcher = false;
                StepFinished = true;
            }
            else
            {
                float progress = mAutoPatcherDataFileList.CurrentLoadProgress;
                progress *= 100f;

                string info = string.Format("({0:00.0}%)正在下载数据文件{1}({2}/{3})",
                    progress,
                    mAutoPatcherDataFileList.CurrentDataFile,
                    mAutoPatcherDataFileList.CurrentIndex,
                    mAutoPatcherDataFileList.TotalCount);

                if (OnAutoPatcherDataPatcher != null)
                {
                    OnAutoPatcherDataPatcher(info);
                }
            }
        }

        // 自动更新结束
        if (StepFinished)
        {
            StepFinished = false;

            if (OnAutoPatcherFinished != null)
            {
                OnAutoPatcherFinished();
            }
        }
    }

    //-------------------------------------------------------------------------
    public void launchAutoPatcher(AutoPatcherConfig cfg)
    {
        // 设置自动更新参数配置，并检查参数有效性
        AutoPatcherConfig.RemoteVersionInfoUrl = cfg.RemoteVersionInfoUrl;
        if (string.IsNullOrEmpty(AutoPatcherConfig.RemoteVersionInfoUrl))
        {
            EbLog.Error("ClientAutoPatcher.launchAutoPatcher() AutoPatcherConfig.ServerUrlPrefix is Null!");
        }

        // 启动自动更新
        StepGetRemoteVersionInfo = true;
    }

    //-------------------------------------------------------------------------
    public void confirmPatcherBundle()
    {
        StepBundlePatcher = true;
    }

    //-------------------------------------------------------------------------
    public void confirmPatcherData()
    {
        StepGetRemoteDataFileList = true;
    }

    //-------------------------------------------------------------------------
    Dictionary<string, string> _getNeedLoadAsset()
    {
        Dictionary<string, string> map_needloadasset = new Dictionary<string, string>();

        Dictionary<string, string> map_remote_asset_info = VersionInfo.MapRemoteAssetInfo;
        Dictionary<string, string> map_local_asset_info = VersionInfo.MapLocalAssetInfo;
        foreach (var i in map_remote_asset_info)
        {
            string value = "";
            map_local_asset_info.TryGetValue(i.Key, out value);

            if (string.IsNullOrEmpty(value))
            {
                map_needloadasset.Add(i.Key, i.Value);
            }
            else
            {
                if (!i.Value.Equals(value))
                {
                    map_needloadasset.Add(i.Key, i.Value);
                }
                else
                {
                    mMapSameAsset.Add(i.Key, i.Value);
                }
            }
        }

        foreach (var i in map_local_asset_info)
        {
            if (!mMapSameAsset.ContainsKey(i.Key))
            {
                mMapNeedDeleteAsset.Add(i.Key, i.Value);
            }
        }

        return map_needloadasset;
    }

    //-------------------------------------------------------------------------
    void _deleteOldAsset()
    {
        foreach (var i in mMapNeedDeleteAsset)
        {
            string path = AutoPatcherPath.combinePersistentDataPath(i.Key);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}

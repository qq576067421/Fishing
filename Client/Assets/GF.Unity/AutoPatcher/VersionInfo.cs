using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;

//-----------------------------------------------------------------------------
[Serializable]
public struct LocalVersionInfo
{
    public string data_version;
    public string remote_version_url;
}

//-----------------------------------------------------------------------------
public struct RemoteVersionInfo
{
    public string bundle_version;
    public string bundle_url;
    public string data_version;
    public string data_url;
    public AutoPatcherServerState server_state;
}

public class VersionInfo
{
    //-------------------------------------------------------------------------
    public string LocalBundleVersion { get; private set; }
    public LocalVersionInfo LocalVersionInfo { get; private set; }
    public RemoteVersionInfo RemoteVersionInfo { get; private set; }
    public Dictionary<string, string> MapLocalAssetInfo { get; private set; }
    public Dictionary<string, string> MapRemoteAssetInfo { get; private set; }
    public string LocalAssetInfoPath { get; private set; }

    //-------------------------------------------------------------------------
    public VersionInfo(string local_assetinfo_path)
    {
        LocalBundleVersion = Application.version;
        LocalAssetInfoPath = local_assetinfo_path;
        MapLocalAssetInfo = new Dictionary<string, string>();
        MapRemoteAssetInfo = new Dictionary<string, string>();

        // Load Local VersionInfo
        string data = PlayerPrefs.GetString(AutoPatcherStringDef.PlayerPrefsKeyLocalVersionInfo);
        if (string.IsNullOrEmpty(data))
        {
            LocalVersionInfo info;
            info.data_version = "";
            info.remote_version_url = "";
            LocalVersionInfo = info;
        }
        else
        {
            LocalVersionInfo = EbTool.jsonDeserialize<LocalVersionInfo>(data);
        }

        // Parse Local DataFileList
        if (File.Exists(LocalAssetInfoPath))
        {
            // 读取本地DataFileList.txt文件成功
            _parseDataFileList(File.ReadAllText(LocalAssetInfoPath), true);
        }
        else
        {
            // 读取本地DataFileList.txt文件失败
            LocalVersionInfo info = LocalVersionInfo;
            info.data_version = "";
            LocalVersionInfo = info;
            data = EbTool.jsonSerialize(LocalVersionInfo);
            PlayerPrefs.SetString(AutoPatcherStringDef.PlayerPrefsKeyLocalVersionInfo, data);
        }
    }

    //-------------------------------------------------------------------------
    public void changeLocalDataVersionToRemote()
    {
        LocalVersionInfo info;
        info.data_version = RemoteVersionInfo.data_version;
        info.remote_version_url = "";
        LocalVersionInfo = info;

        string data = EbTool.jsonSerialize(LocalVersionInfo);

        PlayerPrefs.SetString(AutoPatcherStringDef.PlayerPrefsKeyLocalVersionInfo, data);
    }

    //-------------------------------------------------------------------------
    public void parseRemoteVersionInfo(string remote_version_info_text)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(remote_version_info_text);

        XmlNode version_node =
#if UNITY_ANDROID
        xml.SelectSingleNode("VersionInfo/Android");
#elif UNITY_IPHONE
        xml.SelectSingleNode("VersionInfo/iOS");
#else
        xml.SelectSingleNode("VersionInfo/Pc");
#endif
        XmlNode server_state_node = xml.SelectSingleNode("VersionInfo/ServerState");

        RemoteVersionInfo server_config;
        server_config.bundle_version = version_node.Attributes["BundleVersion"].Value;
        server_config.bundle_url = version_node.Attributes["BundleURL"].Value;
        server_config.data_version = version_node.Attributes["DataVersion"].Value;
        server_config.data_url = version_node.Attributes["DataURL"].Value;
        server_config.server_state = (AutoPatcherServerState)(int.Parse(server_state_node.Attributes["state"].Value));
        RemoteVersionInfo = server_config;
    }

    //-------------------------------------------------------------------------
    public void parseRemoteDataFileList(string pack_info)
    {
        _parseDataFileList(pack_info, false);
    }

    //-------------------------------------------------------------------------
    public bool remoteEqualsLocalBundleVersion()
    {
#if UNITY_EDITOR
        return true;
#else
        if (RemoteVersionInfo.bundle_version.Equals(LocalBundleVersion))
        {
            return true;
        }

        return false;
#endif
    }

    //-------------------------------------------------------------------------
    public bool remoteEqualsLocalDataVersion()
    {
        if (RemoteVersionInfo.data_version.Equals(LocalVersionInfo.data_version))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //-------------------------------------------------------------------------
    void _parseDataFileList(string data_filelist_text, bool is_local)
    {
        string[] infos = data_filelist_text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var i in infos)
        {
            if (string.IsNullOrEmpty(i))
            {
                continue;
            }

            string[] info = i.Split(' ');
            if (is_local)
            {
                if (info.Length != 2)
                {
                    //FloatMsgInfo msg_info;
                    //msg_info.msg = "本地资源文件损坏,请修复文件!";
                    //msg_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(msg_info);
                }
                else
                {
                    MapLocalAssetInfo[info[0]] = info[1];
                }
            }
            else
            {
                if (info.Length != 2)
                {
                    //FloatMsgInfo msg_info;
                    //msg_info.msg = "资源文件损坏!";
                    //msg_info.color = Color.red;
                    //UiMgr.Instance.FloatMsgMgr.createFloatMsg(msg_info);
                }
                else
                {
                    MapRemoteAssetInfo[info[0]] = info[1];
                }
            }
        }
    }
}

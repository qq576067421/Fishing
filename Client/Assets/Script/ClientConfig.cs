using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;
using GF.Common;

public class ClientConfig
{
    //-------------------------------------------------------------------------
    public string BaseIp { get; private set; }
    public int BasePort { get; private set; }
    public string ProjectName { get; private set; }
    public string ChannelName { get; private set; }
    public string ProtocolVersion { get; private set; }
    public string RemoteVersionInfoUrl { get; private set; }
    public string SysNoticeInfoUrl { get; private set; }
    public string UCenterDomain { get; private set; }
    public bool UCenterUseSsl { get; private set; }
    public string[] ListTip { get; private set; }
    SharpConfig.Configuration Cfg { get; set; }

    //-------------------------------------------------------------------------
    public ClientConfig()
    {
        _load();
    }

    //-------------------------------------------------------------------------
    void _load()
    {
        SharpConfig.Configuration.ValidCommentChars = new char[] { '#', ';' };
        var cfg_file = Resources.Load<TextAsset>("Config/ClientConfig");
        Cfg = SharpConfig.Configuration.LoadFromString(cfg_file.text);

        // 游戏服务器配置信息
        var baseserver = Cfg["BaseServer"];
        BaseIp = baseserver["BaseIp"].StringValue;
        BasePort = baseserver["BasePort"].IntValue;
        ProjectName = baseserver["ProjectName"].StringValue;
        ChannelName = baseserver["ChannelName"].StringValue;
        ProtocolVersion = baseserver["ProtocolVersion"].StringValue;

        // 自动更新配置信息
        var autopatcher = Cfg["AutoPatcher"];
        RemoteVersionInfoUrl = autopatcher["RemoteVersionInfoUrl"].StringValue;
        SysNoticeInfoUrl = autopatcher["SysNoticeInfoUrl"].StringValue;
        
        // UCenter配置信息
        var ucenter = Cfg["UCenter"];
        UCenterDomain = ucenter["UCenterDomain"].StringValue;
        UCenterUseSsl = bool.Parse(ucenter["UCenterUseSsl"].StringValue);

        // 退出游戏提示语
        var quitgametip = Cfg["QuitGameTip"];
        ListTip = quitgametip["ListTip"].GetValueArray<string>();

        EbLog.Note("ProjectName=" + ProjectName + " ProtocolVersion=" + ProtocolVersion
            + " ChannelName=" + ChannelName);
        EbLog.Note("BaseIpPort=" + BaseIp + ":" + BasePort);
    }
}

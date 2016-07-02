using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

class EsEngineListener : IEsEngineListener
{
    //-------------------------------------------------------------------------
    Entity EtApp { get; set; }

    //-------------------------------------------------------------------------
    public void init(EntityMgr entity_mgr, Entity et_node)
    {
        EbLog.Note("EsEngineListener.init()");

        EbLog.NoteCallback = Console.WriteLine;
        EbLog.WarningCallback = Console.WriteLine;
        EbLog.ErrorCallback = Console.WriteLine;

        entity_mgr.regComponent<BaseApp<DefApp>>();
        entity_mgr.regComponent<BaseBag<DefBag>>();
        entity_mgr.regComponent<BaseEquip<DefEquip>>();
        entity_mgr.regComponent<BaseLogin<DefLogin>>();
        entity_mgr.regComponent<BasePlayer<DefPlayer>>();
        entity_mgr.regComponent<BasePlayerChat<DefPlayerChat>>();
        entity_mgr.regComponent<BasePlayerDesktop<DefPlayerDesktop>>();
        entity_mgr.regComponent<BasePlayerFriend<DefPlayerFriend>>();
        entity_mgr.regComponent<BasePlayerLobby<DefPlayerLobby>>();
        entity_mgr.regComponent<BasePlayerMailBox<DefPlayerMailBox>>();
        entity_mgr.regComponent<BasePlayerRanking<DefPlayerRanking>>();
        entity_mgr.regComponent<BasePlayerTask<DefPlayerTask>>();
        entity_mgr.regComponent<BasePlayerTrade<DefPlayerTrade>>();
        entity_mgr.regComponent<BaseStatus<DefStatus>>();

        entity_mgr.regEntityDef<EtApp>();
        entity_mgr.regEntityDef<EtLogin>();
        entity_mgr.regEntityDef<EtPlayer>();

        Dictionary<string, object> cache_data = new Dictionary<string, object>();
        EtApp = entity_mgr.genEntity<EtApp>(cache_data);
    }

    //-------------------------------------------------------------------------
    public void release()
    {
        if (EtApp != null)
        {
            EtApp.close();
            EtApp = null;
        }

        EbLog.Note("EsEngineListener.release()");
    }
}

class Program
{
    //-------------------------------------------------------------------------
    static void Main(string[] args)
    {
        GrainClient.Initialize("BaseClientConfiguration.xml");

        Console.Title = "FishingBase";

        ProgramConfig config = new ProgramConfig();
        config.load("./FishingBase.exe.config");

        EsEngineSettings settings;
        settings.NodeType = 2;
        settings.NodeTypeString = "Base";
        settings.ListenIp = config.ListenIp;
        settings.ListenPort = config.ListenPort;
        settings.RootEntityType = "EtRoot";
        settings.EnableCoSupersocket = true;
        settings.Log4NetConfigPath = "../../../Media/Fishing/Config/FishingBase.log4net.config";

        try
        {
            EsEngine e = new EsEngine(ref settings, new EsEngineListener());
            e.run();
        }
        catch (System.Exception ex)
        {
            EbLog.Note(ex.ToString());
        }

        GrainClient.Uninitialize();
    }
}

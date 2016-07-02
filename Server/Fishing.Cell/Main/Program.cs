using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Host;
using GF.Common;
using GF.Server;

class EsEngineListener : IEsEngineListener
{
    //-------------------------------------------------------------------------
    Entity EtApp { get; set; }

    //-------------------------------------------------------------------------
    public void init(EntityMgr entity_mgr, Entity et_node)
    {
        EbLog.Note("EsEngineListener.init()");
    }

    //-------------------------------------------------------------------------
    public void release()
    {
        EbLog.Note("EsEngineListener.release()");
    }
}

class Program
{
    //-------------------------------------------------------------------------
    static void Main(string[] args)
    {
        Console.Title = "FishingCell";

        EsEngineSettings settings;
        settings.NodeType = 3;
        settings.NodeTypeString = "Cell";
        settings.ListenIp = "";
        settings.ListenPort = 0;
        settings.RootEntityType = "EtRoot";
        settings.EnableCoSupersocket = false;
        settings.Log4NetConfigPath = "../../../Media/Fishing/Config/FishingCell.log4net.config";

        EsEngine e = new EsEngine(ref settings, new EsEngineListener());

        var silo_host = new WindowsServerHost();

        int exit_code;
        try
        {
            if (!silo_host.ParseArguments(args))
            {
                silo_host.PrintUsage();
                exit_code = -1;
            }
            else
            {
                silo_host.Init();
                exit_code = silo_host.Run();
            }
        }
        catch (Exception ex)
        {
            EbLog.Error(string.Format("halting due to error - {0}", ex.ToString()));
            exit_code = 1;
        }
        finally
        {
            silo_host.Dispose();
            e.close();
        }

        Environment.Exit(exit_code);
    }
}

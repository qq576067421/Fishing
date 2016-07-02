using System;
using System.Net;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

// Host program for the Orleans Silo when it is being run on Windows Server machine.
public class WindowsServerHost : IDisposable
{
    //-------------------------------------------------------------------------
    // Debug flag, produces some additional log information while starting the Silo.
    public bool Debug
    {
        get { return SiloHost != null && SiloHost.Debug; }
        set { SiloHost.Debug = value; }
    }

    //-------------------------------------------------------------------------
    // Reference to the SiloHost in this process. 
    public SiloHost SiloHost { get; private set; }

    //-------------------------------------------------------------------------
    // Initialization function -- loads silo config information. 
    public void Init()
    {
        SiloHost.ConfigFileName = "CellOrleansConfiguration.xml";
        SiloHost.DeploymentId = "Fishing";
        SiloHost.LoadOrleansConfig();

        SiloHost.Config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.Custom;
        SiloHost.Config.Globals.MembershipTableAssembly = "OrleansConsulUtils";
        SiloHost.Config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.Disabled;
    }

    //-------------------------------------------------------------------------
    // Run fucntion for the Silo.
    // If the Silo starts up successfully, then this method will block and not return 
    // until the silo shutdown event is triggered, or the silo shuts down for some other reason.
    // If the silo fails to star, then a StartupError.txt summary file will be written, 
    // and a process mini-dump will be created in the current working directory.
    // <returns>Returns <c>false</c> is Silo failed to start up correctly.</returns>
    public int Run()
    {
        bool ok;

        try
        {
            SiloHost.InitializeOrleansSilo();
            ok = SiloHost.StartOrleansSilo();

            if (ok)
            {
                EbLog.Note(string.Format("Successfully started Orleans silo '{0}' as a {1} node.", SiloHost.Name, SiloHost.Type));
                SiloHost.WaitForOrleansSiloShutdown();
            }
            else
            {
                EbLog.Error(string.Format("Failed to start Orleans silo '{0}' as a {1} node.", SiloHost.Name, SiloHost.Type));
            }

            Console.WriteLine(string.Format("Orleans silo '{0}' shutdown.", SiloHost.Name));
        }
        catch (Exception exc)
        {
            SiloHost.ReportStartupError(exc);
            //TraceLogger.CreateMiniDump();
            ok = false;
        }

        return ok ? 0 : 1;
    }

    //-------------------------------------------------------------------------
    // Parse command line arguments, to allow override of some silo runtime config settings.
    // <param name="args">Command line arguments, as received by the Main program.</param>
    public bool ParseArguments(string[] args)
    {
        string silo_name = Dns.GetHostName(); // Default to machine name
        SiloHost = new SiloHost(silo_name);

        int argPos = 1;
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            if (a.StartsWith("-") || a.StartsWith("/"))
            {
                switch (a.ToLowerInvariant())
                {
                    case "/?":
                    case "/help":
                    case "-?":
                    case "-help":
                        // Query usage help
                        return false;
                    case "/debug":
                        SiloHost.Debug = true;
                        break;
                    default:
                        EbLog.Error("Bad command line arguments supplied: " + a);
                        return false;
                }
            }
            else if (a.Contains("="))
            {
                string[] split = a.Split('=');
                if (String.IsNullOrEmpty(split[1]))
                {
                    EbLog.Error("Bad command line arguments supplied: " + a);
                    return false;
                }
                switch (split[0].ToLowerInvariant())
                {
                    case "deploymentid":
                        SiloHost.DeploymentId = split[1];
                        break;
                    case "deploymentgroup":
                        Console.WriteLine("Ignoring deprecated command line argument: " + a);
                        break;
                    default:
                        EbLog.Error("Bad command line arguments supplied: " + a);
                        return false;
                }
            }
            // unqualified arguments below
            else if (argPos == 1)
            {
                SiloHost.Name = a;
                argPos++;
            }
            else if (argPos == 2)
            {
                SiloHost.ConfigFileName = a;
                argPos++;
            }
            else
            {
                // Too many command line arguments
                EbLog.Error("Too many command line arguments supplied: " + a);
                return false;
            }
        }

        return true;
    }

    //-------------------------------------------------------------------------
    // Print usage info to console window, showing cmd-line params for OrleansHost.exe
    public void PrintUsage()
    {
        EbLog.Note(
@"USAGE: 
    OrleansHost.exe [<siloName> [<configFile>]] [DeploymentId=<idString>] [/debug]
Where:
    <siloName>      - Name of this silo in the Config file list (optional)
    <configFile>    - Path to the Config file to use (optional)
    DeploymentId=<idString> 
                    - Which deployment group this host instance should run in (optional)
    /debug          - Turn on extra debug output during host startup (optional)");
    }

    //-------------------------------------------------------------------------
    public void Dispose()
    {
        Dispose(true);
    }

    //-------------------------------------------------------------------------
    protected virtual void Dispose(bool dispose)
    {
        SiloHost.Dispose();
        SiloHost = null;
    }
}

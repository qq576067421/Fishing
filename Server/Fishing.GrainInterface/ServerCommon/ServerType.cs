using System;
using System.Collections.Generic;
using System.IO;
using GF.Common;
using GF.Server;

namespace Ps
{
    public static class ServerPath
    {
        //-------------------------------------------------------------------------
        // 获取到../Media/目录的位置
        public static string getPathMediaRoot()
        {
            string file_name = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string p1 = Path.Combine(file_name, "..\\..\\..\\..\\Media\\");
            string p2 = Path.GetFullPath(p1);
            return p2;
        }
    }

    [Serializable]
    public class MethodData
    {
        public MethodType method_id;
        public byte[] param1;
        public byte[] param2;
        public byte[] param3;
        public byte[] param4;
    }

    public class NewPlayerInfo
    {
        public string account_id = "";
        public string account_name = "";
        public bool is_bot = false;
        public bool gender = true;
        public string et_player_guid = "";
    }
}

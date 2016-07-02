using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using GF.Common;

namespace Ps
{
    public static class StringDef
    {
        public static string SMSProvider = "SMSProvider";
    }

    public enum StreamEventId : byte
    {
        None = 0,
        DesktopStreamEvent = 10,// Desktop
        FriendStreamEvent = 20,// Friend
    }

    public class StreamData
    {
        public StreamEventId event_id;
        public object param1;
        public object param2;
        public object param3;
        public object param4;
    }

    [Serializable]
    public class CachePlayerData
    {
        public string player_etguid = "";
        public PlayerServerState player_server_state = PlayerServerState.Hosting;
    }
}

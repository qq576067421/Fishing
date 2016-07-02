using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using GF.Common;

namespace Ps
{
    public interface ICellPlayer : IGrainWithGuidKey
    {
        //---------------------------------------------------------------------
        Task subPlayer(ICellPlayerObserver sub, IPEndPoint remote_endpoint);

        //---------------------------------------------------------------------
        Task unsubPlayer(ICellPlayerObserver sub);

        //---------------------------------------------------------------------
        // Client->Cell的请求
        Task<EntityData> c2sEnterWorld(NewPlayerInfo new_player_info);

        //---------------------------------------------------------------------
        // Client->Cell的请求
        Task<MethodData> c2sRequest(MethodData method_data);

        //---------------------------------------------------------------------
        // Cell->Client的通知
        Task s2sNotify(MethodData method_data);

        //---------------------------------------------------------------------
        // Cell->Cell的请求
        Task s2sBotEnterWorld(string player_etguid);

        //---------------------------------------------------------------------
        // Cell->Cell的请求
        Task s2sDesktop2Player(List<string> vec_param);
    }
}

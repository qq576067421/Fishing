using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;

namespace Ps
{
    // 机器人无状态服务
    public interface ICellBotService : IGrainWithIntegerKey
    {
        //---------------------------------------------------------------------
        Task addBot(ulong acc_id_min, ulong acc_id_max);

        //---------------------------------------------------------------------
        Task addBot(string acc_name);

        //---------------------------------------------------------------------
        Task removeBot(ulong acc_id_min, ulong acc_id_max);

        //---------------------------------------------------------------------
        Task removeBot(string acc_name);
    }
}

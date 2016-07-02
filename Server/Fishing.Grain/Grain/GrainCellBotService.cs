using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using GF.Common;

namespace Ps
{
    // 机器人无状态服务
    [Reentrant]
    [StatelessWorker]
    public class GrainCellBotService : Grain, ICellBotService
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }

        //---------------------------------------------------------------------
        Task ICellBotService.addBot(ulong acc_id_min, ulong acc_id_max)
        {
            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        Task ICellBotService.addBot(string acc_name)
        {
            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        Task ICellBotService.removeBot(ulong acc_id_min, ulong acc_id_max)
        {
            return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        Task ICellBotService.removeBot(string acc_name)
        {
            return TaskDone.Done;
        }
    }
}

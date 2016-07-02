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
    // FishingMgr管理后台无状态服务
    [Reentrant]
    [StatelessWorker]
    public class GrainCellFishingMgrService : Grain, ICellFishingMgrService
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }

        //---------------------------------------------------------------------
        Task<string> ICellFishingMgrService.dummy()
        {
            return Task.FromResult("GrainCellFishingMgrService~~~~~~~~~~~~");
        }
    }
}

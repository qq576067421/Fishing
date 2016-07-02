using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
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
    // 跑马灯新闻组，提供无状态服务
    [Reentrant]
    [StatelessWorker]
    public class GrainCellMarqueeNewsService : Grain, ICellMarqueeNewsService
    {
        //---------------------------------------------------------------------
        public Logger Logger { get { return GetLogger(); } }

        //---------------------------------------------------------------------
        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromHours(24));

            return base.OnActivateAsync();
        }

        //---------------------------------------------------------------------
        public override Task OnDeactivateAsync()
        {
            Logger.Info("OnDeactivateAsync()");

            return base.OnDeactivateAsync();
        }

        //---------------------------------------------------------------------
        Task ICellMarqueeNewsService.dummy()
        {
            return TaskDone.Done;
        }
    }
}

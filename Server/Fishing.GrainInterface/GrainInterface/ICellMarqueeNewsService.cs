using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;

namespace Ps
{
    public interface ICellMarqueeNewsService : IGrainWithIntegerKey
    {
        //---------------------------------------------------------------------
        Task dummy();
    }
}

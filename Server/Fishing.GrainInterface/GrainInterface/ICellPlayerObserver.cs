using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;

namespace Ps
{
    public interface ICellPlayerObserver : IGrainObserver
    {
        //---------------------------------------------------------------------
        void s2cNotify(MethodData method_data);
    }
}

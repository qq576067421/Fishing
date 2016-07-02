using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EtDesktop : EntityDef
    {
        //---------------------------------------------------------------------
        public override void declareAllComponent(byte node_type)
        {
            declareComponent<DefDesktop>();
        }
    }
}

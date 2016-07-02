using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EtLogin : EntityDef
    {
        //---------------------------------------------------------------------
        public override void declareAllComponent(byte node_type)
        {
            // All
            declareComponent<DefLogin>();
        }
    }
}

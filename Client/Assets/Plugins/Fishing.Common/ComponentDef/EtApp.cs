using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EtApp : EntityDef
    {
        //---------------------------------------------------------------------
        public override void declareAllComponent(byte node_type)
        {
            declareComponent<DefApp>();
        }
    }
}

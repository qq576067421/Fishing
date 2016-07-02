using System;
using System.Collections.Generic;
using GF.Common;

public class EtNode : EntityDef
{
    //---------------------------------------------------------------------
    public override void declareAllComponent(byte node_type)
    {
        declareComponent<DefNode>();
    }
}

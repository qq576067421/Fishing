using System;
using System.Collections.Generic;
using GF.Common;

public class EtUCenterSDK : EntityDef
{
    //-------------------------------------------------------------------------
    public override void declareAllComponent(byte node_type)
    {
        declareComponent<DefUCenterSDK>();
    }
}

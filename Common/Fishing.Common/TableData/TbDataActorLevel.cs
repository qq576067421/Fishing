using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class TbDataActorLevel : EbData
{
    //-------------------------------------------------------------------------
    public int Level { get; private set; }
    public int Experience { get; private set; }

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        Level = prop_set.getPropInt("Level").get();
        Experience = prop_set.getPropInt("Experience").get();
    }
}

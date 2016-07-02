using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectScreenShock : EbData
    {
        //-------------------------------------------------------------------------
        public int LastTime;

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            LastTime = prop_set.getPropInt("I_LastTime").get();
        }
    }
}

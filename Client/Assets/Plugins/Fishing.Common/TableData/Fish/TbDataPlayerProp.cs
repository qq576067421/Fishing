using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataPlayerProp : EbData
    {
        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public string PlayerPropKey { get; private set; }
        public string PlayerPropIcon { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            PlayerPropKey = prop_set.getPropString("T_PlayerPropKey").get();
            PlayerPropIcon = prop_set.getPropString("T_PlayerPropIcon").get();
        }
    }
}

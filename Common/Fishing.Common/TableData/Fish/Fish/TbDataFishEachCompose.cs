using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataFishEachCompose : EbData
    {
        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public int FishVibId { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            FishVibId = prop_set.getPropInt("I_FishVibId").get();
            OffsetX = prop_set.getPropInt("I_OffsetX").get();
            OffsetY = prop_set.getPropInt("I_OffsetY").get();
        }
    }
}

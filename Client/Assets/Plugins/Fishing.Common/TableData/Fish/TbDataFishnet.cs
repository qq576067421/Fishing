using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataFishnet : EbData
    {
        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public string AnimName { get; private set; }
        public int FishnetPixelHeight { get; private set; }
        public int FishnetHeight { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            AnimName = prop_set.getPropString("T_AnimName").get();
            FishnetPixelHeight = prop_set.getPropInt("I_FishnetPixelHeight").get();
            FishnetHeight = prop_set.getPropInt("I_FishnetHeight").get();
        }
    }
}

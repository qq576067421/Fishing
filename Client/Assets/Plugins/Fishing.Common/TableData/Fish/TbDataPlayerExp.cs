using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataPlayerExp : EbData
    {
        //-------------------------------------------------------------------------
        public int Name { get; private set; }
        public int LevelExp { get; private set; }//等级经验    
        public int LevelReward { get; private set; }// 等级奖励


        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropInt("I_Name").get();
            LevelExp = prop_set.getPropInt("I_LevelExp").get();
            LevelReward = prop_set.getPropInt("I_LevelReward").get();
        }
    }
}

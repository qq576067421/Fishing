using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataSinglePlayerLife : EbData
    {
        //-------------------------------------------------------------------------
        public int PlayerMaxLife { get; private set; } // 玩家生命最大值
        public int TotalMinute { get; private set; }   // 倒计时 - 分
        public int TotalSecond { get; private set; } // 倒计时 - 秒

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            PlayerMaxLife = prop_set.getPropInt("I_PlayerMaxLife").get();
            TotalMinute = prop_set.getPropInt("I_TotalMinute").get();
            TotalSecond = prop_set.getPropInt("I_TotalSecond").get();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataVipLevel : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public int VipLevelFishExpAdd { get; private set; }    // 经验加成
        public int VipLevelHitRateAdd;   // 命中加成
        public int VipLevelDailyGoldAdd { get; private set; }    // 每日领奖加成
        public int VipLevelFreeRoseId { get; private set; }
        public int VipLevelFreeRoseAmount { get; private set; }
        public int VipLevelFreeShitId { get; private set; }
        public int VipLevelFreeShitAmount { get; private set; }
        public int VipLevelCostRMB { get; private set; }// 花费人民币

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            VipLevelFishExpAdd = prop_set.getPropInt("I_VipLevelFishExpAdd").get();
            VipLevelHitRateAdd = prop_set.getPropInt("I_VipLevelHitRateAdd").get();
            VipLevelDailyGoldAdd = prop_set.getPropInt("I_VipLevelDailyGoldAdd").get();
            VipLevelFreeRoseId = prop_set.getPropInt("I_VipLevelFreeRoseId").get();
            VipLevelFreeRoseAmount = prop_set.getPropInt("I_VipLevelFreeRoseAmount").get();
            VipLevelFreeShitId = prop_set.getPropInt("I_VipLevelFreeShitId").get();
            VipLevelFreeShitAmount = prop_set.getPropInt("I_VipLevelFreeShitAmount").get();
            VipLevelCostRMB = prop_set.getPropInt("I_VipLevelCostRMB").get();
        }
    }
}

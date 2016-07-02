using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectSpreadFish : EbData
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
        public TbDataFish NormalFish { get; private set; }
        public TbDataFish RedFish { get; private set; }
        public int SpreadCount { get; private set; } //扩散的波数
        public int GapTime { get; private set; }
        public int FishCount { get; private set; }  //每圈的个数
        public float Speed { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            NormalFish = EbDataMgr.Instance.getData<TbDataFish>(prop_set.getPropInt("I_NormalFish").get());
            RedFish = EbDataMgr.Instance.getData<TbDataFish>(prop_set.getPropInt("I_RedFish").get());
            SpreadCount = prop_set.getPropInt("I_SpreadCount").get();
            GapTime = prop_set.getPropInt("I_GapTime").get();
            FishCount = prop_set.getPropInt("I_FishCount").get();
            Speed = prop_set.getPropFloat("R_Speed").get();
        }
    }
}

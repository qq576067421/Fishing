using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataShoal : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum _eShoalType
        {
            Default = -1,
            TAIL_BEHIND = 0,
            CONVERGED = 1
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public TbDataFish Fish;
        public int FishAmount;
        public _eShoalType ShoalType;

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            Fish = EbDataMgr.Instance.getData<TbDataFish>(prop_set.getPropInt("I_Fish").get());
            FishAmount = prop_set.getPropInt("I_FishAmount").get();
            ShoalType = (_eShoalType)prop_set.getPropInt("I_ShoalType").get();
        }
    }
}

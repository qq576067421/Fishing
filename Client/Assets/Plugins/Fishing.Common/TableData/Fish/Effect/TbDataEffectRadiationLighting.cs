using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectRadiationLighting : EbData
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
        public TbDataFish NormalFish;
        public TbDataFish RedFish;
        public TbDataFish RedFishDouble;
        public int LastTime;

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            NormalFish = EbDataMgr.Instance.getData<TbDataFish>(prop_set.getPropInt("I_NormalFish").get());
            RedFish = EbDataMgr.Instance.getData<TbDataFish>(prop_set.getPropInt("I_RedFish").get());
            RedFishDouble = EbDataMgr.Instance.getData<TbDataFish>(prop_set.getPropInt("I_RedFishDouble").get());
            LastTime = prop_set.getPropInt("I_LastTime").get();
        }
    }
}

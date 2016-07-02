using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataBuff : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        //-------------------------------------------------------------------------
        public DataState State { get; private set; }
        public int EffectIndex { get; private set; }
        public TbDataEffectName EffectTypeEnum { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            EffectIndex = prop_set.getPropInt("I_EffectIndex").get();
            EffectTypeEnum = EbDataMgr.Instance.getData<TbDataEffectName>(prop_set.getPropInt("I_EffectTypeEnum").get());
        }
    }
}

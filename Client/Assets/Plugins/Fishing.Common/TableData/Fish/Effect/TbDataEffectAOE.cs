using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectAOE : EbData
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
        public int Radius;
        public TbDataParticle ParticleName;

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            Radius = prop_set.getPropInt("I_Radius").get();
            ParticleName = EbDataMgr.Instance.getData<TbDataParticle>(prop_set.getPropInt("I_ParticleName").get());
        }
    }
}

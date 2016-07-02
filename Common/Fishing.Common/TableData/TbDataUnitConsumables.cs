using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class TbDataUnitConsumables : EbData
    {
        //-------------------------------------------------------------------------
        public List<EffectData> ListEffect { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            ListEffect = new List<EffectData>();

            int effect_id1 = prop_set.getPropInt("I_EffectId1").get();
            if (effect_id1 > 0)
            {
                EffectData effect_info = new EffectData();
                effect_info.EffectId = effect_id1;
                string effect_param = prop_set.getPropString("T_EffectParam1").get();
                if (!string.IsNullOrEmpty(effect_param)) effect_info.ListParam = effect_param.Split(';');
                ListEffect.Add(effect_info);
            }

            int effect_id2 = prop_set.getPropInt("I_EffectId2").get();
            if (effect_id2 > 0)
            {
                EffectData effect_info = new EffectData();
                effect_info.EffectId = effect_id2;
                string effect_param = prop_set.getPropString("T_EffectParam2").get();
                if (!string.IsNullOrEmpty(effect_param)) effect_info.ListParam = effect_param.Split(';');
                ListEffect.Add(effect_info);
            }

            int effect_id3 = prop_set.getPropInt("I_EffectId3").get();
            if (effect_id3 > 0)
            {
                EffectData effect_info = new EffectData();
                effect_info.EffectId = effect_id3;
                string effect_param = prop_set.getPropString("T_EffectParam3").get();
                if (!string.IsNullOrEmpty(effect_param)) effect_info.ListParam = effect_param.Split(';');
                ListEffect.Add(effect_info);
            }
        }
    }
}

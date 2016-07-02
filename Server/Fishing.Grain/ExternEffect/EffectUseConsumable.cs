using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 使用消耗品
    public class EffectUseConsumable : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            UnitConsumables unit_consumable = (UnitConsumables)item.UnitLink;
            foreach (var i in unit_consumable.TbDataUnitConsumables.ListEffect)
            {
                TbDataEffect data_effect = EbDataMgr.Instance.getData<TbDataEffect>(i.EffectId);

                effect_mgr.doEffect(effect_context, data_effect.ScriptName,
                    data_effect.PredefineParamList, i.ListParam);
            }

            item.ItemData.n -= (byte)item.TbDataItem.MainOperateInfo.SubOverlapNum;

            return null;
        }
    }
}

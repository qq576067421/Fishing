using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 穿装备
    public class EffectTakeonEquip : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            //EbLog.Note("EffectTakeonEquip.excute()");

            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            // 添加到装备背包
            var co_equip = et.getComponent<CellEquip<DefEquip>>();
            co_equip.takeonEquip(item);

            return null;
        }
    }
}

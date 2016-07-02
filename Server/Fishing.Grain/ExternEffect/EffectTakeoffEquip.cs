using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 脱装备
    public class EffectTakeoffEquip : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            //EbLog.Note("EffectTakeoffEquip.excute()");

            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            // 从装备背包中移除
            var co_equip = et.getComponent<CellEquip<DefEquip>>();
            item = co_equip.takeoffEquip((Ps.EquipSlot)item.TbDataItem.ItemTypeId);

            return null;
        }
    }
}

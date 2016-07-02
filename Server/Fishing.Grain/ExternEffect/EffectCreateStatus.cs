using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 创建状态
    public class EffectCreateStatus : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            //EbLog.Note("EffectCreateStatus.excute()");

            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            // 创建Status
            var co_status = et.getComponent<CellStatus<DefStatus>>();
            co_status.createStatus(item);

            return null;
        }
    }
}

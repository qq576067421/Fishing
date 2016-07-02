using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 状态生成器2
    // 带有几率参数，有一定几率创建指定状态
    public class EffectStatusCreator2 : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            //EbLog.Note("EffectStatusCreator2.excute()");

            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            // 创建Status
            var co_status = et.getComponent<CellStatus<DefStatus>>();
            co_status.createStatus(item);

            return null;
        }
    }
}

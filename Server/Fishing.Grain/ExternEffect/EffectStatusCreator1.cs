using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 状态生成器1
    // 最普通的状态生成器，必创建指定状态
    public class EffectStatusCreator1 : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            //EbLog.Note("EffectStatusCreator1.excute()");

            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            // 创建Status
            var co_status = et.getComponent<CellStatus<DefStatus>>();
            co_status.createStatus(item);

            return null;
        }
    }
}

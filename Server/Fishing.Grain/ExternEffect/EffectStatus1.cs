using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 状态元模板1
    // 最普通的状态元模板，每秒定时执行一次Effect
    // 参数：总秒数
    public class EffectStatus1 : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            //EbLog.Note("EffectStatus1.excute()");

            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            // 创建Status
            var co_status = et.getComponent<CellStatus<DefStatus>>();
            co_status.createStatus(item);

            return null;
        }
    }
}

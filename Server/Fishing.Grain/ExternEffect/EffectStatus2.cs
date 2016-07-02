using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 状态元模板2
    // 指定间隔定时执行一次Effect
    // 参数：总秒数
    // 参数：间隔时间（秒）
    public class EffectStatus2 : Effect
    {
        //-------------------------------------------------------------------------
        public override object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param)
        {
            //EbLog.Note("EffectStatus2.excute()");

            Item item = effect_context.item;
            Entity et = effect_context.EtActor;

            // 创建Status
            var co_status = et.getComponent<CellStatus<DefStatus>>();
            co_status.createStatus(item);

            return null;
        }
    }
}

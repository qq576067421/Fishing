using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GF.Common;

namespace Ps
{
    // 行为树子节点：玩家手动操作
    public class BtChildPlayerManual : BtChild
    {
        //-------------------------------------------------------------------------

        //-------------------------------------------------------------------------
        public BtChildPlayerManual(IComponent co_actorai, Blackboard blackboard, BehaviorTree bt)
            : base(co_actorai, blackboard, bt)
        {
        }

        //-------------------------------------------------------------------------
        public override BehaviorComponent child()
        {
            Action1 action_dummy = new Action1(BehaviorTree, actionDummy);

            var node_playermanual = new Selector(BehaviorTree, action_dummy);

            Conditional con_canmanual = new Conditional(BehaviorTree, conditionPlayerManual);
            var child = new Sequence(BehaviorTree, con_canmanual, node_playermanual);

            return child;
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public bool conditionPlayerManual(BehaviorTree bt, params object[] list_param)
        {
            return true;
        }

        //-------------------------------------------------------------------------
        public BehaviorReturnCode actionDummy(BehaviorTree bt, params object[] list_param)
        {
            return BehaviorReturnCode.Success;
        }
    }
}

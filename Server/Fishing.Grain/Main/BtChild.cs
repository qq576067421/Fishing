using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GF.Common;

namespace Ps
{
    public abstract class BtChild
    {
        //-------------------------------------------------------------------------
        public CellActorMirrorAi<DefActorMirrorAi> CoActorAi { get; private set; }
        public Blackboard Blackboard { get; private set; }
        public BehaviorTree BehaviorTree { get; private set; }

        //-------------------------------------------------------------------------
        public BtChild(IComponent co_actorai, Blackboard blackboard, BehaviorTree bt)
        {
            CoActorAi = (CellActorMirrorAi<DefActorMirrorAi>)co_actorai;
            Blackboard = blackboard;
            BehaviorTree = bt;
        }

        //-------------------------------------------------------------------------
        public abstract BehaviorComponent child();

        //-------------------------------------------------------------------------
        public abstract void update(float elapsed_tm);
    }
}

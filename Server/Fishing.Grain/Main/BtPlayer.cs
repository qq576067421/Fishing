using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class BtPlayer : Bt
    {
        //-------------------------------------------------------------------------
        CellActorMirror<DefActorMirror> CoActor { get; set; }
        CellActorMirrorAi<DefActorMirrorAi> CoActorAi { get; set; }
        BtChildPlayerManual BtChildPlayerManual { get; set; }

        //-------------------------------------------------------------------------
        public BtPlayer(Entity self)
            : base(self)
        {
            CoActor = self.getComponent<CellActorMirror<DefActorMirror>>();
            CoActorAi = self.getComponent<CellActorMirrorAi<DefActorMirrorAi>>();
        }

        //-------------------------------------------------------------------------
        public override void _init()
        {
            BtChildPlayerManual = new BtChildPlayerManual(CoActorAi, Blackboard, BehaviorTree);

            var root = new Selector(mBehaviorTree,
                BtChildPlayerManual.child()
                );

            mBehaviorTree.setRoot(root);
        }

        //-------------------------------------------------------------------------
        public override void close()
        {
            CoActor = null;
            CoActorAi = null;
            BtChildPlayerManual = null;
            mBehaviorTree = null;
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            BtChildPlayerManual.update(elapsed_tm);

            base.update(elapsed_tm);
        }
    }

    public class BtFactoryPlayer : BtFactory
    {
        //-------------------------------------------------------------------------
        public override string getName()
        {
            return "BtPlayer";
        }

        //-------------------------------------------------------------------------
        public override Bt createBt(Entity self)
        {
            BtPlayer bt = new BtPlayer(self);
            bt._init();
            return bt;
        }
    }
}

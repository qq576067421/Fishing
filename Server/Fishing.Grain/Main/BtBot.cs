using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GF.Common;

namespace Ps
{
    public class BtBot : Bt
    {
        //-------------------------------------------------------------------------
        CellActorMirror<DefActorMirror> CoActor { get; set; }
        CellActorMirrorAi<DefActorMirrorAi> CoActorAi { get; set; }

        //-------------------------------------------------------------------------
        List<string> TalkList { get; set; }
        float TalkTmTotal { get; set; }
        float TalkTmCur { get; set; }
        int Count { get; set; }
        int Index { get; set; }

        //-------------------------------------------------------------------------
        public BtBot(Entity self)
            : base(self)
        {
            CoActor = self.getComponent<CellActorMirror<DefActorMirror>>();
            CoActorAi = self.getComponent<CellActorMirrorAi<DefActorMirrorAi>>();
        }

        //-------------------------------------------------------------------------
        public override void _init()
        {
            //var root = new Selector(mBehaviorTree,
            //    BtChildPursue.child(),
            //    BtChildDie.child()
            //    );

            //mBehaviorTree.setRoot(root);
        }

        //-------------------------------------------------------------------------
        public override void close()
        {
            CoActor = null;
            CoActorAi = null;
            mBehaviorTree = null;
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            base.update(elapsed_tm);
        }
    }

    public class BtFactoryBot : BtFactory
    {
        //-------------------------------------------------------------------------
        public override string getName()
        {
            return "BtBot";
        }

        //-------------------------------------------------------------------------
        public override Bt createBt(Entity self)
        {
            var bt = new BtBot(self);
            bt._init();
            return bt;
        }
    }
}

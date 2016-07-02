using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public abstract class Bt
    {
        //-------------------------------------------------------------------------
        protected BehaviorTree mBehaviorTree;
        protected Entity mSelf;

        //-------------------------------------------------------------------------
        public Blackboard Blackboard { get; private set; }
        public BehaviorTree BehaviorTree { get { return mBehaviorTree; } }

        //-------------------------------------------------------------------------
        public Bt(Entity self)
        {
            // 建立行为树
            mSelf = self;
            mBehaviorTree = new BehaviorTree();
            mBehaviorTree.Blackboard.setData("Entity", mSelf);
            Blackboard = mBehaviorTree.Blackboard;
        }

        //-------------------------------------------------------------------------
        public abstract void _init();

        //-------------------------------------------------------------------------
        public abstract void close();

        //-------------------------------------------------------------------------
        public virtual void update(float elapsed_tm)
        {
            if (mBehaviorTree != null)
            {
                mBehaviorTree.Behave();
            }
        }
    }

    public abstract class BtFactory
    {
        //-------------------------------------------------------------------------
        public abstract string getName();

        //-------------------------------------------------------------------------
        public abstract Bt createBt(Entity self);
    }
}

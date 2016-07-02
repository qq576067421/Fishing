using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class StartMoveAffector : EntityAffector<StartMoveAffectorData>
    {
        //---------------------------------------------------------------------
        bool mHasDivergence = false;

        //---------------------------------------------------------------------
        public StartMoveAffector(StartMoveAffectorData affector_data) : base(affector_data) { }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (mHasDivergence) return;

            base.update(elapsed_tm);

            if (getAffectorData().mStartTime <= SecondsSinceCreation)
            {
                //开始鱼的游动
                mHasDivergence = true;
                foreach (var fish in getBaseEntity())
                {
                    fish.setSpeed(getAffectorData().mSpeed);
                }
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
        }
    }

    public class StartMoveAffectorFactory : EntityAffectorFactory
    {
        //---------------------------------------------------------------------
        public override string getAffectorType()
        {
            return typeof(StartMoveAffectorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityAffector buildAffector(EntityAffectorData affector_data)
        {
            return new StartMoveAffector((StartMoveAffectorData)affector_data);
        }

        public override EntityAffectorData buildAffectorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<StartMoveAffectorData>(json_item.mJsonString);
        }
    }

    public class StartMoveAffectorData : EntityAffectorData
    {
        public float mStartTime = 0;
        public float mSpeed = 0;

        public override EntityAffectorData clone()
        {
            StartMoveAffectorData affector_data = new StartMoveAffectorData();

            affector_data.mStartTime = mStartTime;
            affector_data.mSpeed = mSpeed;

            return affector_data;
        }
    }
}

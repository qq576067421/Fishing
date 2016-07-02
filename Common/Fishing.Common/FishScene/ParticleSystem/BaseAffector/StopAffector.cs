using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class StopAffector : EntityAffector<StopAffectorData>
    {
        //---------------------------------------------------------------------
        bool mHasDivergence = false;

        //---------------------------------------------------------------------
        public StopAffector(StopAffectorData affector_data) : base(affector_data) { }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (mHasDivergence) return;

            base.update(elapsed_tm);

            if (getAffectorData().mStartTime <= SecondsSinceCreation)
            {
                //开始停止鱼的游动
                mHasDivergence = true;
                //EbLog.Warning("StopAffector begin stop");

                foreach (var fish in getBaseEntity())
                {
                    fish.setSpeed(0);
                }
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
        }
    }

    public class StopAffectorFactory : EntityAffectorFactory
    {
        //---------------------------------------------------------------------
        public override string getAffectorType()
        {
            return typeof(StopAffectorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityAffector buildAffector(EntityAffectorData affector_data)
        {
            return new StopAffector((StopAffectorData)affector_data);
        }

        public override EntityAffectorData buildAffectorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<StopAffectorData>(json_item.mJsonString);
        }
    }

    public class StopAffectorData : EntityAffectorData
    {
        public float mStartTime = 0;
        public float mDuration = 0;

        public override EntityAffectorData clone()
        {
            StopAffectorData affector_data = new StopAffectorData();

            affector_data.mStartTime = mStartTime;
            affector_data.mDuration = mDuration;

            return affector_data;
        }
    }
}

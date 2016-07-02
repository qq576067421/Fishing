using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class AngleSpeedAffector : EntityAffector<AngleSpeedAffectorData>
    {
        //---------------------------------------------------------------------
        bool mHasDivergence = false;

        //---------------------------------------------------------------------
        public AngleSpeedAffector(AngleSpeedAffectorData affector_data) : base(affector_data) { }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (mHasDivergence) return;

            base.update(elapsed_tm);

            if (getAffectorData().mStartTime <= SecondsSinceCreation)
            {
                //开始停止鱼的游动
                mHasDivergence = true;

                foreach (var fish in getBaseEntity())
                {
                    fish.setAngleSpeed(getAffectorData().mAngleSpeed);
                    fish.setSpeed(getAffectorData().mSpeed);
                }
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
        }
    }

    public class AngleSpeedAffectorFactory : EntityAffectorFactory
    {
        //---------------------------------------------------------------------
        public override string getAffectorType()
        {
            return typeof(AngleSpeedAffectorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityAffector buildAffector(EntityAffectorData affector_data)
        {
            return new AngleSpeedAffector((AngleSpeedAffectorData)affector_data);
        }

        public override EntityAffectorData buildAffectorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<AngleSpeedAffectorData>(json_item.mJsonString);
        }
    }

    public class AngleSpeedAffectorData : EntityAffectorData
    {
        public float mStartTime = 0;
        public float mAngleSpeed = 0;
        public float mSpeed = 0;

        public override EntityAffectorData clone()
        {
            AngleSpeedAffectorData affector_data = new AngleSpeedAffectorData();

            affector_data.mStartTime = mStartTime;
            affector_data.mAngleSpeed = mAngleSpeed;
            affector_data.mSpeed = mSpeed;

            return affector_data;
        }
    }
}

using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class IntervalDivergenceGenerator : EntityGenerator<IntervalDivergenceGeneratorData>
    {
        //---------------------------------------------------------------------
        float mDeltaTime = 0f;
        int mNeedWave = 0;
        System.Random mRandom = new System.Random(unchecked((int)System.DateTime.Now.Ticks));

        //---------------------------------------------------------------------
        public IntervalDivergenceGenerator(IntervalDivergenceGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
            : base(generator_data, server_param, route_object_mgr)
        {
            mNeedWave = generator_data.mWaveNumber;
        }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (Done) return;

            if (mNeedWave <= 0)
            {
                setDone();
                return;
            }

            base.update(elapsed_tm);

            mDeltaTime += elapsed_tm;

            if (getGeneratorData().mStartTime < getPassedTime())
            {
                if (mDeltaTime > getGeneratorData().mDeltaTime)
                {
                    mNeedWave--;
                    mDeltaTime = 0;

                    if (getGeneratorData().mAmount == 0)
                    {
                        return;
                    }
                    float delta_angle = 360f / getGeneratorData().mAmount;
                    EbVector3 position = new EbVector3(getGeneratorData().mGenerationPointX, getGeneratorData().mGenerationPointY, 0);
                    for (int i = getGeneratorData().mAmount; i > 0; i--)
                    {
                        BaseEntity entity = buildEntity(getGeneratorData().mFishVibID);

                        entity.setPosition(position);
                        entity.setDirection(delta_angle * i + getRandoNumber() * getGeneratorData().mRandomAngle);
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        float getRandoNumber()
        {
            return mRandom.Next(0, 100000) / 100000.0f - 0.5f;
        }

        //---------------------------------------------------------------------
        public override void destroy() { base.destroy(); }
    }

    public class IntervalDivergenceGeneratorFactory : EntityGeneratorFactory
    {
        //---------------------------------------------------------------------
        public override string getGeneratorType()
        {
            return typeof(IntervalDivergenceGeneratorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityGenerator buildGenerator(EntityGeneratorData generator_data,
            List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return new IntervalDivergenceGenerator((IntervalDivergenceGeneratorData)generator_data, server_param, route_object_mgr);
        }

        public override EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<IntervalDivergenceGeneratorData>(json_item.mJsonString);
        }
    }

    public class IntervalDivergenceGeneratorData : EntityGeneratorData
    {
        public int mFishVibID = 0;
        public float mGenerationPointX = 0;
        public float mGenerationPointY = 0;
        public int mAmount = 0;
        public float mStartTime = 0f;
        public float mDeltaTime = 0f;
        public int mWaveNumber = 0;
        public float mRandomAngle = 0f;

        public override EntityGeneratorData clone()
        {
            IntervalDivergenceGeneratorData generator_data = new IntervalDivergenceGeneratorData();

            generator_data.mFishVibID = mFishVibID;
            generator_data.mGenerationPointX = mGenerationPointX;
            generator_data.mGenerationPointY = mGenerationPointY;
            generator_data.mAmount = mAmount;
            generator_data.mStartTime = mStartTime;
            generator_data.mDeltaTime = mDeltaTime;
            generator_data.mWaveNumber = mWaveNumber;
            generator_data.mRandomAngle = mRandomAngle;

            return generator_data;
        }

        public override int getBaseEntityCount()
        {
            return mAmount * mWaveNumber;
        }

        public override string ToString()
        {
            return string.Format("mFishVibID {0},mGenerationPoint({1},{2}),mAmount{3},mStartTime{4}"
                , mFishVibID, mGenerationPointX, mGenerationPointY, mAmount, mStartTime);
        }
    }
}

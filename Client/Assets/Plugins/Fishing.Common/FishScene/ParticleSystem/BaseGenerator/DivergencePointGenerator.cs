using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class DivergencePointGenerator : EntityGenerator<DivergencePointGeneratorData>
    {
        //---------------------------------------------------------------------
        public DivergencePointGenerator(DivergencePointGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
            : base(generator_data, server_param, route_object_mgr)
        { }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (Done) return;

            base.update(elapsed_tm);

            if (getGeneratorData().mStartTime < getPassedTime())
            {
                if (getGeneratorData().mAmount != 0)
                {
                    float delta_angle = 360f / getGeneratorData().mAmount;
                    EbVector3 position = new EbVector3(getGeneratorData().mGenerationPointX, getGeneratorData().mGenerationPointY, 0);
                    BaseEntity base_entity = null;
                    for (int i = getGeneratorData().mAmount; i > 0; i--)
                    {

                        if (getGeneratorData().mRadius == 0)
                        {
                            base_entity = buildEntity(getGeneratorData().mFishVibID);
                            base_entity.setPosition(position);
                            base_entity.setDirection(delta_angle * i);
                        }
                        else
                        {
                            base_entity = buildEntity(getGeneratorData().mFishVibID);
                            base_entity.setPosition(position + CLogicUtility.getDirection(delta_angle * i) * getGeneratorData().mRadius);
                            base_entity.setDirection(delta_angle * i);
                        }

                        base_entity.setSpeed(getGeneratorData().mSpeed);
                    }
                }

                setDone();
            }
        }

        //---------------------------------------------------------------------
        public override void destroy() { base.destroy(); }
    }

    public class DivergencePointGeneratorFactory : EntityGeneratorFactory
    {
        //---------------------------------------------------------------------
        public override string getGeneratorType()
        {
            return typeof(DivergencePointGeneratorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityGenerator buildGenerator(EntityGeneratorData generator_data,
            List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return new DivergencePointGenerator((DivergencePointGeneratorData)generator_data, server_param, route_object_mgr);
        }

        public override EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<DivergencePointGeneratorData>(json_item.mJsonString);
        }
    }

    public class DivergencePointGeneratorData : EntityGeneratorData
    {
        public int mFishVibID = 0;
        public float mGenerationPointX = 0;
        public float mGenerationPointY = 0;
        public float mRadius = 0;
        public int mAmount = 0;
        public float mStartTime = 0f;
        public float mSpeed = 0f;

        public override EntityGeneratorData clone()
        {
            DivergencePointGeneratorData generator_data = new DivergencePointGeneratorData();

            generator_data.mFishVibID = mFishVibID;
            generator_data.mGenerationPointX = mGenerationPointX;
            generator_data.mGenerationPointY = mGenerationPointY;
            generator_data.mRadius = mRadius;
            generator_data.mAmount = mAmount;
            generator_data.mStartTime = mStartTime;
            generator_data.mSpeed = mSpeed;

            return generator_data;
        }

        public override int getBaseEntityCount()
        {
            return mAmount;
        }

        public override string ToString()
        {
            return string.Format("mFishVibID {0},mGenerationPoint({1},{2}),mAmount{3},mStartTime{4}"
                , mFishVibID, mGenerationPointX, mGenerationPointY, mAmount, mStartTime);
        }
    }
}

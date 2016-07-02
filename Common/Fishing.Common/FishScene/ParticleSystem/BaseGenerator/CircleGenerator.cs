using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class CircleGenerator : EntityGenerator<CircleGeneratorData>
    {
        //---------------------------------------------------------------------
        public CircleGenerator(CircleGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
            : base(generator_data, server_param, route_object_mgr)
        { }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (Done) return;

            base.update(elapsed_tm);

            //线型点出鱼代码
            if (getGeneratorData().mStartTime < getPassedTime())
            {
                if (getGeneratorData().mAmount == 0) return;

                EbVector3 position = new EbVector3(getGeneratorData().mStartPointX, getGeneratorData().mStartPointY, 0);
                float delta_angle = 360f / getGeneratorData().mAmount;
                int fish_vib_id = -1;

                for (int i = getGeneratorData().mAmount; i > 0; i--)
                {
                    if (getGeneratorData().mRedFishIndex == i - 1)
                    {
                        fish_vib_id = getGeneratorData().mRedFishVibId;
                        if (fish_vib_id <= 0) fish_vib_id = getGeneratorData().mFishVibID;
                    }
                    else
                    {
                        fish_vib_id = getGeneratorData().mFishVibID;
                    }

                    BaseEntity entity = buildEntity(fish_vib_id);
                    entity.setPosition(position + new CDirection(90 + delta_angle * i) * getGeneratorData().mRadius);
                    entity.setDirection(getGeneratorData().mStartAngle);
                    entity.setSpeed(getGeneratorData().mSpeed);
                }

                setDone();
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();
        }
    }

    public class CircleGeneratorFactory : EntityGeneratorFactory
    {
        //---------------------------------------------------------------------
        public override string getGeneratorType()
        {
            return typeof(CircleGeneratorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityGenerator buildGenerator(EntityGeneratorData generator_data,
            List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return new CircleGenerator((CircleGeneratorData)generator_data, server_param, route_object_mgr);
        }

        public override EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<CircleGeneratorData>(json_item.mJsonString);
        }
    }

    public class CircleGeneratorData : EntityGeneratorData
    {
        public int mFishVibID = 0;
        public float mStartPointX = 0;
        public float mStartPointY = 0;
        public float mRadius = 0;
        public int mAmount = 0;
        public float mStartTime = 0f;
        public float mStartAngle = 0f;
        public float mSpeed = 0f;
        public int mRedFishVibId = -1;
        public int mRedFishIndex = -1;

        public override EntityGeneratorData clone()
        {
            CircleGeneratorData generator_data = new CircleGeneratorData();

            generator_data.mFishVibID = mFishVibID;
            generator_data.mStartPointX = mStartPointX;
            generator_data.mStartPointY = mStartPointY;
            generator_data.mAmount = mAmount;
            generator_data.mStartTime = mStartTime;
            generator_data.mRadius = mRadius;
            generator_data.mStartAngle = mStartAngle;
            generator_data.mSpeed = mSpeed;
            generator_data.mRedFishVibId = mRedFishVibId;
            generator_data.mRedFishIndex = mRedFishIndex;

            return generator_data;
        }

        public override int getBaseEntityCount()
        {
            return mAmount;
        }
    }
}

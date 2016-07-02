using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class FixGenerator : EntityGenerator<FixGeneratorData>
    {
        //---------------------------------------------------------------------
        public FixGenerator(FixGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
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
                EbVector3 position = new EbVector3(getGeneratorData().mStartPointX, getGeneratorData().mStartPointY, 0);

                BaseEntity entity = buildEntity(getGeneratorData().mFishVibID);
                entity.addRoute(RouteHelper.buildLineRoute(position, CLogicUtility.getDirection(getGeneratorData().mStartAngle), 1153.7f));
                entity.setSpeed(getGeneratorData().mSpeed);

                setDone();
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();
        }
    }

    public class FixGeneratorFactory : EntityGeneratorFactory
    {
        //---------------------------------------------------------------------
        public override string getGeneratorType()
        {
            return typeof(FixGeneratorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityGenerator buildGenerator(EntityGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return new FixGenerator((FixGeneratorData)generator_data, server_param, route_object_mgr);
        }

        public override EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<FixGeneratorData>(json_item.mJsonString);
        }
    }

    public class FixGeneratorData : EntityGeneratorData
    {
        public float mStartTime = 0f;
        public int mFishVibID = 0;
        public float mStartPointX = 0;
        public float mStartPointY = 0;
        public float mStartAngle = 0;
        public float mSpeed = 0;

        public override EntityGeneratorData clone()
        {
            FixGeneratorData generator_data = new FixGeneratorData();

            generator_data.mFishVibID = mFishVibID;
            generator_data.mStartTime = mStartTime;
            generator_data.mStartPointX = mStartPointX;
            generator_data.mStartPointY = mStartPointY;
            generator_data.mStartAngle = mStartAngle;
            generator_data.mSpeed = mSpeed;

            return generator_data;
        }

        public override int getBaseEntityCount()
        {
            return 1;
        }
    }
}

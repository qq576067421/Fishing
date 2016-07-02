using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class LineGenerator : EntityGenerator<LineGeneratorData>
    {
        //---------------------------------------------------------------------
        public LineGenerator(LineGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
            : base(generator_data, server_param, route_object_mgr)
        { }

        public override void update(float elapsed_tm)
        {
            if (Done) return;

            base.update(elapsed_tm);

            //线型点出鱼代码
            if (getGeneratorData().mStartTime < getPassedTime())
            {
                if (getGeneratorData().mAmount == 0) return;

                EbVector3 start_point = new EbVector3(getGeneratorData().mStartPointX, getGeneratorData().mStartPointY, 0);
                EbVector3 end_to_start = (new EbVector3(getGeneratorData().mEndPointX, getGeneratorData().mEndPointY, 0) - start_point);

                float angle = CLogicUtility.getRightAngle(CLogicUtility.getAngle(-end_to_start));
                EbVector3 delta_position = end_to_start / getGeneratorData().mAmount;
                for (int i = getGeneratorData().mAmount; i > 0; i--)
                {
                    BaseEntity entity = buildEntity(getGeneratorData().mFishVibID);
                    entity.setPosition(start_point + delta_position * i);
                    entity.setDirection(angle);
                }

                setDone();
            }
        }

        public override void destroy()
        {
            base.destroy();
        }
    }

    public class LineGeneratorFactory : EntityGeneratorFactory
    {
        //---------------------------------------------------------------------
        public override string getGeneratorType()
        {
            return typeof(LineGeneratorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityGenerator buildGenerator(EntityGeneratorData generator_data,
            List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return new LineGenerator((LineGeneratorData)generator_data, server_param, route_object_mgr);
        }

        public override EntityGeneratorData buildGeneratorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<LineGeneratorData>(json_item.mJsonString);
        }
    }

    public class LineGeneratorData : EntityGeneratorData
    {
        public int mFishVibID = 0;
        public float mStartPointX = 0;
        public float mStartPointY = 0;
        public float mEndPointX = 0;
        public float mEndPointY = 0;
        public int mAmount = 0;
        public float mStartTime = 0f;

        public override EntityGeneratorData clone()
        {
            LineGeneratorData generator_data = new LineGeneratorData();

            generator_data.mFishVibID = mFishVibID;
            generator_data.mStartPointX = mStartPointX;
            generator_data.mStartPointY = mStartPointY;
            generator_data.mEndPointX = mEndPointX;
            generator_data.mEndPointY = mEndPointY;
            generator_data.mAmount = mAmount;
            generator_data.mStartTime = mStartTime;

            return generator_data;
        }

        public override int getBaseEntityCount()
        {
            return mAmount;
        }
    }
}

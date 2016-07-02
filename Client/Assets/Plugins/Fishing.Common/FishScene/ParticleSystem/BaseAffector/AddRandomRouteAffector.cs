using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class AddRandomRouteAffector : EntityAffector<AddRandomRouteAffectorData>
    {
        //---------------------------------------------------------------------
        List<BaseEntity> mEntities = new List<BaseEntity>();
        static System.Random mRandom = new Random(unchecked((int)System.DateTime.Now.Ticks));

        //---------------------------------------------------------------------
        public AddRandomRouteAffector(AddRandomRouteAffectorData affector_data) : base(affector_data) { }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (Done) return;

            base.update(elapsed_tm);

            if (getAffectorData().mStartTime <= SecondsSinceCreation)
            {
                int random_route_vib_id = mRandom.Next(getAffectorData().mMinRouteVibId, getAffectorData().mMaxRouteVibId + 1);
                IRoute route = RouteHelper.buildLineRoute(random_route_vib_id);

                foreach (var fish in getBaseEntity())
                {
                    if (!mEntities.Contains(fish))
                    {
                        fish.addRoute(route);
                        mEntities.Add(fish);
                    }
                }

                setDone();
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
        }
    }

    public class AddRandomRouteAffectorFactory : EntityAffectorFactory
    {
        //---------------------------------------------------------------------
        public override string getAffectorType()
        {
            return typeof(AddRandomRouteAffectorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityAffector buildAffector(EntityAffectorData affector_data)
        {
            return new AddRandomRouteAffector((AddRandomRouteAffectorData)affector_data);
        }

        public override EntityAffectorData buildAffectorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<AddRandomRouteAffectorData>(json_item.mJsonString);
        }
    }

    public class AddRandomRouteAffectorData : EntityAffectorData
    {
        public float mStartTime = 0;
        public int mMinRouteVibId = 0;
        public int mMaxRouteVibId = 0;

        public override EntityAffectorData clone()
        {
            AddRandomRouteAffectorData affector_data = new AddRandomRouteAffectorData();

            affector_data.mStartTime = mStartTime;
            affector_data.mMinRouteVibId = mMinRouteVibId;
            affector_data.mMaxRouteVibId = mMaxRouteVibId;

            return affector_data;
        }
    }
}

using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class AddRouteAffector : EntityAffector<AddRouteAffectorData>
    {
        //---------------------------------------------------------------------
        List<BaseEntity> mEntities = new List<BaseEntity>();

        //---------------------------------------------------------------------
        public AddRouteAffector(AddRouteAffectorData affector_data) : base(affector_data) { }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (Done) return;

            base.update(elapsed_tm);

            if (getAffectorData().mStartTime <= SecondsSinceCreation)
            {
                IRoute route = RouteHelper.buildLineRoute(getAffectorData().mRouteVibId);

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

    public class AddRouteAffectorFactory : EntityAffectorFactory
    {
        //---------------------------------------------------------------------
        public override string getAffectorType()
        {
            return typeof(AddRouteAffectorData).ToString();
        }

        //---------------------------------------------------------------------
        public override EntityAffector buildAffector(EntityAffectorData affector_data)
        {
            return new AddRouteAffector((AddRouteAffectorData)affector_data);
        }

        public override EntityAffectorData buildAffectorData(JsonItem json_item)
        {
            return BaseJsonSerializer.deserialize<AddRouteAffectorData>(json_item.mJsonString);
        }
    }

    public class AddRouteAffectorData : EntityAffectorData
    {
        public float mStartTime = 0;
        public int mRouteVibId = 0;

        public override EntityAffectorData clone()
        {
            AddRouteAffectorData affector_data = new AddRouteAffectorData();

            affector_data.mStartTime = mStartTime;
            affector_data.mRouteVibId = mRouteVibId;

            return affector_data;
        }
    }
}

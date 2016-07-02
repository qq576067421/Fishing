using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public abstract class EntityGenerator
    {
        //---------------------------------------------------------------------
        public bool Done { get { return mDone; } }
        bool mDone = false;
        int mFishBeginId = 0;
        float mPassedTime = 0f;
        int mBaseGeneratorId = 0;
        EntityParticleSystem mEntityParticleSystem = null;
        List<string> mServerParam = null;
        RouteObjectMgr mRouteObjectMgr = null;

        //---------------------------------------------------------------------
        public EntityGenerator(List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            mServerParam = server_param;
            mRouteObjectMgr = route_object_mgr;
            if (mServerParam == null) mServerParam = new List<string>();
        }

        //---------------------------------------------------------------------
        public virtual void create()
        {
        }

        //---------------------------------------------------------------------
        public void setBaseGeneratorId(int generator_id)
        {
            mBaseGeneratorId = generator_id;
        }

        //---------------------------------------------------------------------
        public void setBaseFishLord(EntityParticleSystem fish_lord)
        {
            mEntityParticleSystem = fish_lord;
        }

        //---------------------------------------------------------------------
        public void setBeginFishId(int begin_id)
        {
            mFishBeginId = begin_id;
        }

        //---------------------------------------------------------------------
        protected int getNextFishId()
        {
            return mFishBeginId++;
        }

        //---------------------------------------------------------------------
        protected List<string> getServerParam()
        {
            return mServerParam;
        }

        //---------------------------------------------------------------------
        protected DynamicSystem getDynamicSystem(int id, float delay_time)
        {
            return mRouteObjectMgr.getDynamicSystem(id, delay_time);
        }

        //---------------------------------------------------------------------
        public virtual void update(float elapsed_tm)
        {
            if (mDone) return;
            mPassedTime += elapsed_tm;
        }

        //---------------------------------------------------------------------
        protected void setDone()
        {
            mDone = true;
        }

        //---------------------------------------------------------------------
        public virtual void destroy()
        {
        }

        //---------------------------------------------------------------------
        protected BaseEntity buildEntity(int fish_vib_id)
        {
            if (mEntityParticleSystem == null)
            {
                EbLog.Warning("BaseGenerator::There are no BaseEntityFactory.");
                return null;
            }

            return mEntityParticleSystem.buildEntity(mBaseGeneratorId, fish_vib_id, getNextFishId());
        }

        //---------------------------------------------------------------------
        protected float getPassedTime()
        {
            return mPassedTime;
        }
    }

    public abstract class EntityGenerator<T> : EntityGenerator where T : EntityGeneratorData
    {
        //---------------------------------------------------------------------
        T mGeneratorData = null;

        //---------------------------------------------------------------------
        public EntityGenerator(T generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
            : base(server_param, route_object_mgr)
        {
            mGeneratorData = generator_data;
        }

        //---------------------------------------------------------------------
        protected T getGeneratorData()
        {
            return mGeneratorData;
        }
    }

    public abstract class EntityGeneratorData
    {
        public abstract EntityGeneratorData clone();
        public abstract int getBaseEntityCount();
    }

    public abstract class EntityGeneratorFactory
    {
        public abstract string getGeneratorType();
        public abstract EntityGenerator buildGenerator(EntityGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr);
        public abstract EntityGeneratorData buildGeneratorData(JsonItem json_item);
    }
}

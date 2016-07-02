using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EntityParticleSystem
    {
        //---------------------------------------------------------------------
        public bool Done { get { return mDone; } }
        bool mDone = false;
        float mSurvivalTime = 0f;
        BaseFishLordMgr mFishLordMgr = null;
        BaseEntityFactory mBaseEntityFactory = null;
        GeneratorAndAffectorKeeper mGeneratorAndAffectorKeeper = null;
        ParticleSystemEntityKeeper mParticleSystemEntityKeeper = null;

        //---------------------------------------------------------------------
        public EntityParticleSystem create(BaseFishLordMgr fish_lord_mgr, BaseEntityFactory factory,
            EntityParticleSystemData fish_lord_data, int fish_begin_id, List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            mFishLordMgr = fish_lord_mgr;
            mBaseEntityFactory = factory;
            mSurvivalTime = fish_lord_data.mFishLordCommonData.mDestroyTime;

            mParticleSystemEntityKeeper = new ParticleSystemEntityKeeper();
            mGeneratorAndAffectorKeeper = new GeneratorAndAffectorKeeper(fish_lord_mgr, this, factory, fish_lord_data, fish_begin_id, server_param, route_object_mgr);

            return this;
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            mGeneratorAndAffectorKeeper.update(elapsed_tm);

            // 阵型的生存时间
            mSurvivalTime -= elapsed_tm;
            if (mSurvivalTime <= 0)
            {
                mDone = true;
            }
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mGeneratorAndAffectorKeeper.destroy();
            mParticleSystemEntityKeeper.destroy();
        }

        //---------------------------------------------------------------------
        public BaseEntity buildEntity(int generator_id, int fish_vib_id, int fish_id)
        {
            if (mBaseEntityFactory == null)
            {
                EbLog.Warning("BaseGenerator::There are no BaseEntityFactory.");
                return null;
            }

            BaseEntity entity = mBaseEntityFactory.buildBaseEntity(fish_vib_id, fish_id);
            addBaseEntity(generator_id, entity);

            return entity;
        }

        //---------------------------------------------------------------------
        void addBaseEntity(int generator_id, BaseEntity entity)
        {
            mParticleSystemEntityKeeper.addBaseEntity(generator_id, entity);
            mFishLordMgr.addBaseEntity(entity);
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getBaseEntity(int generator_id)
        {
            return mParticleSystemEntityKeeper.getBaseEntity(generator_id);
        }
    }
}

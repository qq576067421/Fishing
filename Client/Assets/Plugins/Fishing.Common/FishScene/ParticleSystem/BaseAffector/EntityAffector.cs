using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public abstract class EntityAffector
    {
        //---------------------------------------------------------------------
        public bool Done { get { return mDone; } }
        protected float SecondsSinceCreation { get { return mSecondsSinceCreation; } }
        bool mDone = false;
        float mSecondsSinceCreation = 0f;
        List<int> mListBaseGeneratorId = new List<int>();
        EntityParticleSystem mEntityParticleSystem = null;

        //---------------------------------------------------------------------
        public virtual void update(float elapsed_tm)
        {
            mSecondsSinceCreation += elapsed_tm;
        }

        //---------------------------------------------------------------------
        public abstract void destroy();

        //---------------------------------------------------------------------
        public void setGeneratorId(int generator_id)
        {
            mListBaseGeneratorId.Add(generator_id);
        }

        //---------------------------------------------------------------------
        public void setBaseFishLord(EntityParticleSystem fish_lord)
        {
            mEntityParticleSystem = fish_lord;
        }

        //---------------------------------------------------------------------
        protected void setDone()
        {
            mDone = true;
        }

        //---------------------------------------------------------------------
        protected List<BaseEntity> getBaseEntity()
        {
            List<BaseEntity> list_entity = new List<BaseEntity>();

            foreach (var it in mListBaseGeneratorId)
            {
                foreach (var entity in mEntityParticleSystem.getBaseEntity(it))
                {
                    list_entity.Add(entity);
                }
            }

            return list_entity;
        }
    }

    public abstract class EntityAffector<T> : EntityAffector where T : EntityAffectorData
    {
        //---------------------------------------------------------------------
        T mAffectorData = null;

        //---------------------------------------------------------------------
        public EntityAffector(T affector_data)
        {
            mAffectorData = affector_data;
        }

        //---------------------------------------------------------------------
        protected T getAffectorData()
        {
            return mAffectorData;
        }
    }

    public abstract class EntityAffectorData
    {
        public abstract EntityAffectorData clone();
    }

    public abstract class EntityAffectorFactory
    {
        public abstract string getAffectorType();
        public abstract EntityAffector buildAffector(EntityAffectorData affector_data);
        public abstract EntityAffectorData buildAffectorData(JsonItem json_item);
    }
}

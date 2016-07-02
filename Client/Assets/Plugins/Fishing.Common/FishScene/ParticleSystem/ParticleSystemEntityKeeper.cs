using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class ParticleSystemEntityKeeper
    {
        //---------------------------------------------------------------------
        Dictionary<int, List<BaseEntity>> mDicBaseEntity = new Dictionary<int, List<BaseEntity>>();// key=Generator Id

        //---------------------------------------------------------------------
        public List<BaseEntity> getBaseEntityListByAffectorId(int affector_id)
        {
            return null;
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getBaseEntity(int generator_id)
        {
            return packEntityList(mDicBaseEntity[generator_id]);
        }

        //---------------------------------------------------------------------
        List<BaseEntity> packEntityList(List<BaseEntity> entity_list)
        {
            List<BaseEntity> packed_entity_list = new List<BaseEntity>();

            foreach (var it in entity_list)
            {
                packed_entity_list.Add(it);
            }

            return packed_entity_list;
        }

        //---------------------------------------------------------------------
        public void addBaseEntity(int generator_id, BaseEntity entity)
        {
            if (!mDicBaseEntity.ContainsKey(generator_id))
            {
                mDicBaseEntity.Add(generator_id, new List<BaseEntity>());
            }

            mDicBaseEntity[generator_id].Add(entity);
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mDicBaseEntity.Clear();
        }
    }
}

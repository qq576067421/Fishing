using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class BaseEntityTable : IEnumerable<BaseEntity>
    {
        //---------------------------------------------------------------------
        Dictionary<int, List<BaseEntity>> mVibKeyBaseEntity = new Dictionary<int, List<BaseEntity>>();//key:fish_vib_id
        Dictionary<int, BaseEntity> mFishObjKeyBaseEntityObjId = new Dictionary<int, BaseEntity>();//key:fish_obj_id
        List<BaseEntity> mListBaseEntity = new List<BaseEntity>();
        Dictionary<TbDataOutFishGroup, List<BaseEntity>> mGroupKeyEntityGroup = new Dictionary<TbDataOutFishGroup, List<BaseEntity>>();//key:TbDataOutFishGroup

        //---------------------------------------------------------------------
        public void addEntity(BaseEntity entity)
        {
            if (!mVibKeyBaseEntity.ContainsKey(entity.FishVibId))
            {
                mVibKeyBaseEntity.Add(entity.FishVibId, new List<BaseEntity>());
            }
            mVibKeyBaseEntity[entity.FishVibId].Add(entity);
            mFishObjKeyBaseEntityObjId.Add(entity.FishObjId, entity);
            mListBaseEntity.Add(entity);

            TbDataOutFishGroup group_key = EbDataMgr.Instance.getData<TbDataFish>(entity.FishVibId).dataOutFishGroup;
            if (!mGroupKeyEntityGroup.ContainsKey(group_key))
            {
                mGroupKeyEntityGroup.Add(group_key, new List<BaseEntity>());
            }
            mGroupKeyEntityGroup[group_key].Add(entity);
        }

        //---------------------------------------------------------------------
        public void removeEntity(BaseEntity entity)
        {
            if (mVibKeyBaseEntity.ContainsKey(entity.FishVibId))
            {
                mVibKeyBaseEntity[entity.FishVibId].Remove(entity);
            }
            mFishObjKeyBaseEntityObjId.Remove(entity.FishObjId);
            mListBaseEntity.Remove(entity);

            TbDataOutFishGroup group_key = EbDataMgr.Instance.getData<TbDataFish>(entity.FishVibId).dataOutFishGroup;
            if (mGroupKeyEntityGroup.ContainsKey(group_key))
            {
                mGroupKeyEntityGroup[group_key].Remove(entity);
            }
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getAllEntity()
        {
            return mListBaseEntity;
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getBaseEntityListByVibId(int fish_vib_id)
        {
            if (!mVibKeyBaseEntity.ContainsKey(fish_vib_id)) return new List<BaseEntity>();
            return mVibKeyBaseEntity[fish_vib_id];
        }

        //---------------------------------------------------------------------
        public BaseEntity getBaseEntity(int obj_id)
        {
            BaseEntity be = null;
            mFishObjKeyBaseEntityObjId.TryGetValue(obj_id, out be);
            return be;
        }

        //---------------------------------------------------------------------
        public int getCountByGroup(TbDataOutFishGroup group)
        {
            if (mGroupKeyEntityGroup.ContainsKey(group))
            {
                return mGroupKeyEntityGroup[group].Count;
            }
            return 0;
        }

        //---------------------------------------------------------------------
        public int getAllEntityCount()
        {
            return mListBaseEntity.Count;
        }

        //---------------------------------------------------------------------
        public int getEntityCount(int fish_vib_id)
        {
            if (!mVibKeyBaseEntity.ContainsKey(fish_vib_id)) return 0;
            return mVibKeyBaseEntity[fish_vib_id].Count;
        }

        //-------------------------------------------------------------------------
        public void Clear()
        {
            mVibKeyBaseEntity.Clear();
            mFishObjKeyBaseEntityObjId.Clear();
            mListBaseEntity.Clear();
            foreach (var it in mGroupKeyEntityGroup)
            {
                it.Value.Clear();
            }
            mGroupKeyEntityGroup.Clear();
        }

        //-------------------------------------------------------------------------
        IEnumerator<BaseEntity> IEnumerable<BaseEntity>.GetEnumerator()
        {
            return mListBaseEntity.GetEnumerator();
        }

        //-------------------------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var entity in mFishObjKeyBaseEntityObjId)
            {
                yield return entity;
            }
        }
    }
}

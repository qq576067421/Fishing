using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class BaseEntityKeeper
    {
        //---------------------------------------------------------------------
        BaseEntityTable mBaseEntityTable = new BaseEntityTable();
        Queue<BaseEntity> mQueBaseEntityDestroy = new Queue<BaseEntity>();

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            foreach (var entity in mBaseEntityTable)
            {
                entity.update(elapsed_tm);

                if (entity.IsDestroy)
                {
                    mQueBaseEntityDestroy.Enqueue(entity);
                }
            }

            while (mQueBaseEntityDestroy.Count > 0)
            {
                BaseEntity be = mQueBaseEntityDestroy.Dequeue();
                mBaseEntityTable.removeEntity(be);
                be.destroy();
            }
        }

        //---------------------------------------------------------------------
        public BaseEntity getBaseEntity(int obj_id)
        {
            return mBaseEntityTable.getBaseEntity(obj_id);
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getEntityListById(int fish_vib_id)
        {
            return packEntityList(mBaseEntityTable.getBaseEntityListByVibId(fish_vib_id));
        }

        //---------------------------------------------------------------------
        public int getCountByGroup(TbDataOutFishGroup group)
        {
            return mBaseEntityTable.getCountByGroup(group);
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getAllEntity()
        {
            return packEntityList(mBaseEntityTable.getAllEntity());
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
        public void addBaseEntity(BaseEntity fish_entity)
        {
            mBaseEntityTable.addEntity(fish_entity);
        }

        //---------------------------------------------------------------------
        public int getEntityCount(int fish_vib_id)
        {
            return mBaseEntityTable.getEntityCount(fish_vib_id);
        }

        //---------------------------------------------------------------------
        public int getAllEntityCount()
        {
            return mBaseEntityTable.getAllEntityCount();
        }

        //---------------------------------------------------------------------
        public void clearAllBaseEntity()
        {
            destroy();
        }

        //---------------------------------------------------------------------
        public void clearBaseEntityByPosition(float x)
        {
            foreach (var entity in mBaseEntityTable)
            {
                if (entity.Position.x > x + 50)
                {
                    mQueBaseEntityDestroy.Enqueue(entity);
                }
            }

            while (mQueBaseEntityDestroy.Count > 0)
            {
                BaseEntity be = mQueBaseEntityDestroy.Dequeue();
                mBaseEntityTable.removeEntity(be);
                be.destroy();
            }
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            foreach (var entity in mBaseEntityTable)
            {
                entity.destroy();
            }
            mBaseEntityTable.Clear();

            while (mQueBaseEntityDestroy.Count > 0)
            {
                BaseEntity be = mQueBaseEntityDestroy.Dequeue();
                mBaseEntityTable.removeEntity(be);
                be.destroy();
            }
            mQueBaseEntityDestroy.Clear();
        }
    }
}

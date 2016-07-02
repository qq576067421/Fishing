using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class BaseFishLordMgr
    {
        //---------------------------------------------------------------------
        BaseEntityFactory mEntityFactory = null;
        ParticleSystemFactory mParticleSystemFactory = null;
        ParticleSystemDataPrototype mParticleSystemDataPrototype = null;
        ParticleSystemKeeper mParticleSystemKeeper = null;
        BaseEntityKeeper mBaseEntityKeeper = null;
        RouteObjectMgr mRouteObjectMgr = null;

        //---------------------------------------------------------------------
        public BaseFishLordMgr(BaseEntityFactory factory, ParticleSystemFactory fish_lord_factory)
        {
            mEntityFactory = factory;
            mParticleSystemFactory = fish_lord_factory;
        }

        //---------------------------------------------------------------------
        public void create(List<JsonPacket> json_packet_list, List<RouteJsonPacket> route_json_packet_list)
        {
            mParticleSystemDataPrototype = new ParticleSystemDataPrototype(json_packet_list, mParticleSystemFactory);
            mParticleSystemKeeper = new ParticleSystemKeeper();
            mBaseEntityKeeper = new BaseEntityKeeper();
            mRouteObjectMgr = new RouteObjectMgr(route_json_packet_list);
        }

        //---------------------------------------------------------------------
        public int getEntityCount(int fish_vib_id)
        {
            return mBaseEntityKeeper.getEntityCount(fish_vib_id);
        }

        //---------------------------------------------------------------------
        public int getAllEntityCount()
        {
            return mBaseEntityKeeper.getAllEntityCount();
        }

        //---------------------------------------------------------------------
        public void clearAllEntity()
        {
            mParticleSystemKeeper.clearAllParticleSystem();
            mBaseEntityKeeper.clearAllBaseEntity();
        }

        //---------------------------------------------------------------------
        public void clearBaseEntityByPosition(float x)
        {
            mBaseEntityKeeper.clearBaseEntityByPosition(x);
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            if (mEntityFactory != null)
            {
                mEntityFactory = null;
            }

            if (mParticleSystemDataPrototype != null)
            {
                mParticleSystemDataPrototype = null;
            }

            if (mRouteObjectMgr != null)
            {
                mRouteObjectMgr = null;
            }

            if (mParticleSystemFactory != null)
            {
                mParticleSystemFactory.destroy();
                mParticleSystemFactory = null;
            }

            if (mParticleSystemKeeper != null)
            {
                mParticleSystemKeeper.destroy();
                mParticleSystemKeeper = null;
            }

            if (mBaseEntityKeeper != null)
            {
                mBaseEntityKeeper.destroy();
                mBaseEntityKeeper = null;
            }
        }

        //---------------------------------------------------------------------
        public BaseEntity findBaseEntity(int fish_objid)
        {
            return mBaseEntityKeeper.getBaseEntity(fish_objid);
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getEntityListById(int fish_vib_id)
        {
            return mBaseEntityKeeper.getEntityListById(fish_vib_id);
        }

        //---------------------------------------------------------------------
        public int getCountByGroup(TbDataOutFishGroup group)
        {
            return mBaseEntityKeeper.getCountByGroup(group);
        }

        //---------------------------------------------------------------------
        public List<BaseEntity> getAllEntity()
        {
            return mBaseEntityKeeper.getAllEntity();
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            mParticleSystemKeeper.update(elapsed_tm);
            mBaseEntityKeeper.update(elapsed_tm);
        }

        //---------------------------------------------------------------------
        public BaseEntity getBaseEntity(int obj_id)
        {
            return mBaseEntityKeeper.getBaseEntity(obj_id);
        }

        //---------------------------------------------------------------------
        public int getParticleSystemCount()
        {
            return mParticleSystemKeeper.getParticleSystemCount();
        }

        //---------------------------------------------------------------------
        public void addBaseEntity(BaseEntity fish_entity)
        {
            mBaseEntityKeeper.addBaseEntity(fish_entity);
        }

        //---------------------------------------------------------------------
        public int getCountOfParticleSystemEntity(string file_name)
        {
            return mParticleSystemDataPrototype.getCountOfParticleSystemEntity(file_name);
        }

        //---------------------------------------------------------------------
        public void addParticleSystem(string file_name, List<string> server_param, int fish_begin_id)
        {
            EntityParticleSystemData lord_data = mParticleSystemDataPrototype.cloneParticleSystemData(file_name);
            if (lord_data == null)
            {
                EbLog.Warning("ParticleSystem::Add::Error::" + file_name);
                return;
            }

            mParticleSystemKeeper.addParticleSystem(
                    new EntityParticleSystem().create(this, mEntityFactory, lord_data, fish_begin_id, server_param, mRouteObjectMgr));
        }

        //---------------------------------------------------------------------
        public List<string> getPrototypeNameList()
        {
            return mParticleSystemDataPrototype.getPrototypeNameList();
        }

        //---------------------------------------------------------------------
        public EntityGenerator buildGenerator(EntityGeneratorData generator_data, List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            return mParticleSystemFactory.buildGenerator(generator_data, server_param, route_object_mgr);
        }

        //---------------------------------------------------------------------
        public EntityAffector buildAffector(EntityAffectorData affector_data)
        {
            return mParticleSystemFactory.buildAffector(affector_data);
        }
    }
}

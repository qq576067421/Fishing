using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class GeneratorAndAffectorKeeper
    {
        //---------------------------------------------------------------------
        public List<EntityGenerator> mListBaseGenerator = new List<EntityGenerator>();
        public List<EntityAffector> mListBaseAffector = new List<EntityAffector>();
        BaseFishLordMgr mFishLordMgr = null;
        Queue<EntityGenerator> mQueBaseGeneratorDestroy = new Queue<EntityGenerator>();

        //---------------------------------------------------------------------
        public GeneratorAndAffectorKeeper(
            BaseFishLordMgr fish_lord_mgr, EntityParticleSystem fish_lord, BaseEntityFactory factory,
            EntityParticleSystemData fish_lord_data, int fish_begin_id, List<string> server_param, RouteObjectMgr route_object_mgr)
        {
            mFishLordMgr = fish_lord_mgr;

            int fish_id = fish_begin_id;

            int generator_id = 0;
            foreach (var it in fish_lord_data.mListBaseGenerator)
            {
                EntityGenerator generator = mFishLordMgr.buildGenerator(it, server_param, route_object_mgr);
                generator.setBaseFishLord(fish_lord);
                generator.setBaseGeneratorId(generator_id);

                generator.setBeginFishId(fish_id);
                fish_id += it.getBaseEntityCount();

                if (generator == null) continue;
                mListBaseGenerator.Add(generator);

                generator_id++;
            }

            foreach (var it in fish_lord_data.mListBaseAffector)
            {
                EntityAffector affector = mFishLordMgr.buildAffector(it);
                if (affector == null) continue;
                affector.setBaseFishLord(fish_lord);
                mListBaseAffector.Add(affector);
            }

            foreach (var it in fish_lord_data.mGeneratorAffectorMap)
            {
                if (mListBaseAffector.Count > it.mAffectorIndex && mListBaseGenerator.Count > it.mGeneratorIndex)
                {
                    mListBaseAffector[it.mAffectorIndex].setGeneratorId(it.mGeneratorIndex);
                }
            }

            foreach (var it in mListBaseGenerator)
            {
                it.create();
            }
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            foreach (var it in mListBaseGenerator)
            {
                it.update(elapsed_tm);
                if (it.Done)
                {
                    mQueBaseGeneratorDestroy.Enqueue(it);
                }
            }

            while (mQueBaseGeneratorDestroy.Count > 0)
            {
                EntityGenerator eg = mQueBaseGeneratorDestroy.Dequeue();
                mListBaseGenerator.Remove(eg);
                eg.destroy();
            }

            foreach (var it in mListBaseAffector)
            {
                it.update(elapsed_tm);
            }
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mListBaseGenerator.Clear();
            mListBaseAffector.Clear();
        }
    }
}

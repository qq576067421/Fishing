using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class ParticleSystemKeeper
    {
        //---------------------------------------------------------------------
        List<EntityParticleSystem> mEntityParticleSystem = new List<EntityParticleSystem>();
        Queue<EntityParticleSystem> mQueEntityParticleSystemDestroy = new Queue<EntityParticleSystem>();

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            foreach (var it in mEntityParticleSystem)
            {
                it.update(elapsed_tm);
                if (it.Done)
                {
                    mQueEntityParticleSystemDestroy.Enqueue(it);
                }
            }

            while (mQueEntityParticleSystemDestroy.Count > 0)
            {
                EntityParticleSystem ps = mQueEntityParticleSystemDestroy.Dequeue();
                mEntityParticleSystem.Remove(ps);
                ps.destroy();
            }
        }

        //---------------------------------------------------------------------
        public void addParticleSystem(EntityParticleSystem fish_lord)
        {
            mEntityParticleSystem.Add(fish_lord);
        }

        //---------------------------------------------------------------------
        public void clearAllParticleSystem()
        {
            destroy();
        }

        //---------------------------------------------------------------------
        public int getParticleSystemCount()
        {
            return mEntityParticleSystem.Count;
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            foreach (var it in mEntityParticleSystem)
            {
                it.destroy();
            }
            mEntityParticleSystem.Clear();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;
using Ps;

public class ParticleManager
{
    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    Dictionary<string, List<StillParticle>> mDciParticle = new Dictionary<string,List<StillParticle>>();
#if UNITY_EDITOR
    GameObject mTKGameObject = null;
#endif
    //-------------------------------------------------------------------------
    public ParticleManager(CRenderScene scene)
    {
        mScene = scene;
#if UNITY_EDITOR
        mTKGameObject = GameObject.Find("TKParticle");
#endif
    }

    //-------------------------------------------------------------------------
    public StillParticle newParticle(string particle_prefab_name)
    {
        //Debug.LogWarning(particle_prefab_name);
        UnityEngine.Object particle_prefab_object = Resources.Load("Game/Particle/" + particle_prefab_name);

        if (particle_prefab_object == null)
        {
            Debug.LogError("ParticleManager::newParticle:: " + particle_prefab_name + " does not exist.");
        }

        GameObject particle_object = GameObject.Instantiate(particle_prefab_object) as GameObject;
        StillParticle still_particle = particle_object.GetComponent<StillParticle>();

        if (still_particle == null) {
            still_particle = particle_object.AddComponent<StillParticle>();
        }

        still_particle.init();

        if (!mDciParticle.ContainsKey(particle_prefab_name))
        {
            mDciParticle.Add(particle_prefab_name,new List<StillParticle>());
        }
        mDciParticle[particle_prefab_name].Add(still_particle);

#if UNITY_EDITOR
        still_particle.gameObject.name = "TkParticle_" + particle_prefab_name;
        still_particle.transform.parent = mTKGameObject.transform;
#endif

        return still_particle;
    }

    //-------------------------------------------------------------------------
    public void freeParticle(StillParticle still_particle)
    {
        if (still_particle == null) return;
        still_particle.destroy();
        foreach (var it in mDciParticle)
        {
            it.Value.Remove(still_particle);
        }
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        List<StillParticle> free_particle_list = new List<StillParticle>();
        foreach (var it in mDciParticle)
        {
            foreach (var particle in it.Value) {
                particle.update(elapsed_tm);
                if (particle.CanDestroy)
                {
                    free_particle_list.Add(particle);
                }
            }
        }

        foreach (var it in free_particle_list)
        {
            freeParticle(it);
        }
    }

    //-------------------------------------------------------------------------
    public void destroy() {
        foreach (var it in mDciParticle) {
            foreach (var particle in it.Value)
            {
                if (particle == null) continue;
                particle.destroy();
            }
        }
    }
}

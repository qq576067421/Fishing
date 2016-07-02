using System;
using System.Collections.Generic;
using UnityEngine;
using Ps;

public static class ParticleLoader
{
    public static GameObject Load(string particle_name, Vector3 init_pos)
    {
        GameObject particle = GameObject.Instantiate(Resources.Load("Game/Particle/" + particle_name), init_pos, Quaternion.identity) as GameObject;

#if UNITY_EDITOR
        particle.name = "TkParticle_" + particle_name;
        particle.transform.parent = GameObject.Find("TKParticle").transform;
#endif
        return particle;
    }
}
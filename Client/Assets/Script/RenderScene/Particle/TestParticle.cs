using UnityEngine;
using System.Collections;
using Ps;

public class TestParticle : MonoBehaviour
{
    ParticleSystem mParticleSystem = null;
    // Use this for initialization
    void Start()
    {
        mParticleSystem = GetComponent<ParticleSystem>();
        Debug.LogWarning(mParticleSystem.duration);
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}

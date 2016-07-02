using UnityEngine;
using System.Collections.Generic;
using Ps;

public class ParticleCoins : BaseParticle
{
    protected Vector3 mInitPosition;
    protected Vector3 mTargetPosition;
    protected int mCoinsCount;
    protected float mLayer;

    protected override void create()
    {
        mInitPosition = getParameter<Vector3>("init_position");
        mTargetPosition = getParameter<Vector3>("target_position");
        mCoinsCount = getParameter<int>("coins_count");
        mLayer = getParameter<float>("layer");
    }

    public override void destroy()
    {
    }
}
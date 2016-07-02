using System;
using System.Collections.Generic;
using UnityEngine;
using Ps;

public class NullParticle : BaseParticle
{
    protected override void create()
    {
        SignFinished();
    }

    public override void destroy()
    {
    }
}

public abstract class BaseParticle : MonoBehaviour
{
    public bool IsFinished { get { return mIsFinished; } }
    bool mIsFinished = false;

    Dictionary<string, object> mParameters = new Dictionary<string, object>();

    public void init(Dictionary<string,object> parameters)
    {
        foreach (var p in parameters)
        {
            mParameters.Add(p.Key, p.Value);
        }

        create();
    }

    protected T getParameter<T>(string key)
    {
        if (mParameters.ContainsKey(key))
        {
            return (T)mParameters[key];
        }
        throw new Exception(GetType().ToString() + "::getParameter::there are no key:" + key);
    }

    protected abstract void create();
    public abstract void destroy();

    protected void SignFinished()
    {
        mIsFinished = true;
    }
}
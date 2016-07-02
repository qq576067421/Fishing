using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GF.Common;
using Ps;

public sealed class FishStillSprite : StillSprite
{
    //-------------------------------------------------------------------------
    ISpriteFish mISpriteFish = null;

    //-------------------------------------------------------------------------
    public void init(ISpriteFish sprite_fish, CRenderScene scene)
    {
        base.init(scene);
        mISpriteFish = sprite_fish;
    }

    //-------------------------------------------------------------------------
    public void setISpriteFishNull() {
        mISpriteFish = null;
    }

    //-------------------------------------------------------------------------
    public void detachChildren() {
        transform.DetachChildren();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        setISpriteFishNull();
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    //-------------------------------------------------------------------------
    public ISpriteFish getSpriteFish()
    {
        return mISpriteFish;
    }
}
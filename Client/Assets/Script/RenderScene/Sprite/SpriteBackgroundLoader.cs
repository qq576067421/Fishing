using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteBackgroundLoader
{
    //-------------------------------------------------------------------------
    StillSpriteLoader<StillSprite> mStillSpriteLoader = new StillSpriteLoader<StillSprite>();

    //-------------------------------------------------------------------------
    public StillSprite newBackgroundStillSprite(string prefab_name, CRenderScene scene)
    {
        return mStillSpriteLoader.loadSpriteFromPrefab("Game/Background/" + prefab_name + "Prefab", scene);
    }

    //-------------------------------------------------------------------------
    public void freeBackgroundStillSprite(StillSprite still_sprite)
    {
        if (still_sprite == null) return;
        //Resources.UnloadAsset(still_sprite.renderer.material.mainTexture);
        still_sprite.destroy();

        //Resources.UnloadUnusedAssets();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mStillSpriteLoader.destroy();
    }
}
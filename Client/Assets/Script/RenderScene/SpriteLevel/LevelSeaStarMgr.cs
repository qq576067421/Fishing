using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using UnityEngine;
using Ps;

public class LevelSeaStarMgr
{
    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    List<StillParticle> mSeaStar = new List<StillParticle>();

    //-------------------------------------------------------------------------
    public LevelSeaStarMgr(CRenderScene render_scene)
    {
        mScene = render_scene;
    }

    //-------------------------------------------------------------------------
    public void switchBackground(int map_id)
    {
        _destroySeaStar();
        _loadSeaStar(map_id);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        _destroySeaStar();
    }

    //-------------------------------------------------------------------------
    void _destroySeaStar()
    {
        foreach (var it in mSeaStar)
        {
            mScene.getParticlemanager().freeParticle(it);
        }
        mSeaStar.Clear();
    }

    //-------------------------------------------------------------------------
    void _loadSeaStar(int map_id)
    {
        TbDataMap data = EbDataMgr.Instance.getData<TbDataMap>(map_id);
        foreach (var it in data.SeaStarParticle)
        {
            try
            {
                if (it.TbDataParticle == null || it.TbDataParticle.Id == 0) continue;

                StillParticle still_particle = mScene.getParticlemanager().newParticle(it.TbDataParticle.ParticlePrefabName);
                still_particle.setPosition(new EbVector3(it.PositionX, it.PositionY, 0));
                still_particle.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.StarFish));

                mSeaStar.Add(still_particle);

#if UNITY_EDITOR
                still_particle.gameObject.name = "TkSpriteSeaStar_" + it.TbDataParticle.ParticlePrefabName;
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
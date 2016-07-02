using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;
using Ps;

public class CSpriteBullet
{
    //-------------------------------------------------------------------------
    CRenderBullet mBullet = null;
    StillSprite mStillSprite = null;
    CRenderScene mScene = null;
    float mBoxY = 1f;
    TbDataParticle mHitParticle = null;
    bool mIsSignDestroy = false;

    //-------------------------------------------------------------------------
    public void create(CRenderScene scene, CRenderBullet render_bullet, string animation_name, TbDataParticle hit_particle)
    {
        mScene = scene;
        mBullet = render_bullet;
        mHitParticle = hit_particle;

        mStillSprite = mScene.getRenderObjectPool().newStillSprite();
        mStillSprite.setTrigger(true);
        mStillSprite.playAnimation(animation_name);
        mStillSprite.onTriggerEnter += OnTriggerEnter;

#if UNITY_EDITOR
        mStillSprite.gameObject.name = "TkSpriteBullet_" + animation_name;
#endif

        mBoxY = mStillSprite.getBoxColliderSize().y;
    }

    //-------------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (mIsSignDestroy) return;

        if ("CSpriteFish" != other.gameObject.tag) return;

        if (mBullet.getLockFish() == null)
        {
            FishStillSprite sprite_fish = other.gameObject.GetComponent<FishStillSprite>();
            if (sprite_fish != null)
            {
                _hitFish(sprite_fish.getSpriteFish().getRenderFish());
            }
        }
        else
        {
            FishStillSprite sprite_fish = other.gameObject.GetComponent<FishStillSprite>();
            if (sprite_fish != null && sprite_fish.getSpriteFish().getRenderFish().FishObjId == mBullet.getLockFish().FishObjId)
            {
                _hitFish(sprite_fish.getSpriteFish().getRenderFish());
            }
        }
    }

    //-------------------------------------------------------------------------
    void _hitFish(CRenderFish render_fish)
    {
        if (render_fish == null || render_fish.IsDie) return;

        uint et_player_rpcid = mBullet.getPlayerId();
        if (mBullet.getScene().getMyPlayerId() == et_player_rpcid)
        {
            mBullet.getScene().getProtocol().c2sFishHit(et_player_rpcid, mBullet.getBulletObjId(), render_fish.FishObjId);
        }

        mBullet.signDestroy();
        mIsSignDestroy = true;

        CRenderTurret turret = mBullet.getScene().getTurret(et_player_rpcid);
        if (turret == null) return;
        TbDataTurret vib_turret = turret.getVibTurret();

        Dictionary<string, object> map_param = new Dictionary<string, object>();
        map_param.Add("SourcePosition", getHitPosition(render_fish.Position, mBullet.getPosition()));
        map_param["NetColor"] = mBullet.getScene().getTurret(et_player_rpcid).getTurretColor();
        map_param["PlayerId"] = et_player_rpcid;
        map_param["BulletHitFish"] = render_fish;
        TbDataBullet buttlt_data = mBullet.getBulletData();

        mBullet.getScene().addEffect(buttlt_data.EffectCompose.Id, map_param, EffectTypeEnum.Client);

        if (!string.IsNullOrEmpty(mHitParticle.ParticlePrefabName))
        {
            StillParticle particle = mScene.getParticlemanager().newParticle(mHitParticle.ParticlePrefabName);
            particle.setPosition(getHitPosition(render_fish.Position, mBullet.getPosition()));
            particle.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.FishHitParticle));
        }
    }

    //-------------------------------------------------------------------------
    float getHitAngle(EbVector3 fish_pos, EbVector3 bullet_pos)
    {
        return CLogicUtility.getAngle(fish_pos - bullet_pos);
    }

    //-------------------------------------------------------------------------
    EbVector3 getHitPosition(EbVector3 fish_pos, EbVector3 bullet_pos)
    {
        float distance = fish_pos.getDistance(bullet_pos);
        if (distance > 0)
        {
            return EbVector3.lerp(bullet_pos, fish_pos, mBoxY / distance);
        }
        return fish_pos;
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mStillSprite.onTriggerEnter -= OnTriggerEnter;
        mScene.getRenderObjectPool().freeStillSprite(mStillSprite);
    }

    //-------------------------------------------------------------------------
    public void setScale(float scale)
    {
        mStillSprite.setScale(scale);
    }

    //-------------------------------------------------------------------------
    public void setLayer(float layer)
    {
        mStillSprite.setLayer(layer);
    }

    //-------------------------------------------------------------------------
    public void setPosition(EbVector3 position)
    {
        mStillSprite.setPosition(position);
    }

    //-------------------------------------------------------------------------
    public void setDirection(float angle)
    {
        mStillSprite.setDirection(angle);
    }

    //-------------------------------------------------------------------------
    public void playAnimation(string name)
    {
        mStillSprite.playAnimation(name);
    }

    //-------------------------------------------------------------------------
    public void stopAnimation()
    {
        mStillSprite.stopAnimation();
    }

    //-------------------------------------------------------------------------
    public void setTrigger(bool isTrigger, float size = 1)
    {
        mStillSprite.setTrigger(isTrigger, size);
    }

    //-------------------------------------------------------------------------
    public void setColor(UnityEngine.Color color)
    {
        mStillSprite.setColor(color);
    }

    //-------------------------------------------------------------------------
    public void setAlpha(float alpha)
    {
        mStillSprite.setAlpha(alpha);
    }

    //-------------------------------------------------------------------------
    public void setTag(string tag)
    {
        mStillSprite.setTag(tag);
    }
}
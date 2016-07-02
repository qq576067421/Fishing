using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteTurretFort
{
    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    CRenderTurret mRenderTurret = null;
    StillSprite mTurretBaseSprite = null;
    StillSprite mTurretTopSprite = null;
    StillSprite mTurretBlazeSprite = null;
    StillSprite mTurretBarrelSprite = null;
    bool mIsFireAnimation = false;
    MassEntity mMassEntity = null;

    string mTkMaterialName = "Game/atlas0 material";
    string mGlowMaterialName = "Game/Glow";
    UnityEngine.Material mTkMaterial;
    UnityEngine.Material mGlowMaterial;

    bool mIsGlow = false;
    float mRestGlowSeconds = 0;

    //-------------------------------------------------------------------------
    public void create(CRenderScene scene, CRenderTurret render_turret)
    {
        mScene = scene;
        mRenderTurret = render_turret;
        mRenderTurret.getTurretId();

        _initTurretBase();
        _initTurretTop();
        _initTurretBarrel();
        _initTurretBlaze();

        _setTrigger(true);
        mTkMaterial = GameObject.Instantiate(Resources.Load(mTkMaterialName)) as UnityEngine.Material;
        mGlowMaterial = GameObject.Instantiate(Resources.Load(mGlowMaterialName)) as UnityEngine.Material;

#if UNITY_EDITOR
        mTurretBaseSprite.gameObject.name = "TkSpriteTurret";
        mTurretTopSprite.gameObject.name = "TkSpriteTurret";
        mTurretBlazeSprite.gameObject.name = "TkSpriteTurret";
        mTurretBarrelSprite.gameObject.name = "TkSpriteTurret";
#endif

        if (mRenderTurret.isMyTurret())
        {
            startGlow(5f);
        }
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        _updateFireAnimation(elapsed_tm);
        mTurretBarrelSprite.setDirection(mRenderTurret.getTurretAngle());

        updateGlow(elapsed_tm);
    }

    //-------------------------------------------------------------------------
    void updateGlow(float elapsed_tm)
    {
        if (mIsGlow)
        {
            mRestGlowSeconds -= elapsed_tm;
            if (mRestGlowSeconds < 0)
            {
                mRestGlowSeconds = 1f;
                mIsGlow = false;
                endGlow();
            }
        }
    }

    //-------------------------------------------------------------------------
    public void startGlow(float duration)
    {
        mIsGlow = true;
        mRestGlowSeconds = duration;

        mTurretBaseSprite.GetComponent<Renderer>().material = mGlowMaterial;
        mTurretTopSprite.GetComponent<Renderer>().material = mGlowMaterial;
        mTurretBlazeSprite.GetComponent<Renderer>().material = mGlowMaterial;
        mTurretBarrelSprite.GetComponent<Renderer>().material = mGlowMaterial;

        mGlowMaterial.SetFloat("_GlowAlpha", 10);
    }

    //-------------------------------------------------------------------------
    void endGlow()
    {
        mTurretBaseSprite.GetComponent<Renderer>().material = mTkMaterial;
        mTurretTopSprite.GetComponent<Renderer>().material = mTkMaterial;
        mTurretBlazeSprite.GetComponent<Renderer>().material = mTkMaterial;
        mTurretBarrelSprite.GetComponent<Renderer>().material = mTkMaterial;
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        endGlow();

        mScene.getRenderObjectPool().freeStillSprite(mTurretBaseSprite);
        mScene.getRenderObjectPool().freeStillSprite(mTurretTopSprite);
        mScene.getRenderObjectPool().freeStillSprite(mTurretBlazeSprite);
        mScene.getRenderObjectPool().freeStillSprite(mTurretBarrelSprite);

        mTurretBaseSprite = null;
        mTurretTopSprite = null;
        mTurretBlazeSprite = null;
        mTurretBarrelSprite = null;
    }

    //-------------------------------------------------------------------------
    public void reloadAnimation()
    {
        mTurretBarrelSprite.playAnimation(mRenderTurret.getVibTurret().TurretBarrelAnimationName);
        mTurretBaseSprite.playAnimation(mRenderTurret.getVibTurret().TurretBaseAnimationName);
        mTurretTopSprite.playAnimation(mRenderTurret.getVibTurret().TurretTopCoverAnimationName);
        mTurretBlazeSprite.playAnimation(mRenderTurret.getVibTurret().TurretFireBlazeAnimationName);
    }

    //-------------------------------------------------------------------------
    public void setBarrelColor(UnityEngine.Color color)
    {
        mTurretBarrelSprite.setColor(color);
    }

    //-------------------------------------------------------------------------
    void _updateFireAnimation(float elapsed_tm)
    {
        if (mIsFireAnimation)
        {
            mMassEntity.update(elapsed_tm);
            mTurretBarrelSprite.setPosition(mMassEntity.Position);
            mTurretBarrelSprite.setDirection(mMassEntity.Angle);

            if (!mMassEntity.IsEndRoute) return;

            mIsFireAnimation = false;
            mTurretBlazeSprite.setActive(false);
        }
    }

    //-------------------------------------------------------------------------
    public void fireAt(float target_direction)
    {
        mTurretBarrelSprite.setDirection(target_direction);
        _playFireAnimation();
    }

    //-------------------------------------------------------------------------
    public void aimAt(float target_direction)
    {
        mTurretBarrelSprite.setDirection(target_direction);
    }

    //-------------------------------------------------------------------------
    void _initTurretBase()
    {
        float scale = (float)mRenderTurret.getVibTurret().TurretBaseHeight /
            (float)mRenderTurret.getVibTurret().TurretBasePixelHeight;

        _initTurretUnit(ref mTurretBaseSprite, mRenderTurret.getVibTurret().TurretBaseAnimationName, scale, _eLevelLayer.TurretBase);
    }

    //-------------------------------------------------------------------------
    void _initTurretTop()
    {
        float scale = (float)mRenderTurret.getVibTurret().TurretTopCoverHeight /
            (float)mRenderTurret.getVibTurret().TurretTopCoverPixelHeight;

        _initTurretUnit(ref mTurretTopSprite, mRenderTurret.getVibTurret().TurretTopCoverAnimationName, scale, _eLevelLayer.TurretTopCover);
    }

    //-------------------------------------------------------------------------
    void _initTurretBarrel()
    {
        float scale = (float)mRenderTurret.getVibTurret().TurretBarrelHeight /
            (float)mRenderTurret.getVibTurret().TurretBarrelPixelHeight;

        _initTurretUnit(ref mTurretBarrelSprite, mRenderTurret.getVibTurret().TurretBarrelAnimationName, scale, _eLevelLayer.TurretBarrel);
    }

    //-------------------------------------------------------------------------
    void _initTurretBlaze()
    {
        float scale = (float)mRenderTurret.getVibTurret().TurretFireBlazeHeight /
            (float)mRenderTurret.getVibTurret().TurretFireBlazeixelHeight;

        _initTurretUnit(ref mTurretBlazeSprite, mRenderTurret.getVibTurret().TurretFireBlazeAnimationName, scale, _eLevelLayer.TurretBlaze);
        mTurretBlazeSprite.setActive(false);
    }

    //-------------------------------------------------------------------------
    void _initTurretUnit(ref StillSprite still_sprite, string animation_name, float scale, _eLevelLayer layer)
    {
        still_sprite = mScene.getRenderObjectPool().newStillSprite();
        still_sprite.playAnimation(animation_name);
        still_sprite.setScale(scale);
        still_sprite.setLayer(mScene.getLayerAlloter().getLayer(layer));
        still_sprite.setPosition(mRenderTurret.getTurretPos());
        still_sprite.setDirection(mScene.getTurretHelper().getBaseAngleByTurretId(mRenderTurret.getTurretId()));
        still_sprite.setTag("CSpriteTurret" + mRenderTurret.getTurretId());
    }

    //-------------------------------------------------------------------------
    void _setTrigger(bool isTrigger)
    {
        mTurretBaseSprite.setTrigger(isTrigger);
        mTurretTopSprite.setTrigger(isTrigger);
        //mTurretBlazeSprite.setTrigger(isTrigger);
        mTurretBarrelSprite.setTrigger(isTrigger);
    }

    //-------------------------------------------------------------------------
    void _playFireAnimation()
    {
        mMassEntity = new MassEntity();
        RouteParaCurve route = new RouteParaCurve();
        route.create(mRenderTurret.getTurretPos(), mRenderTurret.getTurretAngle(), 0.1f, -13f);
        mMassEntity.setRoute(route);
        mIsFireAnimation = true;
        mTurretBlazeSprite.setActive(true);
        mTurretBlazeSprite.setPosition(mRenderTurret.getTurretPos());
        mTurretBlazeSprite.setDirection(mRenderTurret.getTurretAngle());
    }
}

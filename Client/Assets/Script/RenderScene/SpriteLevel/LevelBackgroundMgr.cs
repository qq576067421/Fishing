using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using UnityEngine;
using Ps;

public class LevelBackgroundMgr
{
    //-------------------------------------------------------------------------
    public delegate void onLoadMapEndDelegate();
    public onLoadMapEndDelegate onLoadMapEnd;

    CRenderScene mScene = null;
    CSpriteBackgroundLoader mBackgroundLoader = null;
    MassEntity mMassEntity = null;
    StillSprite mWaveStillSprite = null;
    BgSpriteMgr mBgMgr = null;
    BgSpriteMgr mPreBgMgr = null;
    float mMaxTime = 3f;
    float mSpeed = 300f;
    float mAlreadyUpdateTime = 0;
    bool mIsSwitching = false;
    bool mHasEarlyClean = false;

    //-------------------------------------------------------------------------
    public LevelBackgroundMgr(CRenderScene scene, CSpriteBackgroundLoader loader)
    {

        mScene = scene;
        mBackgroundLoader = loader;

        mSpeed = 960f / mMaxTime;

        mWaveStillSprite = mScene.getRenderObjectPool().newStillSprite();
        mWaveStillSprite.playAnimation("WaterWave");
        mWaveStillSprite.setActive(false);
        mWaveStillSprite.setScale(700f / 550f);

#if UNITY_EDITOR
        mWaveStillSprite.gameObject.name = "TkSpriteWaterWave";
#endif
    }

    //-------------------------------------------------------------------------
    public void switchBackground(string prefab_name, float already_update_time)
    {
        if (mBgMgr == null)
        {
            _loadBgSprite(prefab_name);
            if (onLoadMapEnd != null) onLoadMapEnd();
        }
        else
        {
            if (mIsSwitching) {
                if (!mHasEarlyClean)
                {
                    mScene.getLevel().clearAllFish();
                }
                switchBackgroundPic();
                mWaveStillSprite.setActive(false);
                mIsSwitching = false;
            }
            mAlreadyUpdateTime = already_update_time;
            mBgMgr.mBgClippedSprite.ClipRect = new Rect(1, 0, 1, 1);
            _loadFgSprite(prefab_name);

            mMassEntity = new MassEntity();
            mMassEntity.setSpeed(mSpeed);
            mMassEntity.setRoute(RouteHelper.buildLineRoute(new EbVector3(650, 0, 0), CLogicUtility.getDirection(-90), 2000));
            mWaveStillSprite.setActive(true);
            mWaveStillSprite.setDirection(-90);
            mWaveStillSprite.setPosition(new EbVector3(650, 0, 0));
            mWaveStillSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Ripple));

            mIsSwitching = true;
            mHasEarlyClean = false;
        }
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (!mIsSwitching) return;
        mAlreadyUpdateTime += elapsed_tm;
        float t = Mathf.Lerp(0, -1, mSpeed / CCoordinate.LogicSceneLength * mAlreadyUpdateTime);

        mMassEntity.update(elapsed_tm);
        mWaveStillSprite.setPosition(mMassEntity.Position);

        //if (!mHasEarlyClean)
        //{
        //    _setClipRect(t);
        //}

        _setClipRect(t);

        if (!mHasEarlyClean)
        {
            mScene.getLevel().clearBaseEntityByPosition(960 * t + 480);
        }

        if (t == -1)
        {
            if (!mHasEarlyClean)
            {
                mScene.getLevel().clearAllFish();
            }
            switchBackgroundPic();
            mWaveStillSprite.setActive(false);
            if (onLoadMapEnd != null) onLoadMapEnd();
            mIsSwitching = false;
            mHasEarlyClean = false;
        }
    }

    //-------------------------------------------------------------------------
    void _setClipRect(float t)
    {
        mBgMgr.mBgClippedSprite.ClipRect = new Rect(t, 0, 1, 1);
        mPreBgMgr.mBgClippedSprite.ClipRect = new Rect(t + 1, 0, 1, 1);
    }

    //-------------------------------------------------------------------------
    void switchBackgroundPic()
    {
        mBgMgr.destroy();
        mBgMgr = mPreBgMgr;
        mPreBgMgr = null;
        mBgMgr.mBgSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Background));
    }

    //-------------------------------------------------------------------------
    public void switchBackgroundEarly()
    {
        if (!mIsSwitching) return;
        mScene.getLevel().clearAllFish();
        //_setClipRect(-1);
        //switchBackgroundPic();

        if (mPreBgMgr != null)
        {
            mPreBgMgr.mBgSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Background));
        }
        mBgMgr.mBgSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.PreBackground));

        mHasEarlyClean = true;
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mScene.getRenderObjectPool().freeStillSprite(mWaveStillSprite);
        if (mBgMgr != null) mBgMgr.destroy();
        if (mPreBgMgr != null) mPreBgMgr.destroy();
    }

    //-------------------------------------------------------------------------
    void _loadFgSprite(string prefab_name)
    {
        mPreBgMgr = new BgSpriteMgr(mScene, mBackgroundLoader);
        mPreBgMgr.load(prefab_name, _eLevelLayer.PreBackground);
        mIsSwitching = true;
    }

    //-------------------------------------------------------------------------
    void _loadBgSprite(string prefab_name)
    {
        mBgMgr = new BgSpriteMgr(mScene, mBackgroundLoader);
        mBgMgr.load(prefab_name, _eLevelLayer.Background);
    }

    class BgSpriteMgr
    {
        //-------------------------------------------------------------------------
        CRenderScene mScene = null;
        public StillSprite mBgSprite = null;
        public tk2dClippedSprite mBgClippedSprite = null;
        CSpriteBackgroundLoader mBackgroundLoader = null;

        //-------------------------------------------------------------------------
        public BgSpriteMgr(CRenderScene scene, CSpriteBackgroundLoader loader)
        {
            mScene = scene;
            mBackgroundLoader = loader;
        }

        //-------------------------------------------------------------------------
        public void load(string prefab_name, _eLevelLayer layer)
        {
            mBgSprite = mBackgroundLoader.newBackgroundStillSprite(prefab_name, mScene);
            mBgSprite.setPosition(EbVector3.Zero);
            mBgSprite.setDirection(0);
            mBgSprite.setLayer(mScene.getLayerAlloter().getLayer(layer));
            mBgClippedSprite = mBgSprite.gameObject.GetComponent<tk2dClippedSprite>();

#if UNITY_EDITOR
            mBgSprite.gameObject.name = "TkBackground";
#endif
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            mBackgroundLoader.freeBackgroundStillSprite(mBgSprite);
            mBgSprite = null;
            mBgClippedSprite = null;
        }
    }
}
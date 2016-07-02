using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using UnityEngine;
using Ps;

public class LevelScreenShocker
{
    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    CSpriteBackgroundLoader mBackgroundLoader = null;
    StillSprite mShockBackgroundStillSprite = null;
    bool mIsShockingScreen = false;

    float mFirstFrameTime = 0.1f;
    float mSecondFrameTime = 0.14f;
    float mThirdFrameTime = 0.15f;
    float mSpeedScale = 0.8f;

    float mFirstFrameTimeDistance = -30f;
    float mSecondFrameTimeDistance = -45f;

    float mSecondsSinceSwitchBackground = 0;
    float mShakeAngle = -30;

    //-------------------------------------------------------------------------
    public LevelScreenShocker(CRenderScene render_scene, CSpriteBackgroundLoader loader)
    {
        mScene = render_scene;
        mBackgroundLoader = loader;
    }

    //-------------------------------------------------------------------------
    public void switchBackground(int map_id)
    {
        _freeShockBackground();
        _newShockBackground(EbDataMgr.Instance.getData<TbDataMap>(map_id).MapName);
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (!mIsShockingScreen) return;
        mSecondsSinceSwitchBackground += elapsed_tm;

        EbVector3 background_new_position = EbVector3.Zero;

        if (_isInFirstFrame())
        {
            background_new_position = CLogicUtility.getDirection(mShakeAngle) * mFirstFrameTimeDistance;
        }
        else if (_isInSecondFrame())
        {
            background_new_position = CLogicUtility.getDirection(mShakeAngle) * mSecondFrameTimeDistance;
        }
        else if (_isInInitFrame())
        {
            mSecondsSinceSwitchBackground = 0;
        }

        mShockBackgroundStillSprite.setPosition(background_new_position);
    }

    //-------------------------------------------------------------------------
    public void shockScreen()
    {
        if (mShockBackgroundStillSprite == null) return;
        mShockBackgroundStillSprite.setActive(true);
        mSecondsSinceSwitchBackground = 0;
        mIsShockingScreen = true;
    }

    //-------------------------------------------------------------------------
    public void stopShockScreen()
    {
        if (mShockBackgroundStillSprite == null) return;
        mIsShockingScreen = false;
        mShockBackgroundStillSprite.setActive(false);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        _freeShockBackground();
    }

    //-------------------------------------------------------------------------
    void _freeShockBackground()
    {
        if (mShockBackgroundStillSprite == null) return;
        mBackgroundLoader.freeBackgroundStillSprite(mShockBackgroundStillSprite);
        mShockBackgroundStillSprite = null;
    }

    //-------------------------------------------------------------------------
    void _newShockBackground(string background_name)
    {
        mShockBackgroundStillSprite = mBackgroundLoader.newBackgroundStillSprite(background_name, mScene);
        mShockBackgroundStillSprite.setPosition(EbVector3.Zero);
        mShockBackgroundStillSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.ShockBackground));
        mShockBackgroundStillSprite.setActive(false);

#if UNITY_EDITOR
        mShockBackgroundStillSprite.gameObject.name = "TkBackground";
#endif
    }

    //-------------------------------------------------------------------------
    bool _isInInitFrame()
    {
        return mSecondsSinceSwitchBackground >= mThirdFrameTime * mSpeedScale;
    }

    //-------------------------------------------------------------------------
    bool _isInFirstFrame()
    {
        return mSecondsSinceSwitchBackground < mSecondFrameTime * mSpeedScale;
    }

    //-------------------------------------------------------------------------
    bool _isInSecondFrame()
    {
        return mSecondsSinceSwitchBackground < mThirdFrameTime * mSpeedScale;
    }
}
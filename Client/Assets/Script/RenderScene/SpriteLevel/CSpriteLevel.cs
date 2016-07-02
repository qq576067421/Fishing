using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteLevel
{
    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    StillSprite mRippleBackgroundStillSprite = null;

    CSpriteBackgroundLoader mBackgroundLoader = null;
    LevelBackgroundMgr mLevelBackgroundMgr = null;
    LevelScreenShocker mLevelScreenShocker = null;
    LevelSeaStarMgr mLevelSeaStarMgr = null;
    int mCurrentMapId = 0;

#if UNITY_EDITOR
    GameObject mTKGameObject = null;
#endif

    //-------------------------------------------------------------------------
    public void create(CRenderScene render_scene)
    {
        mScene = render_scene;

        _initRippleBackground();

        mBackgroundLoader = new CSpriteBackgroundLoader();
        mLevelScreenShocker = new LevelScreenShocker(mScene, mBackgroundLoader);
        mLevelBackgroundMgr = new LevelBackgroundMgr(mScene, mBackgroundLoader);
        mLevelSeaStarMgr = new LevelSeaStarMgr(mScene);
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mLevelBackgroundMgr != null) mLevelBackgroundMgr.update(elapsed_tm);
        mLevelScreenShocker.update(elapsed_tm);
    }

    //-------------------------------------------------------------------------
    public void switchBackgroundMap(int map_vibid, float already_update_time)
    {
        playSwitchAudio(EbDataMgr.Instance.getData<TbDataMap>(map_vibid).SwitchLevelAudioName);

        mLevelBackgroundMgr.onLoadMapEnd += resetNewBackground;

        mCurrentMapId = map_vibid;

        mLevelBackgroundMgr.switchBackground(EbDataMgr.Instance.getData<TbDataMap>(map_vibid).MapName, already_update_time);
        mRippleBackgroundStillSprite.setActive(true);
        mRippleBackgroundStillSprite.setPosition(new EbVector3(-mScene.getSceneLength() / 2, -mScene.getSceneWidth() / 2, 0));
    }

    //-------------------------------------------------------------------------
    void playSwitchAudio(string audio_file_name)
    {
        if (string.IsNullOrEmpty(audio_file_name)) return;
        //ViDebuger.Warning("playSwitchAudio " + audio_file_name);
        //关闭背景音乐并打开切关卡音乐
        mScene.getSoundMgr().play(audio_file_name, _eSoundLayer.Background);
    }

    //-------------------------------------------------------------------------
    public void shockScreen()
    {
        mLevelScreenShocker.shockScreen();
    }

    //-------------------------------------------------------------------------
    public void stopShockScreen()
    {
        mLevelScreenShocker.stopShockScreen();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mLevelScreenShocker.destroy();
        mLevelBackgroundMgr.destroy();
        mLevelSeaStarMgr.destroy();

        if (mRippleBackgroundStillSprite == null) return;
        mRippleBackgroundStillSprite.destroy();
        mRippleBackgroundStillSprite = null;
    }

    //-------------------------------------------------------------------------
    void _initRippleBackground()
    {
        mRippleBackgroundStillSprite = _loadRippleStillSprite();
        mRippleBackgroundStillSprite.setPosition(new EbVector3(-mScene.getSceneLength() / 2, -mScene.getSceneWidth() / 2, 0));
        mRippleBackgroundStillSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Wave));
        mRippleBackgroundStillSprite.setActive(false);
    }

    //-------------------------------------------------------------------------
    void resetNewBackground()
    {
        mLevelBackgroundMgr.onLoadMapEnd -= resetNewBackground;
        mLevelScreenShocker.switchBackground(mCurrentMapId);
        mLevelSeaStarMgr.switchBackground(mCurrentMapId);
        mScene.getSoundMgr().play(EbDataMgr.Instance.getData<TbDataMap>(mCurrentMapId).AudioName, _eSoundLayer.Background);
    }

    //-------------------------------------------------------------------------
    public void switchBackgroundEarly()
    {
        mLevelBackgroundMgr.switchBackgroundEarly();
    }

    //-------------------------------------------------------------------------
    StillSprite _loadRippleStillSprite()
    {
        UnityEngine.Object prefab_object = Resources.Load("Game/Background/water_ani_movePrefab");
        GameObject game_object = GameObject.Instantiate(prefab_object) as GameObject;
        StillSprite still_sprite = game_object.GetComponent<StillSprite>();
        still_sprite.init(mScene);

#if UNITY_EDITOR
        still_sprite.gameObject.name = "TkSprite_Water_Wave";
        mTKGameObject = GameObject.Find("TKGameObject");
        still_sprite.transform.parent = mTKGameObject.transform;
        //still_sprite.transform.localScale = new Vector3(2.34f, 2.34f, 1);
#endif

        return still_sprite;
    }
}
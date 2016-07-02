using UnityEngine;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteFishDieScore
{
    //-------------------------------------------------------------------------
    static List<CSpriteFishDieScore> mSprites = new List<CSpriteFishDieScore>();
    static List<CSpriteFishDieScore> mReleaseSprites = new List<CSpriteFishDieScore>();

    //-------------------------------------------------------------------------
    public static CSpriteFishDieScore getScore(CRenderScene render_scene, EbVector3 pos, int score, float delay_time)
    {
        CSpriteFishDieScore sprite_score = new CSpriteFishDieScore(render_scene, pos, score, delay_time);
        mSprites.Add(sprite_score);
        return sprite_score;
    }

    //-------------------------------------------------------------------------
    public static void DestroyAll()
    {
        foreach (var it in mSprites)
        {
            it.destroy();
        }
        mSprites.Clear();

        foreach (var it in mReleaseSprites)
        {
            it.destroy();
        }
        mReleaseSprites.Clear();
    }

    //-------------------------------------------------------------------------
    public static void updateall(float elapsed_tm)
    {
        foreach (var it in mReleaseSprites)
        {
            mSprites.Remove(it);
            it.destroy();
        }
        mReleaseSprites.Clear();

        foreach (var it in mSprites)
        {
            it.update(elapsed_tm);
        }
    }

    //-------------------------------------------------------------------------
    static void remove(CSpriteFishDieScore number)
    {
        mReleaseSprites.Add(number);
    }

    //-------------------------------------------------------------------------
    bool mIsNeedAutoDestroy = false;
    float mFunDelayTime = 0;
    float mTimeCounter = 0;
    CSpriteNumber mCSpriteNumber = null;
    int mNumber = 0;
    bool mNeedFadeout = false;
    float mFadeoutFunDelayTime = 1.6f;
    float mFadeoutTimeCounter = 0;
    float mAlpha = 1f;
    MassEntity mMassEntity = null;

    //-------------------------------------------------------------------------
    CSpriteFishDieScore(CRenderScene render_scene, EbVector3 pos, int score, float delay_time)
    {
        mCSpriteNumber = new CSpriteNumber(render_scene, new CScoreDigitFactory(render_scene));
        mCSpriteNumber.create(score, pos, 0, getByScore(score));
        mCSpriteNumber.setLayer(render_scene.getLayerAlloter().getLayer(_eLevelLayer.FishScore));
        mFunDelayTime = delay_time;
        mIsNeedAutoDestroy = true;

        _initRoute(pos, 0);
        mNumber = score;
    }

    //-------------------------------------------------------------------------
    CSpriteNumber._eNumberSize getByScore(int score)
    {
        int length = score.ToString().Length;
        switch (length)
        {
            case 1:
                return CSpriteNumber._eNumberSize.Small1;
            case 2:
                return CSpriteNumber._eNumberSize.Small1;
            case 3:
                return CSpriteNumber._eNumberSize.Small1;
            case 4:
                return CSpriteNumber._eNumberSize.Nomal;
            case 5:
                return CSpriteNumber._eNumberSize.Big1;
            case 6:
                return CSpriteNumber._eNumberSize.Big2;
            default:
                return CSpriteNumber._eNumberSize.Big3;
        }
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mIsNeedAutoDestroy)
        {
            mTimeCounter += elapsed_tm;
            if (mTimeCounter >= mFunDelayTime)
            {
                remove(this);
            }
            else if (mNeedFadeout)
            {
                mAlpha -= elapsed_tm / mFadeoutFunDelayTime;
                mCSpriteNumber.setAlpha(mAlpha);
            }
            else
            {
                mMassEntity.update(elapsed_tm);
                mCSpriteNumber.setPosition(mMassEntity.Position, 0);
                if (mMassEntity.IsEndRoute)
                {
                    mNeedFadeout = true;
                }
            }
        }
    }

    public void destroy()
    {
        mCSpriteNumber.destroy();
    }

    //-------------------------------------------------------------------------
    void _initRoute(EbVector3 init_position, float init_direction)
    {
        mMassEntity = new MassEntity();
        RouteParaCurve route = new RouteParaCurve();
        route.create(init_position, init_direction, 0.4f, 20f);
        mMassEntity.setRoute(route);
    }
}

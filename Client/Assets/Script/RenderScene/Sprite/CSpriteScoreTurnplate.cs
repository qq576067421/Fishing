using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GF.Common;
using Ps;

public class CSpriteScoreTurnplate
{
    //-------------------------------------------------------------------------
    EbVector3 mPosition = EbVector3.Zero;
    CRenderScene mScene = null;
    StillParticle mStillParticle = null;
    CSpriteNumber mSpriteNumber = null;
    float mAngle = 0;
    float mMaxAngle = 20f;
    bool mRoteteRight = true;
    float mCurrentAngle = 0;
    float mRotateSpeed = 85f;
    bool mIsDisplay = false;

    //-------------------------------------------------------------------------
    public CSpriteScoreTurnplate(CRenderScene scene, EbVector3 position, float up_angle)
    {
        mScene = scene;
        mPosition = position;
        mAngle = up_angle;
        mSpriteNumber = new CSpriteNumber(mScene, new CScoreDigitFactory(mScene));
        mSpriteNumber.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.TurretScore));
    }

    //---------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mIsDisplay)
        {
            updateRotate(elapsed_tm);
            if (mStillParticle == null || !mStillParticle.CanDestroy) return;
            hide();
        }
    }

    //---------------------------------------------------------------------
    void initRotate()
    {
        mCurrentAngle = 0;
        mRoteteRight = true;
    }

    //---------------------------------------------------------------------
    void updateRotate(float elapsed_tm)
    {
        if (mRoteteRight)
        {
            mCurrentAngle += mRotateSpeed * elapsed_tm;
            if (mCurrentAngle >= mMaxAngle)
            {
                mCurrentAngle = mMaxAngle;
                mRoteteRight = !mRoteteRight;
            }
        }
        else
        {
            mCurrentAngle -= mRotateSpeed * elapsed_tm;
            if (mCurrentAngle <= -mMaxAngle)
            {
                mCurrentAngle = -mMaxAngle;
                mRoteteRight = !mRoteteRight;
            }
        }
        mSpriteNumber.setPosition(mPosition, mAngle + mCurrentAngle);
    }

    //-------------------------------------------------------------------------
    public void display(int score, TbDataParticle particle_data)
    {
        if (score <= 0) return;

        mSpriteNumber.create(score, 100, mPosition, mAngle + mCurrentAngle);

        mStillParticle = mScene.getParticlemanager().newParticle(particle_data.ParticlePrefabName);
        mStillParticle.setPosition(mPosition);
        mStillParticle.setLooping(false);
        mStillParticle.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.TurretScoreBg));

        mIsDisplay = true;
        initRotate();
    }

    //-------------------------------------------------------------------------
    void hide()
    {
        mIsDisplay = false;
        mSpriteNumber.destroy();
        _distroyTurnplateParticle();
    }

    //-------------------------------------------------------------------------
    public void release()
    {
        if (mSpriteNumber != null)
        {
            mSpriteNumber.destroy();
            mSpriteNumber = null;
        }
        _distroyTurnplateParticle();
    }

    //-------------------------------------------------------------------------
    void _distroyTurnplateParticle()
    {
        if (mStillParticle != null)
        {
            mScene.getParticlemanager().freeParticle(mStillParticle);
            mStillParticle = null;
        }
    }
}
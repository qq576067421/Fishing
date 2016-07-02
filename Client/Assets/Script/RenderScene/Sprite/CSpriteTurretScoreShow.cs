using System;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;
using Ps;

public class CSpriteTurretScoreShow
{
    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    //CSpriteScore mCSpriteScore = null;
    CSpriteCounter mCSpriteCounter = null;
    CSpriteScoreTurnplate mScoreTurnplate = null;
    CRenderTurret mCRenderTurret = null;
    AimParticle mAimParticle = null;

    //-------------------------------------------------------------------------
    public void create(CRenderScene scene, CRenderTurret render_turret)
    {
        mScene = scene;
        mCRenderTurret = render_turret;
        int turret_id = render_turret.getTurretId();

        CTurretHelper turret_helper = mScene.getTurretHelper();

        float base_angle = turret_helper.getBaseAngleByTurretId(turret_id);

        mCSpriteCounter = new CSpriteCounter(mScene,
            turret_helper.getPositionByOffset(turret_id, mScene.getRenderConfigure().ChipsOffset),
            turret_helper.getBaseAngleByTurretId(turret_id));

        mScoreTurnplate = new CSpriteScoreTurnplate(mScene, turret_helper.getPositionByOffset(turret_id,
            mScene.getRenderConfigure().TurretTurnplateOffset), base_angle);

        //mCSpriteScore = new CSpriteScore(mScene, turret_helper.getPositionByOffset(turret_id, mScene.getRenderConfigure().ChipsOffset), 0, turret_id);

        if (turret_id == 0 || turret_id == 1)
        {
            EbVector3 offset = mScene.getRenderConfigure().TurretPanelScoreOffset;
            offset.x += mScene.getRenderConfigure().UpTurretPanelScoreOffset;
            //mCSpriteScore.setDigitPosition(turret_helper.getPositionByOffset(turret_id, offset), 0);
        }
        else
        {
            //mCSpriteScore.setDigitPosition(turret_helper.getPositionByOffset(turret_id, mScene.getRenderConfigure().TurretPanelScoreOffset), base_angle);
        }
        //mCSpriteScore.setBgPosition(turret_helper.getPositionByOffset(turret_id, mScene.getRenderConfigure().TurretPanelScoreBgOffset), base_angle);
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        mCSpriteCounter.update(elapsed_tm);

        mScoreTurnplate.update(elapsed_tm);

        if (mAimParticle != null)
        {
            mAimParticle.update(elapsed_tm);
            if (mAimParticle.IsEnd)
            {
                mAimParticle = null;
            }
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        //if (mCSpriteScore != null)
        //{
        //    mCSpriteScore.release();
        //    mCSpriteScore = null;
        //}

        if (mScoreTurnplate != null)
        {
            mScoreTurnplate.release();
            mScoreTurnplate = null;
        }

        if (mCSpriteCounter != null)
        {
            mCSpriteCounter.destroy();
            mCSpriteCounter = null;
        }

        mAimParticle = null;
    }

    //-------------------------------------------------------------------------
    public void setScore(int score)
    {
        //mCSpriteScore.setScore(score);
    }

    //-------------------------------------------------------------------------
    public void setAim(CRenderFish lock_fish)
    {
        if (mCRenderTurret.getVibTurret().AimParticle.Id <= 0) return;

        StillParticle still_particle = mScene.getParticlemanager().newParticle(mCRenderTurret.getVibTurret().AimParticle.ParticlePrefabName);
        mAimParticle = new AimParticle(lock_fish, still_particle);
    }

    //-------------------------------------------------------------------------
    public void displayScoreTurnplate(int score, TbDataParticle particle_data)
    {
        if (particle_data.Id <= 0) return;
        mScoreTurnplate.display(score, particle_data);
    }

    //-------------------------------------------------------------------------
    public void displayChips(int score)
    {
        mCSpriteCounter.addChip(score);
    }
}

public class AimParticle
{
    //-------------------------------------------------------------------------
    public bool IsEnd { get { return mStillParticle.CanDestroy; } }
    CRenderFish mLockFish = null;
    StillParticle mStillParticle = null;

    //-------------------------------------------------------------------------
    public AimParticle(CRenderFish lock_fish, StillParticle still_particle)
    {
        mLockFish = lock_fish;
        mStillParticle = still_particle;
        mStillParticle.setLooping(false);
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        mStillParticle.setPosition(mLockFish.Position);
    }
}
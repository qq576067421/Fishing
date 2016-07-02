using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteTurret
{
    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    CRenderTurret mRenderTurret = null;
    CSpriteTurretFort mCSpriteTurretFort = null;
    //CSpriteNumber mRateNumber = null;
    LinkLockedFishFeature mLinkLockedFishFeature = null;

    EbVector3 mRateNumberPosition = EbVector3.Zero;
    float mRateNumberAngel = 0;

    //-------------------------------------------------------------------------
    public void create(CRenderScene scene, CRenderTurret render_turret)
    {
        mScene = scene;
        mRenderTurret = render_turret;

        mCSpriteTurretFort = new CSpriteTurretFort();
        mCSpriteTurretFort.create(mScene, mRenderTurret);

        mLinkLockedFishFeature = new LinkLockedFishFeature(mScene, mRenderTurret);

        //mRateNumber = new CSpriteNumber(mScene, new CPanelDigitFactory(mScene));
        //mRateNumber.create(0, mRateNumberPosition, mRateNumberAngel, CSpriteNumber._eNumberSize.Small1);
        //mRateNumber.setTag("CSpriteTurret" + mRenderTurret.getTurretId());
        //mRateNumber.setTrigger(true);
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mLinkLockedFishFeature != null)
        {
            mLinkLockedFishFeature.update(Time.deltaTime);
        }

        if (mCSpriteTurretFort != null)
        {
            mCSpriteTurretFort.update(elapsed_tm);
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        if (mCSpriteTurretFort != null)
        {
            mCSpriteTurretFort.destroy();
            mCSpriteTurretFort = null;
        }

        if (mLinkLockedFishFeature != null)
        {
            mLinkLockedFishFeature.destroy();
            mLinkLockedFishFeature = null;
        }

        //mRateNumber.destroy();
        //mRateNumber = null;
    }

    //-------------------------------------------------------------------------
    public void fireAt(float target_direction)
    {
        mCSpriteTurretFort.fireAt(target_direction);
    }

    //-------------------------------------------------------------------------
    public void aimAt(float target_direction)
    {
        mCSpriteTurretFort.aimAt(target_direction);
    }

    //-------------------------------------------------------------------------
    public void reloadAnimation()
    {
        mCSpriteTurretFort.reloadAnimation();
    }

    //-------------------------------------------------------------------------
    public void setBarrelColor(UnityEngine.Color color)
    {
        mCSpriteTurretFort.setBarrelColor(color);
    }

    //-------------------------------------------------------------------------
    public void displayLinkFish(CRenderFish fish)
    {
        mLinkLockedFishFeature.display(fish);
    }

    //-------------------------------------------------------------------------
    public void displayRate(int number)
    {
        //mRateNumber.destroy();
        //mRateNumber.setNumber(number);
        //mRateNumber.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.TurretRate));

        //int turret_id = mRenderTurret.getTurretId();
        //if (turret_id == 0 || turret_id == 1)
        //{
        //    mRateNumber.setPosition(mScene.getTurretHelper().getPositionByOffset(mRenderTurret.getTurretId(), mScene.getRenderConfigure().TurretRateOffset),
        //        0f);
        //}
        //else
        //{
        //    mRateNumber.setPosition(mScene.getTurretHelper().getPositionByOffset(mRenderTurret.getTurretId(), mScene.getRenderConfigure().TurretRateOffset),
        //        mScene.getTurretHelper().getBaseAngleByTurretId(mRenderTurret.getTurretId()));
        //}
        //mRateNumber.setTrigger(true);

        //mRateNumber.setPosition(new EbVector3(10000,10000,0),0);
    }
}
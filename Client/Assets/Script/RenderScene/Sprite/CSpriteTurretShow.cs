using System;
using System.Collections.Generic;
using GF.Common;
using UnityEngine;
using Ps;

public class CSpriteTurretShow
{
    //-------------------------------------------------------------------------
    CSpriteTurretScoreShow mCSpriteTurretScoreShow = null;
    CSpriteTurret mSpriteTurret = null;
    CRenderScene mScene = null;

    //-------------------------------------------------------------------------
    public void create(CRenderScene scene, CRenderTurret render_turret)
    {
        mScene = scene;

        if (mScene.isBot()) return;

        mCSpriteTurretScoreShow = new CSpriteTurretScoreShow();
        mCSpriteTurretScoreShow.create(scene, render_turret);

        mSpriteTurret = new CSpriteTurret();
        mSpriteTurret.create(scene, render_turret);
        mSpriteTurret.aimAt(render_turret.getTurretAngle());
        mSpriteTurret.displayRate(render_turret.getTurretRate());
        mSpriteTurret.reloadAnimation();
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mScene.isBot()) return;

        mCSpriteTurretScoreShow.update(elapsed_tm);
        mSpriteTurret.update(elapsed_tm);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        if (mScene.isBot()) return;

        if (mCSpriteTurretScoreShow != null)
        {
            mCSpriteTurretScoreShow.destroy();
            mCSpriteTurretScoreShow = null;
        }

        if (mSpriteTurret != null)
        {
            mSpriteTurret.destroy();
            mSpriteTurret = null;
        }
    }

    //-------------------------------------------------------------------------
    public void setAim(CRenderFish lock_fish)
    {
        if (mScene.isBot()) return;
        mCSpriteTurretScoreShow.setAim(lock_fish);
    }

    //-------------------------------------------------------------------------
    public void displayScoreTurnplate(int score, TbDataParticle particle_data)
    {
        if (mScene.isBot()) return;
        mCSpriteTurretScoreShow.displayScoreTurnplate(score, particle_data);
    }

    //-------------------------------------------------------------------------
    public void displayChips(int score)
    {
        if (mScene.isBot()) return;
        mCSpriteTurretScoreShow.displayChips(score);
    }

    //-------------------------------------------------------------------------
    public void fireAt(float target_direction)
    {
        if (mScene.isBot()) return;
        mSpriteTurret.fireAt(target_direction);
    }

    //-------------------------------------------------------------------------
    public void aimAt(float target_direction)
    {
        if (mScene.isBot()) return;
        mSpriteTurret.aimAt(target_direction);
    }

    //-------------------------------------------------------------------------
    public void displayLinkFish(CRenderFish fish)
    {
        if (mScene.isBot()) return;
        mSpriteTurret.displayLinkFish(fish);
    }

    //-------------------------------------------------------------------------
    public void setBarrelColor(UnityEngine.Color c)
    {
        if (mScene.isBot()) return;
        mSpriteTurret.setBarrelColor(c);
    }

    //-------------------------------------------------------------------------
    public void setScore(int score)
    {
        if (mScene.isBot()) return;
        //mCSpriteTurretScoreShow.setScore(score);
    }

    //-------------------------------------------------------------------------
    public void reloadAnimation()
    {
        if (mScene.isBot()) return;
        mSpriteTurret.reloadAnimation();
    }

    //-------------------------------------------------------------------------
    public void displayRate(int number)
    {
        if (mScene.isBot()) return;
        mSpriteTurret.displayRate(number);
    }
}
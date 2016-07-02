using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;
using UnityEngine;
using Ps;

public class CSpriteScore
{
    //-------------------------------------------------------------------------
    CSpriteNumber mSpriteNumber = null;
    float mDigitGap = 20;
    StillSprite mBackground = null;
    CRenderScene mScene = null;

    //-------------------------------------------------------------------------
    public CSpriteScore(CRenderScene scene, EbVector3 position, float angle, int turret_no)
    {
        mScene = scene;

        mSpriteNumber = new CSpriteNumber(mScene, new CPanelDigitFactory(mScene));
        mSpriteNumber.create(0, position, angle, CSpriteNumber._eNumberSize.Small1, CSpriteNumber._eNumberAlign.Right);
        mSpriteNumber.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.TurretScore));
        mBackground = mScene.getRenderObjectPool().newStillSprite();
        mBackground.playAnimation("score_panel_" + turret_no);
        mBackground.setScale(1, 1f);

        setScore(0);

#if UNITY_EDITOR
        mBackground.gameObject.name = "TkSpriteSpriteScore_score_panel_" + turret_no;
#endif
    }

    //-------------------------------------------------------------------------
    public void setDigitPosition(EbVector3 position, float up_angle)
    {
        mSpriteNumber.setPosition(position, up_angle);
    }

    //-------------------------------------------------------------------------
    public void setBgPosition(EbVector3 position, float up_angle)
    {
        mBackground.setPosition(position);
        mBackground.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.TurretScoreBg));
        mBackground.setDirection(up_angle);
    }

    //-------------------------------------------------------------------------
    public void setScore(int score)
    {
        mSpriteNumber.setNumber(score);
    }

    //-------------------------------------------------------------------------
    public void release()
    {
        mSpriteNumber.destroy();
        mScene.getRenderObjectPool().freeStillSprite(mBackground);
    }

    //-------------------------------------------------------------------------
    string getSpriteNameByDigit(int digit)
    {
        if (digit < 0)
        {
            digit = 0;
        }

        if (digit > 9)
        {
            digit = 9;
        }

        digit++;

        return "fontscorespr00" + digit.ToString("D2");
    }
}
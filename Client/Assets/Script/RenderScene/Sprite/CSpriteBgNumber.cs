using System;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteChipBg
{
    class SpriteBox
    {
        public StillSprite mStillSprite;

        bool mHorizontalScale;
        bool mVerticalScale;

        public float mFixedWidth;
        public float mFixedHeight;

        public EbVector3 mOffset;

        public SpriteBox(StillSprite sprite, bool horizontal_scale, bool vertical_scale, float width, float height)
        {
            mStillSprite = sprite;
            mHorizontalScale = horizontal_scale;
            mVerticalScale = vertical_scale;
            mFixedWidth = width;
            mFixedHeight = height;
        }

        //-------------------------------------------------------------------------
        public void setScale(float x_scale, float y_scale, float fixed_scale, SpriteBox center_box)
        {
            if (mHorizontalScale && mVerticalScale)
            {
                mStillSprite.setScale(x_scale, y_scale);
            }
            else if (mHorizontalScale)
            {
                mStillSprite.setScale(x_scale, fixed_scale);
            }
            else if (mVerticalScale)
            {
                mStillSprite.setScale(fixed_scale, y_scale);
            }

            if (this != center_box)
            {
                mOffset = calPosition(this, center_box);
            }
            else
            {

            }
        }

        EbVector3 calPosition(SpriteBox cal_box, SpriteBox center_box)
        {
            EbVector3 center_box_new_size = new EbVector3(center_box.mFixedWidth, center_box.mFixedHeight, 0);
            center_box_new_size *= 0.5f;
            EbVector3 cal_box_new_size = new EbVector3(cal_box.mFixedWidth, cal_box.mFixedHeight, 0);
            cal_box_new_size *= 0.5f;

            return center_box_new_size + cal_box_new_size;
        }
    }

    List<SpriteBox> mStillSprites = new List<SpriteBox>(9);
    EbVector3 mPosition;
    float mAngle;
    float mXScale;
    float mYScale;
    float mFixedScale;
    CRenderScene mScene;

    public void create(CRenderScene scene)
    {
        mScene = scene;

        mStillSprites.Add(newSpriteBox(false, false, 2, 2));
        mStillSprites.Add(newSpriteBox(true, false, 2, 2));
        mStillSprites.Add(newSpriteBox(false, false, 2, 2));

        mStillSprites.Add(newSpriteBox(false, true, 2, 2));
        mStillSprites.Add(newSpriteBox(true, true, 2, 2));
        mStillSprites.Add(newSpriteBox(false, true, 2, 2));

        mStillSprites.Add(newSpriteBox(false, false, 2, 2));
        mStillSprites.Add(newSpriteBox(true, false, 2, 2));
        mStillSprites.Add(newSpriteBox(false, false, 2, 2));
    }

    SpriteBox newSpriteBox(bool horizontal_scale, bool vertical_scale, float width, float height)
    {
        return new SpriteBox(newStillSprite(), horizontal_scale, vertical_scale, width, height);
    }

    StillSprite newStillSprite()
    {
        return mScene.getRenderObjectPool().newStillSprite();
    }

    public void setPosition(EbVector3 position, float angle)
    {
        mPosition = position;
        mAngle = angle;
        _updateView();
    }

    //-------------------------------------------------------------------------
    public void setAlpha(float alpha)
    {
        foreach (var it in mStillSprites)
        {
            it.mStillSprite.setAlpha(alpha);
        }
    }

    //-------------------------------------------------------------------------
    public void setScale(float x_scale, float y_scale)
    {
        mXScale = x_scale;
        mYScale = y_scale;
        foreach (var it in mStillSprites)
        {
            it.setScale(x_scale, y_scale, mFixedScale, getSpriteBox(1, 1));
        }
        _updateView();
    }

    //-------------------------------------------------------------------------
    void _updateView()
    {
        foreach (var it in mStillSprites)
        {
            EbVector3 offset = new EbVector3(it.mOffset.x * mXScale, it.mOffset.y * mYScale, 0);
            it.mStillSprite.setPosition(mPosition + CLogicUtility.getVector2ByRotateAngle(offset, mAngle));
            it.mStillSprite.setDirection(mAngle);
        }
    }

    //-------------------------------------------------------------------------
    SpriteBox getSpriteBox(int x, int y)
    {
        return mStillSprites[x * 3 + y];
    }

    EbVector3 calPosition(SpriteBox cal_box, SpriteBox center_box)
    {
        EbVector3 center_box_new_size = new EbVector3(center_box.mFixedWidth, center_box.mFixedHeight, 0);
        center_box_new_size *= 0.5f;
        EbVector3 cal_box_new_size = new EbVector3(cal_box.mFixedWidth, cal_box.mFixedHeight, 0);
        cal_box_new_size *= 0.5f;

        return center_box_new_size + cal_box_new_size;
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var it in mStillSprites)
        {
            mScene.getRenderObjectPool().freeStillSprite(it.mStillSprite);
        }
        mStillSprites.Clear();
    }
}

public class CSpriteBgNumber
{
    //-------------------------------------------------------------------------
    public enum BgColorEnum
    {
        Red,
        Green
    }

    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    CSpriteNumber mCSpriteNumber = null;
    StillSprite mNumberBackgroundCSprite = null;

    //-------------------------------------------------------------------------
    public CSpriteBgNumber(CRenderScene scene, BgColorEnum color, int number)
    {
        mScene = scene;
        float number_layer = mScene.getLayerAlloter().getLayer(_eLevelLayer.TurretScore);

        mCSpriteNumber = new CSpriteNumber(mScene, new CPanelDigitFactory(mScene));
        mCSpriteNumber.create(number, EbVector3.Zero, 0, CSpriteNumber._eNumberSize.Small2);
        mCSpriteNumber.setLayer(number_layer);

        mNumberBackgroundCSprite = mScene.getRenderObjectPool().newStillSprite();

        mNumberBackgroundCSprite.setLayer(number_layer + mScene.getLayerAlloter().EachFishGap / 100f);
        mNumberBackgroundCSprite.setScale(0.65f * number.ToString().Length, 0.8f);

        mNumberBackgroundCSprite.playAnimation(getAnimationNameByColor(color));

#if UNITY_EDITOR
        mNumberBackgroundCSprite.gameObject.name = "TkSpriteChips_" + getAnimationNameByColor(color);
#endif
    }

    //-------------------------------------------------------------------------
    string getAnimationNameByColor(BgColorEnum color)
    {
        if (color == BgColorEnum.Red)
        {
            return "chip_score_bg_red";
        }
        return "chip_score_bg_green";
    }

    //-------------------------------------------------------------------------
    public void setPosition(EbVector3 position, float angle)
    {
        mCSpriteNumber.setPosition(position, angle);
        mNumberBackgroundCSprite.setPosition(position);
        mNumberBackgroundCSprite.setDirection(angle);
    }

    //-------------------------------------------------------------------------
    public void setAlpha(float alpha)
    {
        mCSpriteNumber.setAlpha(alpha);
        mNumberBackgroundCSprite.setAlpha(alpha);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mCSpriteNumber.destroy();
        mScene.getRenderObjectPool().freeStillSprite(mNumberBackgroundCSprite);
    }
}
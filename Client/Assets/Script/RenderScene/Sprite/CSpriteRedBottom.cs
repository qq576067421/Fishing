using System;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;
using Ps;

public class CSpriteRedBottom
{
    //-------------------------------------------------------------------------
    protected class SpriteAndOffset
    {
        public FishStillSprite mFishStillSprite;
        public EbVector3 mOffset;
        public float mAngle = 0;
        public float mAngleSpeed;
        public float mLayerOffset;
        public string mDieAnimationName;

        public SpriteAndOffset(FishStillSprite fish, EbVector3 offset, float angle_speed, float layer_offset, string die_animation_name = "")
        {
            mFishStillSprite = fish;
            mOffset = offset;
            mAngleSpeed = angle_speed;
            mLayerOffset = layer_offset;
            mDieAnimationName = die_animation_name;
        }

        public void update(float elapsed_tm)
        {
            mAngle += mAngleSpeed * elapsed_tm;
            mFishStillSprite.setDirection(mAngle);
        }
    }

    //-------------------------------------------------------------------------
    public delegate void onTriggerEnterDelegate(Collider other);
    public onTriggerEnterDelegate onTriggerEnter;

    List<SpriteAndOffset> mSprite = new List<SpriteAndOffset>();
    protected EbVector3 mPosition = new EbVector3(10000f, 0, 0);
    protected float mAngle;
    protected float mScale = 1f;
    protected float mFixedScale = 1f;
    CRenderScene mScene = null;

    public void create(CRenderScene scene, ISpriteFish sprite_fish, TbDataFish vib_fish_data)
    {
        mScene = scene;
        mFixedScale = (float)vib_fish_data.CycleHeight / (float)vib_fish_data.CyclePixelHeight;

        initFish("red_fish_bottom_bottom", EbVector3.Zero, 130, mScene.getLayerAlloter().EachFishGap / 10f * 3, sprite_fish);
        //initFish("red_fish_bottom_middle", EbVector3.Zero, -130, mScene.getLayerAlloter().EachFishGap / 10f * 2, sprite_fish);
        initFish("red_fish_bottom_sign2", EbVector3.Zero, 0, mScene.getLayerAlloter().EachFishGap / 10f, sprite_fish);

        setScale(mFixedScale);
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        foreach (var it in mSprite)
        {
            it.update(elapsed_tm);
        }
    }

    void initFish(string animation, EbVector3 offset, float angle_speed, float layer_offset, ISpriteFish sprite_fish)
    {
        SpriteAndOffset sprite_offset =
            new SpriteAndOffset(mScene.getRenderObjectPool().newFishStillSprite(),
                    EbVector3.Zero, angle_speed, layer_offset);
        sprite_offset.mFishStillSprite.init(sprite_fish, mScene);
        sprite_offset.mFishStillSprite.playAnimation(animation);
        mSprite.Add(sprite_offset);
    }

    void _updateView()
    {
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.setPosition(mPosition + CLogicUtility.getVector2ByRotateAngle(it.mOffset * mScale * mFixedScale, mAngle));
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var it in mSprite)
        {
            mScene.getRenderObjectPool().freeFishStillSprite(it.mFishStillSprite);
        }
        mSprite.Clear();
    }

    //-------------------------------------------------------------------------
    public void setScale(float scale)
    {
        mScale = scale;
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.setScale(mScale * mFixedScale);
        }
        _updateView();
    }

    //-------------------------------------------------------------------------
    public void setLayer(float layer)
    {
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.setLayer(layer + it.mLayerOffset);
        }
    }

    //-------------------------------------------------------------------------
    public void setPosition(EbVector3 pos, float angle)
    {
        mPosition = pos;
        //mAngle = angle;
        _updateView();
    }

    //-------------------------------------------------------------------------
    public void setDirection(float angle)
    {
        mAngle = angle;
        _updateView();
    }

    //-------------------------------------------------------------------------
    public void setTrigger(bool isTrigger, float size = 1)
    {
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.setTrigger(isTrigger);
        }
    }

    //-------------------------------------------------------------------------
    public void setColor(Color c)
    {
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.setColor(c);
        }
    }

    //-------------------------------------------------------------------------
    public void setAlpha(float alpha)
    {
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.setAlpha(alpha);
        }
    }

    //-------------------------------------------------------------------------
    public void setTag(string tag)
    {
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.setTag(tag);
        }
    }

    //-------------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (onTriggerEnter == null) return;
        onTriggerEnter(other);
    }

    //-------------------------------------------------------------------------
    public bool hasFishStillSprite(FishStillSprite fish_still_sprite)
    {
        foreach (var it in mSprite)
        {
            if (it.mFishStillSprite == fish_still_sprite)
            {
                return true;
            }
        }
        return false;
    }

    //-------------------------------------------------------------------------
    public void setGameobjectName(string name)
    {
        foreach (var it in mSprite)
        {
            it.mFishStillSprite.gameObject.name = name;
        }
    }
}
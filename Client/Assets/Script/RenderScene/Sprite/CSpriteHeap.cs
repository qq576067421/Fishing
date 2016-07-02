using System;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteHeap
{
    struct HeapScruct
    {
        public EbVector3 Position;
        public float UpAngle;
        public float RightAngle;
        public int NumberOfChip;
        public float Alpha;
    }

    enum _eHeapState
    {
        None,
        Adding,
        Fadeout
    }

    //-------------------------------------------------------------------------
    public bool EndOfLife { get { return mEndOfLife; } }
    CRenderScene mScene = null;
    HeapScruct mHeapScruct;
    _eHeapState mHeapState = _eHeapState.None;
    CSpriteBgNumber mCSpriteBgNumber = null;

    List<StillSprite> mStackChips = new List<StillSprite>();
    float mSecondsSinceCreation = 0f;
    float mSecondsOfLife = 2f;

    float mSecondsSincePreChip = 0f;
    float mSecondsOfAddChip = 0.02f;
    float mFadeoutSpeed = 1f;

    bool mEndOfLife = false;
    float mHeapLayer;

    //-------------------------------------------------------------------------
    public CSpriteHeap(CRenderScene scene, int number_of_chip, int score, EbVector3 position, float up_angle, CSpriteBgNumber.BgColorEnum color)
    {
        mScene = scene;

        mHeapScruct.NumberOfChip = number_of_chip;
        mHeapScruct.Position = position;
        mHeapScruct.UpAngle = up_angle;
        mHeapScruct.Alpha = 1f;
        mCSpriteBgNumber = new CSpriteBgNumber(mScene, color, score);

        mHeapLayer = mScene.getLayerAlloter().getLayer(_eLevelLayer.TurretChips);

        mHeapState = _eHeapState.Adding;
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        mSecondsSinceCreation += elapsed_tm;

        switch (mHeapState)
        {
            case _eHeapState.None:
                break;
            case _eHeapState.Adding:
                _addChip(elapsed_tm);
                if (mSecondsOfLife <= mSecondsSinceCreation)
                {
                    mHeapState = _eHeapState.Fadeout;
                }
                break;
            case _eHeapState.Fadeout:
                _fadeout(elapsed_tm);
                break;
        }
    }

    //-------------------------------------------------------------------------
    void _addChip(float elapsed_tm)
    {
        if (mStackChips.Count >= mHeapScruct.NumberOfChip)
        {
            return;
        }
        mSecondsSincePreChip += elapsed_tm;
        if (mSecondsSincePreChip >= mSecondsOfAddChip)
        {
            _newChip();
        }
    }

    void _newChip()
    {
        StillSprite still_sprite = mScene.getRenderObjectPool().newStillSprite();
        still_sprite.playAnimation("chip");
        still_sprite.setScale(mScene.getRenderConfigure().ChipSacle);
        still_sprite.setLayer(mHeapLayer);
        mStackChips.Add(still_sprite);


#if UNITY_EDITOR
        still_sprite.gameObject.name = "TkSpriteChips_chip";
#endif

        _updateChipsView(mHeapScruct.Position, mHeapScruct.UpAngle);
    }

    //-------------------------------------------------------------------------
    void _updateChipsView(EbVector3 position, float up_angle)
    {
        float chip_gap = mScene.getRenderConfigure().ChipVSpace;
        float chip_number_offset = mScene.getRenderConfigure().ChipNumberOffset;

        EbVector3 new_position = EbVector3.Zero;

        int index = 0;
        foreach (var it in mStackChips)
        {
            new_position = position;
            new_position += CLogicUtility.getDirection(up_angle) * (chip_gap * index++);
            it.setPosition(new_position);
            it.setDirection(up_angle);
        }

        new_position += CLogicUtility.getDirection(up_angle) * chip_number_offset;
        mCSpriteBgNumber.setPosition(new_position, up_angle);
    }

    //-------------------------------------------------------------------------
    void _fadeout(float elapsed_tm)
    {
        mHeapScruct.Alpha -= mFadeoutSpeed * elapsed_tm;
        if (mHeapScruct.Alpha <= 0)
        {
            mHeapScruct.Alpha = 0;
            mEndOfLife = true;
        }
        foreach (var it in mStackChips)
        {
            it.setAlpha(mHeapScruct.Alpha);
        }
        mCSpriteBgNumber.setAlpha(mHeapScruct.Alpha);
    }

    //-------------------------------------------------------------------------
    public void translate(EbVector3 position)
    {
        mHeapScruct.Position += position;
        _updateChipsView(mHeapScruct.Position, mHeapScruct.UpAngle);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var it in mStackChips)
        {
            mScene.getRenderObjectPool().freeStillSprite(it);
        }
        mStackChips.Clear();
        mCSpriteBgNumber.destroy();
    }
}
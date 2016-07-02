using System;
using Ps;

public abstract class CSpriteNumberFactory
{
    protected CRenderScene mScene = null;
    public CSpriteNumberFactory(CRenderScene scene) { mScene = scene; }
    public abstract StillSprite newDigitSprite(int digit);
    public abstract void freeDigitSprite(StillSprite digit_sprite);
    public abstract float getScale(CSpriteNumber._eNumberSize size);
}

public class CPanelDigitFactory : CSpriteNumberFactory
{
    //-------------------------------------------------------------------------
    public CPanelDigitFactory(CRenderScene scene)
        : base(scene) { }

    //-------------------------------------------------------------------------
    public override StillSprite newDigitSprite(int digit)
    {
        if (digit < 0) digit = 0;
        if (digit > 9) digit = 9;
        StillSprite still_sprite = mScene.getRenderObjectPool().newStillSprite();
        still_sprite.playAnimation("digit_panel_" + digit);

#if UNITY_EDITOR
        still_sprite.gameObject.name = "TkSpritePanelDigit_" + digit;
#endif

        return still_sprite;
    }

    //-------------------------------------------------------------------------
    public override void freeDigitSprite(StillSprite digit_sprite)
    {
        mScene.getRenderObjectPool().freeStillSprite(digit_sprite);
    }

    //-------------------------------------------------------------------------
    public override float getScale(CSpriteNumber._eNumberSize size)
    {
        return mScene.getRenderConfigure().getPanelNumberScale(size);
    }
}

public class CScoreDigitFactory : CSpriteNumberFactory
{
    //-------------------------------------------------------------------------
    public CScoreDigitFactory(CRenderScene scene)
        : base(scene) { }

    //-------------------------------------------------------------------------
    public override StillSprite newDigitSprite(int digit)
    {
        if (digit < 0) digit = 0;
        if (digit > 9) digit = 9;

        StillSprite still_sprite = mScene.getRenderObjectPool().newStillSprite();
        still_sprite.playAnimation("digit_score_" + digit);

#if UNITY_EDITOR
        still_sprite.gameObject.name = "TkSpriteScoreDigit_" + digit;
#endif

        return still_sprite;
    }

    //-------------------------------------------------------------------------
    public override void freeDigitSprite(StillSprite digit_sprite)
    {
        mScene.getRenderObjectPool().freeStillSprite(digit_sprite);
    }

    //-------------------------------------------------------------------------
    public override float getScale(CSpriteNumber._eNumberSize size)
    {
        return mScene.getRenderConfigure().getScoreNumberScale(size);
    }
}
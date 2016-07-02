using UnityEngine;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteFishNetGroup
{
    class SpriteAndOffset
    {
        public StillSprite mStillSprite;
        public EbVector3 mOffset;
        public string mDieAnimationName;

        public SpriteAndOffset(StillSprite sprite, EbVector3 offset)
        {
            mStillSprite = sprite;
            mOffset = offset;
        }
    }

    //-------------------------------------------------------------------------
    public delegate void onTriggerEnterDelegate(Collider other);
    public onTriggerEnterDelegate onTriggerEnter;
    CRenderScene mScene = null;
    List<SpriteAndOffset> mStillSpriteList = new List<SpriteAndOffset>();

    //-------------------------------------------------------------------------
    public void create(CRenderScene scene, Color color, uint et_player_rpcid, string net_name)
    {
        mScene = scene;

        switch (net_name)
        {
            case "Fishnet12":
                _initTwoNet();
                break;
            case "Fishnet13":
                _initThreeNet();
                break;
            case "Fishnet14":
                _initFourNet();
                break;
            default:
                _initOneNet();
                break;
        }

        foreach (var it in mStillSpriteList)
        {
            it.mStillSprite.onTriggerEnter += _onTriggerEnter;
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var it in mStillSpriteList)
        {
            mScene.getRenderObjectPool().freeStillSprite(it.mStillSprite);
            it.mStillSprite = null;
        }
        mStillSpriteList.Clear();
    }

    EbVector3 mPosition;
    float mAngle;
    float mScale = 1f;

    void _updateView()
    {
        foreach (var it in mStillSpriteList)
        {
            it.mStillSprite.setPosition(mPosition + CLogicUtility.getVector2ByRotateAngle(it.mOffset * mScale, mAngle));
            it.mStillSprite.setDirection(mAngle);
        }
    }

    //-------------------------------------------------------------------------
    public void setPosition(EbVector3 position, float angle)
    {
        mPosition = position;
        mAngle = angle;
        _updateView();
    }

    //-------------------------------------------------------------------------
    public void setScale(float scale)
    {
        foreach (var it in mStillSpriteList)
        {
            it.mStillSprite.setScale(scale);
        }
    }

    //-------------------------------------------------------------------------
    public void setLayer(float layer)
    {
        foreach (var it in mStillSpriteList)
        {
            it.mStillSprite.setLayer(layer);
        }
    }

    //-------------------------------------------------------------------------
    public void setTrigger(bool isTrigger, float size = 1)
    {
        foreach (var it in mStillSpriteList)
        {
            it.mStillSprite.setTrigger(isTrigger, size);
        }
    }

    //-------------------------------------------------------------------------
    public void setColor(UnityEngine.Color color)
    {
        foreach (var it in mStillSpriteList)
        {
            it.mStillSprite.setColor(color);
        }
    }

    //-------------------------------------------------------------------------
    void _initOneNet()
    {
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), EbVector3.Zero));
    }

    //-------------------------------------------------------------------------
    void _initTwoNet()
    {
        float offset = 40f;
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(offset, 0, 0)));
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(-offset, 0, 0)));
    }

    //-------------------------------------------------------------------------
    void _initThreeNet()
    {
        float offset = 44f;
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(0, offset, 0)));

        float x = offset * Mathf.Cos(Mathf.Deg2Rad * 30f);
        float y = offset * Mathf.Sin(Mathf.Deg2Rad * 30f);

        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(-x, -y, 0)));
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(x, -y, 0)));
    }

    //-------------------------------------------------------------------------
    void _initFourNet()
    {
        float offset = 40f;
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(-offset, offset, 0)));
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(offset, offset, 0)));
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(-offset, -offset, 0)));
        mStillSpriteList.Add(new SpriteAndOffset(_newStillSprite(), new EbVector3(offset, -offset, 0)));
    }

    StillSprite _newStillSprite()
    {
        StillSprite still_sprite = mScene.getRenderObjectPool().newStillSprite();
        still_sprite.playAnimation("Fishnet11");
#if UNITY_EDITOR
        still_sprite.gameObject.name = "TkSpriteFishNet";
#endif
        return still_sprite;
    }

    void _onTriggerEnter(Collider other)
    {
        if (onTriggerEnter == null) return;
        onTriggerEnter(other);
    }
}

public class CSpriteFishNet
{
    //-------------------------------------------------------------------------
    float mDelayTime = 0.5f;
    float mCounter = 0;
    uint mPlayerId = 0;
    HashSet<int> mRenderFishID = new HashSet<int>();
    CRenderScene mScene = null;
    CSpriteFishNetGroup mCSpriteFishNetGroup = null;
    int mMaxHitFish = 3;

    //-------------------------------------------------------------------------
    public void create(Color color, uint et_player_rpcid, CRenderFish hit_by_bullet_fish, CRenderScene scene, string animation_name)
    {
        mScene = scene;
        mPlayerId = et_player_rpcid;

        mCSpriteFishNetGroup = new CSpriteFishNetGroup();
        mCSpriteFishNetGroup.create(mScene, color, mPlayerId, animation_name);
        mCSpriteFishNetGroup.setTrigger(true);
        mCSpriteFishNetGroup.setColor(color);
        mCSpriteFishNetGroup.onTriggerEnter += OnTriggerEnter;

        addFishList(hit_by_bullet_fish);
    }

    //-------------------------------------------------------------------------
    void addFishList(CRenderFish fish)
    {
        mRenderFishID.Add(fish.FishObjId);
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        mCounter += Time.deltaTime;
        if (mCounter >= mDelayTime)
        {
            destroy();
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mCSpriteFishNetGroup.onTriggerEnter -= OnTriggerEnter;
        mCSpriteFishNetGroup.destroy();
    }

    //-------------------------------------------------------------------------
    public void setScale(float scale)
    {
        mCSpriteFishNetGroup.setScale(scale);
    }

    //-------------------------------------------------------------------------
    public void setPosition(EbVector3 pos, float angle)
    {
        mCSpriteFishNetGroup.setPosition(pos, angle);
    }

    //-------------------------------------------------------------------------
    public void setLayer(float layer)
    {
        mCSpriteFishNetGroup.setLayer(layer);
    }

    //-------------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (mMaxHitFish <= 0)
        {
            mCSpriteFishNetGroup.setTrigger(false);
            return;
        }
        if ("CSpriteFish" != other.gameObject.tag) return;

        FishStillSprite sprite_fish = other.gameObject.GetComponent<FishStillSprite>();
        CRenderFish render_fish = sprite_fish.getSpriteFish().getRenderFish();
        if (render_fish != null)
        {
            _hitFish(render_fish);
        }
    }

    //-------------------------------------------------------------------------
    void _hitFish(CRenderFish render_fish)
    {
        if (mRenderFishID.Contains(render_fish.FishObjId))
        {
            return;
        }

        mMaxHitFish--;

        addFishList(render_fish);

        if (!render_fish.canHitByFishNet()) return;

        if (mScene.getMyPlayerId() == mPlayerId)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add("playerFishNetHit");
            vec_param.Add(mPlayerId.ToString());
            vec_param.Add(render_fish.FishObjId.ToString());
            mScene.getListener().onSceneRender2Logic(vec_param);
        }
    }
}
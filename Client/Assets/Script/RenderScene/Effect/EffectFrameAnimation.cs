using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using GF.Common;

namespace Ps
{
    public class EffectFrameAnimation : ClientEffectComponent
    {
        //-------------------------------------------------------------------------
        private float mLastTime = 0.0f;
        private string mPicName = string.Empty;
        CSpriteFishNet mCSpriteFishNet = null;
        StillSprite mStillSprite = null;
        float mFunDelayTime = 0;
        float mTimeCounter = 0;
        int vmVibId = 0;
        string mAnimationName = "";

        //-------------------------------------------------------------------------
        public EffectFrameAnimation()
        {
        }

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            base.create(param);
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();

            mScene = (CRenderScene)mMapParam["RenderScene"];

            TbDataEffectFrameAnimation effec_data = EbDataMgr.Instance.getData<TbDataEffectFrameAnimation>(mEffectId);
            mLastTime = effec_data.LastTime / 100.0f;
            buildFrame(effec_data.SourceName, effec_data.Scale / 300.0f, mEffectId);
            mFunDelayTime = mLastTime;
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (mCSpriteFishNet != null)
            {
                mCSpriteFishNet.update(elapsed_tm);
            }

            mTimeCounter += elapsed_tm;
            if (mTimeCounter >= mFunDelayTime)
            {
                signDestroy();
            }
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            if (mHasDestroy) return;
            mHasDestroy = true;
            if (mCSpriteFishNet != null) { mCSpriteFishNet.destroy(); }
            mScene.getRenderObjectPool().freeStillSprite(mStillSprite);

            mStillSprite = null;
            mCSpriteFishNet = null;
        }

        //-------------------------------------------------------------------------
        void buildFrame(string pic_name, float scale, int vib_id)
        {
            mPicName = pic_name;
            vmVibId = vib_id;

            if ((vib_id == 1 || vib_id == 2 || vib_id == 3 || vib_id == 4) && mMapParam.ContainsKey("NetColor"))
            {
                mAnimationName = pic_name;
                mCSpriteFishNet = new CSpriteFishNet();
                Color color = (Color)mMapParam["NetColor"];
                uint et_player_rpcid = (uint)mMapParam["PlayerId"];
                mCSpriteFishNet.create(color, et_player_rpcid, (CRenderFish)mMapParam["BulletHitFish"], mScene, mAnimationName);
                mCSpriteFishNet.setScale(scale);
            }
            else
            {
                mStillSprite = mScene.getRenderObjectPool().newStillSprite();
                mStillSprite.setScale(scale);
                mStillSprite.playAnimation(pic_name);

#if UNITY_EDITOR
                mStillSprite.gameObject.name = "TkSpriteEffectFrameAnimation_" + pic_name;
#endif
            }

            EbVector3 v2 = (EbVector3)mMapParam["SourcePosition"];

            if (mCSpriteFishNet != null)
            {
                mCSpriteFishNet.setPosition(v2, 0);
                mCSpriteFishNet.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.FishNet));
            }
            if (mStillSprite != null)
            {
                mStillSprite.setPosition(v2);
                mStillSprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.FishNet));
            }
        }

        //-------------------------------------------------------------------------
        protected override void signDestroy()
        {
            base.signDestroy();
        }
    }

    public class EffectFrameAnimationFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "FrameAnimation";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Client;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectFrameAnimation();
            return effect;
        }
    }
}

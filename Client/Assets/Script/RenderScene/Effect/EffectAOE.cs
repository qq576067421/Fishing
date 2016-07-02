using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using GF.Common;

namespace Ps
{
    public class EffectAOE : ClientEffectComponent
    {
        //-------------------------------------------------------------------------
        float mTimeCounter = 1f;
        StillParticle mStillParticle = null;
        List<CRenderFish> mDieFishList = new List<CRenderFish>();
        List<CRenderFish> mFishList = new List<CRenderFish>();
        float mAoeSpeed = 250f;
        float mRadius = 0f;
        TbDataEffectAOE mEffectAOEData = null;
        int mCurrentRate = 0;
        uint mPlayerId = 0;
        int mParticleMax = 20;

        //-------------------------------------------------------------------------
        public EffectAOE()
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

            mSourcePosition = (EbVector3)mMapParam["SourcePosition"];
            TbDataEffectAOE effect_data = EbDataMgr.Instance.getData<TbDataEffectAOE>(mEffectId);

            mEffectAOEData = effect_data;

            mCurrentRate = (int)mMapParam["Rate"];
            mPlayerId = (uint)mMapParam["PlayerId"];
            List<string> custom_param_list = (List<string>)mMapParam["EffectCustomParam"];
            foreach (var it in custom_param_list)
            {
                CRenderFish fish = mScene.getLevel().findFish(int.Parse(it));
                if (fish == null) continue;
                mDieFishList.Add(fish);
            }

            mFishList = getFishByPositionAndRadius(mSourcePosition, mEffectAOEData.Radius);

            foreach (var it in mDieFishList)
            {
                mFishList.Remove(it);
            }

            mTimeCounter = 5f;
        }

        //---------------------------------------------------------------------
        List<CRenderFish> getFishByPositionAndRadius(EbVector3 position, float radius)
        {

            List<CRenderFish> fish_list = new List<CRenderFish>();
            List<CRenderFish> scene_fish_list = mScene.getLevel().getAllFish();
            foreach (var it in scene_fish_list)
            {
                if (position.getDistance(it.Position) > radius) continue;
                fish_list.Add(it);
            }

            return fish_list;
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            mTimeCounter -= elapsed_tm;

            if ((mDieFishList.Count > 0 || mFishList.Count > 0) && mEffectAOEData.ParticleName.Id > 0)
            {
                List<CRenderFish> destroy_list = new List<CRenderFish>();

                mRadius += mAoeSpeed * elapsed_tm;

                foreach (var it in mDieFishList)
                {
                    float distance = mSourcePosition.getDistance(it.Position);
                    if (distance <= mRadius)
                    {
                        _newParticle(it.Position, it.FishVibId);
                        it.dieWithRate(mPlayerId, mCurrentRate);
                        destroy_list.Add(it);
                    }
                }

                foreach (var it in destroy_list)
                {
                    mDieFishList.Remove(it);
                }
                destroy_list.Clear();

                foreach (var it in mFishList)
                {
                    float distance = mSourcePosition.getDistance(it.Position);
                    if (distance <= mRadius)
                    {
                        _newParticle(it.Position, it.FishVibId);
                        destroy_list.Add(it);
                    }
                }

                foreach (var it in destroy_list)
                {
                    mFishList.Remove(it);
                }
            }


            if (mTimeCounter < 0)
            {
                signDestroy();
            }
        }

        //-------------------------------------------------------------------------
        void _newParticle(EbVector3 position, int fish_vib_id)
        {
            if (mParticleMax < 0) return;
            --mParticleMax;
            mStillParticle = mScene.getParticlemanager().newParticle(mEffectAOEData.ParticleName.ParticlePrefabName);
            mStillParticle.setPosition(position);
            mStillParticle.setLayer(mScene.getLayerAlloter().getFishLayer(fish_vib_id));
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            mScene.getParticlemanager().freeParticle(mStillParticle);
        }
    }


    public class EffectAOEFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "EffectAOE";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server2Client;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectAOE();
            return effect;
        }
    }
}

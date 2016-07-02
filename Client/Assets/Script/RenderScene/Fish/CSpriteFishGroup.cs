using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;
using UnityEngine;

namespace Ps
{
    public class CSpriteFishGroup : ISpriteFish
    {
        //---------------------------------------------------------------------
        public UnityEngine.GameObject FishGameObject
        {
            get
            {
                foreach (var it in mFishs)
                {
                    return it.gameObject;
                }
                return null;
            }
        }

        //---------------------------------------------------------------------
        List<FishStillSprite> mFishs = new List<FishStillSprite>();
        CSpriteRedBottom mRedBottom = null;
        CRenderScene mScene = null;
        TbDataFish mVibFishData = null;
        CRenderFish mRenderFish = null;
        bool mIsSpriteDestroy = false;
        float mCount = 0;
        bool mIsPlayingDieAnimation = false;
        int mFishNumber = 1;
        bool mIsRotation = false;
        bool mDestroy = false;
        float mFishCycleGap = 0.00005f;
        EbVector3 mPosition;
        float mAngle;
        float mWidth = 70;
        float mScale = 1.2f;
        bool mIsDieAnimation = false;
        MassEntity mMassEntity = null;
        UnityEngine.Color mInitColor = UnityEngine.Color.white;

        //---------------------------------------------------------------------
        public CSpriteFishGroup(int number)
        {
            mFishNumber = number;
        }

        //---------------------------------------------------------------------
        public void create(CRenderScene scene, TbDataFish vib_fish_data)
        {
            mScene = scene;
            mVibFishData = vib_fish_data;
            mFishCycleGap = mScene.getLayerAlloter().EachFishGap / 10f;

            bool has_cycle = !string.IsNullOrEmpty(mVibFishData.CycleAnimationName);

            for (int i = 0; i < mFishNumber; i++)
            {
                mFishs.Add(mScene.getRenderObjectPool().newFishStillSprite());
                if (has_cycle)
                {
                    mRedBottom = new CSpriteRedBottom();
                    mRedBottom.create(mScene, this, mVibFishData);
                }
            }

            foreach (var it in mFishs)
            {
                it.init(this, scene);
            }

            float fish_scale = ((float)mVibFishData.FishHeight / (float)mVibFishData.FishPixelHeight);

            foreach (var it in mFishs)
            {
                it.setScale(0.7f * fish_scale);
            }

            playAnimation(mVibFishData.FishAnimMove);

            if (mVibFishData.Red == TbDataFish.IsRed.YES)
            {
                mInitColor = new Color(1, 0, 0);
                resetColor(1);
            }
        }

        //---------------------------------------------------------------------
        public void resetColor(float scale)
        {
            setColor(mInitColor);
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            if (mDestroy) return;
            mDestroy = true;

            foreach (var it in mFishs)
            {
                mScene.getRenderObjectPool().freeFishStillSprite(it);
            }
            mFishs.Clear();

            if (mRedBottom != null)
            {
                mRedBottom.destroy();
            }
        }

        //---------------------------------------------------------------------
        public void setScale(float scale)
        {
            foreach (var it in mFishs)
            {
                it.setScale(0.7f * scale);
            }

            if (mRedBottom != null)
            {
                mRedBottom.setScale(scale);
            }
        }

        //---------------------------------------------------------------------
        public void setLayer(float layer)
        {
            foreach (var it in mFishs)
            {
                it.setLayer(layer);
            }

            if (mRedBottom != null)
            {
                mRedBottom.setLayer(layer);
            }
        }

        //---------------------------------------------------------------------
        public void setPosition(EbVector3 position, float angle)
        {
            mPosition = position;
            if (mIsRotation) return;
            mAngle = angle;
            updatePosition();
        }

        //---------------------------------------------------------------------
        public void setDirection(float angle)
        {
            mAngle = angle;
            updatePosition();
        }

        //---------------------------------------------------------------------
        void updatePosition()
        {
            float clip = mVibFishData.CycleHeight;

            float offset = 0f;

            if (isOdd(mFishNumber))
            {
                offset = (int)(((float)mFishNumber - 1f) / 2f);
            }
            else
            {
                offset = mFishNumber / 2 - 0.5f;
            }

            int index = 0;
            foreach (var it in mFishs)
            {
                it.setPosition(mPosition + new EbVector3(CLogicUtility.getDirection(mAngle + 90).x, CLogicUtility.getDirection(mAngle + 90).y, CLogicUtility.getDirection(mAngle + 90).z) * (index - offset) * clip);
                it.setDirection(mAngle);
                index++;
            }

            if (mRedBottom != null)
            {
                mRedBottom.setPosition(mPosition, mAngle);
            }
        }

        //---------------------------------------------------------------------
        bool isOdd(int number)
        {
            return number % 2 == 1;
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            if (mIsRotation)
            {
                mAngle += 60 * elapsed_tm;
                updatePosition();
            }
            else
            {
                if (mIsPlayingDieAnimation)
                {
                    _updateDieAnimation(UnityEngine.Time.deltaTime);
                }
            }
            if (mIsFadeout)
            {
                mAlpha -= elapsed_tm * mAlphaSpeed;
                foreach (var it in mFishs)
                {
                    it.setAlpha(mAlpha);
                }
            }
            if (mRedBottom != null)
            {
                mRedBottom.update(elapsed_tm);
            }
        }

        //---------------------------------------------------------------------
        void _updateDieAnimation(float elapsed_tm)
        {
            if (mIsDieAnimation)
            {
                mCount -= elapsed_tm;
                if (mCount <= 0)
                {
                    _signDestroy();
                }
                return;
            }

            if (mMassEntity == null)
            {
                mIsDieAnimation = true;
                return;
            }
            mMassEntity.update(elapsed_tm);
            setPosition(mMassEntity.Position, mRenderFish.getFishAngle());

            if (mMassEntity.IsEndRoute && !mIsDieAnimation)
            {
                setScale(mScale * (float)mVibFishData.FishHeight / (float)mVibFishData.FishPixelHeight);
                playAnimation(mVibFishData.FishAnimDie);
                mIsDieAnimation = true;
            }
        }

        //---------------------------------------------------------------------
        void _signDestroy()
        {
            mIsSpriteDestroy = true;
            mRenderFish.signDestroy();
            mIsPlayingDieAnimation = false;
        }

        bool mIsFadeout = false;
        float mAlpha = 1;
        float mAlphaSpeed;

        //---------------------------------------------------------------------
        public void playDieAnimation()
        {
            mIsPlayingDieAnimation = true;

            foreach (var it in mFishs)
            {
                it.setTrigger(false);
            }

            if (mRedBottom != null)
            {
                mRedBottom.setTrigger(false);
            }
            mCount = 1.5f;

            if (mVibFishData.IsFullScreenAnimation == TbDataFish.IsFullScreen.YES)
            {
                mIsFadeout = true;
                mCount = 2;
                mAlphaSpeed = mAlpha / mCount;
            }

            if (mVibFishData.FishDieType == TbDataFish.FishDieTypeEnum.BigFish)
            {
                playAnimation(mVibFishData.FishAnimDie);
            }
            else
            {
                _initDieRoute();
            }
        }

        //---------------------------------------------------------------------
        public void playAnimation(string name)
        {
            foreach (var it in mFishs)
            {
                it.playAnimation(name);
            }
        }

        //---------------------------------------------------------------------
        public void stopAnimation()
        {
            foreach (var it in mFishs)
            {
                it.stopAnimation();
            }
        }

        //---------------------------------------------------------------------
        public void setTrigger(bool isTrigger, float size = 1)
        {
            foreach (var it in mFishs)
            {
                it.setTrigger(isTrigger, size);
            }

            if (mRedBottom != null)
            {
                mRedBottom.setTrigger(isTrigger);
            }
        }

        //---------------------------------------------------------------------
        public void setColor(UnityEngine.Color color)
        {
            foreach (var it in mFishs)
            {
                it.setColor(color);
            }
        }

        //---------------------------------------------------------------------
        public void setAlpha(float alpha)
        {
            foreach (var it in mFishs)
            {
                it.setAlpha(alpha);
            }

            if (mRedBottom != null)
            {
                mRedBottom.setAlpha(alpha);
            }
        }

        //---------------------------------------------------------------------
        public void setTag(string tag)
        {
            foreach (var it in mFishs)
            {
                it.setTag(tag);
            }
            if (mRedBottom != null)
            {
                mRedBottom.setTag(tag);
            }
        }

        //---------------------------------------------------------------------
        public void setRenderFish(CRenderFish render_fish)
        {
            mRenderFish = render_fish;
        }

        //---------------------------------------------------------------------
        public CRenderFish getRenderFish()
        {
            return mRenderFish;
        }

        //---------------------------------------------------------------------
        public void playRotationAnimation()
        {
            mIsRotation = true;
        }

        //---------------------------------------------------------------------
        public bool hasFishStillSprite(FishStillSprite fish_still_sprite)
        {
            if (fish_still_sprite == null) return false;
            foreach (var it in mFishs)
            {
                if (it == fish_still_sprite)
                {
                    return true;
                }
            }

            if (mRedBottom != null)
            {
                mRedBottom.hasFishStillSprite(fish_still_sprite);
            }

            return false;
        }

        //---------------------------------------------------------------------
        public void setGameobjectName(string name)
        {
            foreach (var it in mFishs)
            {
                it.gameObject.name = name;
            }

            if (mRedBottom != null)
            {
                mRedBottom.setGameobjectName(name);
            }
        }

        //---------------------------------------------------------------------
        void _initDieRoute()
        {
            mMassEntity = new MassEntity();
            RouteParaCurve route = new RouteParaCurve();
            route.create(mRenderFish.Position, 0, 0.6f, mRenderFish.jumpDistance());
            mMassEntity.setRoute(route);
        }
    }
}

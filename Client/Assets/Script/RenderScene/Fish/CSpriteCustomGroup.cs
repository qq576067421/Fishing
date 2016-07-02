using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CSpriteCustomGroup : ISpriteFish
    {
        class FishSpriteAndOffset
        {
            public FishStillSprite mFishStillSprite;
            public EbVector3 mOffset;
            public string mDieAnimationName;

            public FishSpriteAndOffset(FishStillSprite fish, EbVector3 offset, string die_animation_name)
            {
                mFishStillSprite = fish;
                mOffset = offset;
                mDieAnimationName = die_animation_name;
            }
        }

        //-------------------------------------------------------------------------
        public UnityEngine.GameObject FishGameObject
        {
            get
            {
                foreach (var it in mFishs)
                {
                    return it.mFishStillSprite.gameObject;
                }
                return null;
            }
        }

        List<FishSpriteAndOffset> mFishs = new List<FishSpriteAndOffset>();
        FishStillSprite mCycle = null;
        CRenderScene mScene = null;
        TbDataFish mVibFishData = null;
        CRenderFish mRenderFish = null;
        float mCount = 0;
        bool mIsPlayingDieAnimation = false;
        bool mIsRotation = false;
        bool mDestroy = false;
        float mScale = 1f;
        float mFixFishScale = 0.7f;
        float mFishCycleGap = 0.00005f;
        UnityEngine.Color mInitColor = UnityEngine.Color.white;

        public CSpriteCustomGroup()
        {
        }

        //-------------------------------------------------------------------------
        public void create(CRenderScene scene, TbDataFish vib_fish_data)
        {
            mScene = scene;
            mVibFishData = vib_fish_data;
            mFishCycleGap = mScene.getLayerAlloter().EachFishGap / 10f;

            if (mVibFishData.Type == TbDataFish.FishType.Custom)
            {
                mCycle = mScene.getRenderObjectPool().newFishStillSprite();
                mCycle.init(this, scene);
                mCycle.playAnimation(mVibFishData.FishCompose.BackGroundAniName);
                mCycle.setScale((float)mVibFishData.FishCompose.BackGroundAnimationScale / 100.0f);
                foreach (var it in mVibFishData.FishCompose.FishComposes)
                {
                    if (null != it && it.Id > 0)
                    {
                        TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(it.FishVibId);
                        FishStillSprite fish_sprite = mScene.getRenderObjectPool().newFishStillSprite();

                        fish_sprite.init(this, scene);
                        fish_sprite.playAnimation(fish_data.FishAnimMove);
                        fish_sprite.setScale(mFixFishScale * (float)fish_data.FishHeight / (float)fish_data.FishPixelHeight);

                        mFishs.Add(new FishSpriteAndOffset(
                            fish_sprite,
                            new EbVector3(it.OffsetX, it.OffsetY, 0),
                            fish_data.FishAnimDie));
                    }
                }
            }
        }

        //-------------------------------------------------------------------------
        public void resetColor(float scale)
        {
            setColor(mInitColor);
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            if (mDestroy) return;
            mDestroy = true;
            foreach (var it in mFishs)
            {
                mScene.getRenderObjectPool().freeFishStillSprite(it.mFishStillSprite);
                it.mFishStillSprite = null;
            }
            mFishs.Clear();

            mScene.getRenderObjectPool().freeFishStillSprite(mCycle);
        }

        //-------------------------------------------------------------------------
        public void setScale(float scale)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setScale(mFixFishScale * scale);
            }

            mCycle.setScale(mFixFishScale * scale);

            mScale = mFixFishScale * scale;

            _updateView();
        }

        //-------------------------------------------------------------------------
        public void setLayer(float layer)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setLayer(layer);
            }

            mCycle.setLayer(layer + mFishCycleGap);
        }

        EbVector3 mPosition;
        float mAngle;
        float mWidth = 70;
        //-------------------------------------------------------------------------
        public void setPosition(EbVector3 position, float angle)
        {
            mPosition = position;
            if (mIsRotation) return;
            mAngle = angle;
            _updateView();
        }

        void _updateView()
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setPosition(mPosition + CLogicUtility.getVector2ByRotateAngle(it.mOffset * mScale, mAngle));
                it.mFishStillSprite.setDirection(mAngle);
            }
            mCycle.setPosition(mPosition);
            mCycle.setDirection(mAngle);
        }

        //-------------------------------------------------------------------------
        bool isOdd(int number)
        {
            return number % 2 == 1;
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            if (mIsRotation)
            {
                mAngle += 60 * UnityEngine.Time.deltaTime;//不受时间因子影响
                _updateView();
            }
            else
            {
                if (mIsPlayingDieAnimation)
                {
                    mCount -= UnityEngine.Time.deltaTime;//不受时间因子影响
                    if (mCount <= 0)
                    {
                        mRenderFish.signDestroy();
                        mIsPlayingDieAnimation = false;
                    }
                }
            }
        }

        public void playDieAnimation()
        {
            //playAnimation(mVibFishData.FishAnimDie);

            foreach (var it in mFishs)
            {
                playAnimation(it.mDieAnimationName);
            }

            mIsPlayingDieAnimation = true;

            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setTrigger(false);
            }

            mCycle.setTrigger(false);

            mCount = 2f;
        }

        //-------------------------------------------------------------------------
        public void playAnimation(string name)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.playAnimation(name);
            }
        }

        //-------------------------------------------------------------------------
        public void stopAnimation()
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.stopAnimation();
            }

            mCycle.stopAnimation();
        }

        //-------------------------------------------------------------------------
        public void setTrigger(bool isTrigger, float size = 1)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setTrigger(isTrigger, size);
            }

            mCycle.setTrigger(isTrigger, size);
        }

        //-------------------------------------------------------------------------
        public void setColor(UnityEngine.Color color)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setColor(color);
            }

            //foreach (var it in mCycles)
            //{
            //    it.setTrigger(false);
            //}
        }

        //-------------------------------------------------------------------------
        public void setAlpha(float alpha)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setAlpha(alpha);
            }

            mCycle.setAlpha(alpha);
        }

        //-------------------------------------------------------------------------
        public void setTag(string tag)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setTag(tag);
            }

            mCycle.setTag(tag);
        }

        //-------------------------------------------------------------------------
        public void setRenderFish(CRenderFish render_fish)
        {
            mRenderFish = render_fish;
        }

        //-------------------------------------------------------------------------
        public CRenderFish getRenderFish()
        {
            return mRenderFish;
        }

        //-------------------------------------------------------------------------
        public void playRotationAnimation()
        {
            mIsRotation = true;
        }

        //-------------------------------------------------------------------------
        public bool hasFishStillSprite(FishStillSprite fish_still_sprite)
        {
            if (fish_still_sprite == null) return false;
            foreach (var it in mFishs)
            {
                if (it.mFishStillSprite == fish_still_sprite)
                {
                    return true;
                }
            }
            return mCycle == fish_still_sprite;
        }

        //-------------------------------------------------------------------------
        public void setGameobjectName(string name)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.gameObject.name = name;
            }
            mCycle.gameObject.name = name;
        }

        //-------------------------------------------------------------------------
        public void setDirection(float angle)
        {
            mAngle = angle;
            _updateView();
        }
    }
}

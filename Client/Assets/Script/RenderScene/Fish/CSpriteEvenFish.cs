using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CSpriteEvenFish : ISpriteFish
    {
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

        List<SpriteAndOffset> mFishs = new List<SpriteAndOffset>();
        protected List<SpriteAndOffset> mBackground = new List<SpriteAndOffset>();


        protected CRenderScene mScene = null;
        protected TbDataFish mVibFishData = null;
        protected CRenderFish mRenderFish = null;
        protected float mFishCycleGap = 0.00005f;
        protected float mFixFishScale = 0.7f;

        protected EbVector3 mPosition;
        protected float mAngle;
        protected float mWidth = 70;

        protected float mCount = 0;
        protected bool mIsPlayingDieAnimation = false;
        protected bool mIsRotation = false;
        protected bool mDestroy = false;
        protected float mScale = 1f;
        UnityEngine.Color mInitColor = UnityEngine.Color.white;

        //-------------------------------------------------------------------------
        public CSpriteEvenFish()
        {
        }

        //-------------------------------------------------------------------------
        public virtual void create(CRenderScene scene, TbDataFish vib_fish_data)
        {
            mScene = scene;
            mVibFishData = vib_fish_data;
            mFishCycleGap = mScene.getLayerAlloter().EachFishGap / 10f;
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

            foreach (var it in mBackground)
            {
                mScene.getRenderObjectPool().freeFishStillSprite(it.mFishStillSprite);
                it.mFishStillSprite = null;
            }
            mBackground.Clear();
        }

        //-------------------------------------------------------------------------
        public void setScale(float scale)
        {
            _setFishSize(scale);
            _setBackgroundSize(scale);
        }

        //-------------------------------------------------------------------------
        void _setFishSize(float scale)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setScale(mFixFishScale * scale);
            }
        }

        //-------------------------------------------------------------------------
        protected void _setBackgroundSize(float scale)
        {
            mScale = scale;
            foreach (var it in mBackground)
            {
                it.mFishStillSprite.setScale(scale);
            }
            _updateView();
        }

        //-------------------------------------------------------------------------
        public void setLayer(float layer)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setLayer(layer + it.mLayerOffset);
            }
            foreach (var it in mBackground)
            {
                it.mFishStillSprite.setLayer(layer + it.mLayerOffset);
            }
        }


        //-------------------------------------------------------------------------
        public void setPosition(EbVector3 position, float angle)
        {
            mPosition = position;
            if (mIsRotation) return;
            mAngle = angle;
            _updateView();
        }

        //-------------------------------------------------------------------------
        public void setDirection(float angle)
        {
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
            foreach (var it in mBackground)
            {
                it.mFishStillSprite.setPosition(mPosition + CLogicUtility.getVector2ByRotateAngle(it.mOffset * mScale, mAngle));
            }
        }

        //-------------------------------------------------------------------------
        bool isOdd(int number)
        {
            return number % 2 == 1;
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            foreach (var it in mBackground)
            {
                it.update(elapsed_tm);
            }

            _updateDieAnimation(UnityEngine.Time.deltaTime);
        }

        //-------------------------------------------------------------------------
        void _updateDieAnimation(float elapsed_tm)
        {
            if (mIsRotation)
            {
                mAngle += 60 * elapsed_tm;
                _updateView();
            }
            else
            {
                if (mIsPlayingDieAnimation)
                {
                    mCount -= elapsed_tm;
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

            setTrigger(false);

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
        }

        //-------------------------------------------------------------------------
        public void setTrigger(bool isTrigger, float size = 1)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setTrigger(isTrigger, size);
            }

            foreach (var it in mBackground)
            {
                it.mFishStillSprite.setTrigger(isTrigger, size);
            }
        }

        //-------------------------------------------------------------------------
        public void setColor(UnityEngine.Color color)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setColor(color);
            }
        }

        //-------------------------------------------------------------------------
        public void setAlpha(float alpha)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setAlpha(alpha);
            }
            foreach (var it in mBackground)
            {
                it.mFishStillSprite.setAlpha(alpha);
            }
        }

        //-------------------------------------------------------------------------
        public void setTag(string tag)
        {
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.setTag(tag);
            }
            foreach (var it in mBackground)
            {
                it.mFishStillSprite.setTag(tag);
            }
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
            foreach (var it in mBackground)
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
            foreach (var it in mFishs)
            {
                it.mFishStillSprite.gameObject.name = name;
            }

            foreach (var it in mBackground)
            {
                it.mFishStillSprite.gameObject.name = name;
            }
        }

        //-------------------------------------------------------------------------
        protected void _newSpriteAndOffset(string layer0animation, string layer1animation, TbDataFish.FishIdAndScaleStruct fish_id_scale, EbVector3 position, float layer_offset, float angle_speed)
        {
            mBackground.Add(new SpriteAndOffset(_loadFishStillSprite(layer0animation), position, -angle_speed, mFishCycleGap * 2 + layer_offset));
            mBackground.Add(new SpriteAndOffset(_loadFishStillSprite(layer1animation), position, angle_speed, mFishCycleGap + layer_offset));
            mFishs.Add(new SpriteAndOffset(_loadFishStillSpriteByFishId(fish_id_scale), position, 0, 0, EbDataMgr.Instance.getData<TbDataFish>(fish_id_scale.FishId).FishAnimDie));
        }

        //-------------------------------------------------------------------------
        protected FishStillSprite _loadFishStillSpriteByFishId(TbDataFish.FishIdAndScaleStruct fish_id_scale)
        {
            TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(fish_id_scale.FishId);
            FishStillSprite fish_sprite = _loadFishStillSprite(fish_data.FishAnimMove);
            fish_sprite.setScale(((float)fish_id_scale.Scale / 100f) * mFixFishScale * (float)fish_data.FishHeight / (float)fish_data.FishPixelHeight);
            return fish_sprite;
        }

        //-------------------------------------------------------------------------
        protected FishStillSprite _loadFishStillSprite(string animation_name, float scale = 1f)
        {
            FishStillSprite fish_sprite = mScene.getRenderObjectPool().newFishStillSprite();
            fish_sprite.init(this, mScene);
            fish_sprite.setScale(scale);
            fish_sprite.playAnimation(animation_name);
            return fish_sprite;
        }
    }
}

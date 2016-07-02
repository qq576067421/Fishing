using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;

namespace Ps
{
    public class LinkLockedFishFeature
    {
        CRenderTurret mCRenderTurret = null;
        List<StillSprite> mRectangle = new List<StillSprite>();
        StillSprite mArrow = null;
        StillSprite mNumber = null;
        bool mIsDisplaying = false;
        CRenderScene mScene = null;

        float mRectangleGap = 25f;
        float mLayer;

        public LinkLockedFishFeature(CRenderScene scene, CRenderTurret turret)
        {
            mCRenderTurret = turret;
            mScene = scene;

            mLayer = mScene.getLayerAlloter().getLayer(_eLevelLayer.LinkLockedFish);

            mArrow = mScene.getRenderObjectPool().newStillSprite();
            mArrow.setScale(0.5f);
            mArrow.playAnimation("locking_line_array");
            mArrow.setColor(mCRenderTurret.getTurretColor());
            mArrow.setLayer(mLayer);

            mNumber = mScene.getRenderObjectPool().newStillSprite();
            mNumber.setScale(1f);
            mNumber.playAnimation("locking_line_target_" + mCRenderTurret.getTurretId());
            mNumber.setColor(mCRenderTurret.getTurretColor());

            mNumber.setLayer(mLayer);

            setActive(false);

#if UNITY_EDITOR
            mArrow.gameObject.name = "TkSpriteLinkLocked_locking_line_array";
            mNumber.gameObject.name = "TkSpriteLinkLocked_locking_line_target_" + mCRenderTurret.getTurretId();
#endif
        }

        void newRectangleSprite()
        {
            StillSprite sprite = mScene.getRenderObjectPool().newStillSprite();
            sprite.playAnimation("locking_line_rectangle");
            sprite.setColor(mCRenderTurret.getTurretColor());
            sprite.setScale(0.5f);
            sprite.setLayer(mLayer);
            mRectangle.Add(sprite);

#if UNITY_EDITOR
            sprite.gameObject.name = "TkSpriteLinkLocked_locking_line_rectangle";
#endif
        }

        public void display(CRenderFish fish)
        {
            mIsDisplaying = true;
            setActive(true);

            updateView(fish);
        }

        public void hide()
        {
            mIsDisplaying = false;
            //destroyRectangle();
            setActive(false);
        }

        void setActive(bool active)
        {
            foreach (var it in mRectangle)
            {
                it.setActive(active);
            }
            mArrow.setActive(active);
            mNumber.setActive(active);
        }

        public void update(float elapsed_tm)
        {
            if (!mIsDisplaying) return;

            CRenderFish fish = mCRenderTurret.getLockFish();
            if (fish == null)
            {
                hide();
                return;
            }

            updateView(fish);
        }

        void updateView(CRenderFish fish)
        {
            EbVector3 turret_position = mCRenderTurret.getTurretPos();
            EbVector3 fish_position = fish.Position;

            float turret_fish_distance = turret_position.getDistance(fish_position);

            int rectangle_count = (int)(turret_fish_distance / mRectangleGap);
            if (rectangle_count < 0) rectangle_count = 0;
            if (mRectangle.Count > rectangle_count)
            {
                int remove_count = mRectangle.Count - rectangle_count;

                do
                {
                    mScene.getRenderObjectPool().freeStillSprite(mRectangle[0]);
                    mRectangle.RemoveAt(0);
                    --remove_count;
                } while (remove_count > 0);
            }
            else if (mRectangle.Count < rectangle_count)
            {
                int add_count = rectangle_count - mRectangle.Count;
                do
                {
                    newRectangleSprite();
                    --add_count;
                } while (add_count > 0);
            }


            float angle = CLogicUtility.getAngle(fish_position - turret_position);

            float node_count = mRectangle.Count + 2;

            for (int i = 0; i < mRectangle.Count; i++)
            {
                mRectangle[i].setPosition(EbVector3.lerp(turret_position, fish_position, ((float)i + 1) / node_count));
                mRectangle[i].setDirection(angle);
            }

            mArrow.setPosition(EbVector3.lerp(turret_position, fish_position, (node_count - 1) / node_count));
            mArrow.setDirection(angle);

            mNumber.setPosition(fish_position);
        }

        void destroyRectangle()
        {
            foreach (var it in mRectangle)
            {
                mScene.getRenderObjectPool().freeStillSprite(it);
            }
            mRectangle.Clear();
        }

        public void destroy()
        {
            destroyRectangle();
            mScene.getRenderObjectPool().freeStillSprite(mArrow);
            mScene.getRenderObjectPool().freeStillSprite(mNumber);

            mArrow = null;
            mNumber = null;
        }
    }
}

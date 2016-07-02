using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderBufferLock : CRenderBuffer
    {
        //-------------------------------------------------------------------------
        ISpriteFish mSpriteFish = null;
        float mFishSize = 100f;
        int mLockFishObjId = -1;

        //-------------------------------------------------------------------------
        public CRenderBufferLock(CRenderScene scene, CRenderTurret turret, string name, List<object> param, string animation_name)
            : base(scene, turret, name, param, animation_name)
        {
            mScene = scene;
            resetLockedFishObjId((int)param[0]);
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (canDestroy()) return;

            if (mSpriteFish != null)
            {
                mSpriteFish.update(elapsed_tm);
            }

            CRenderLevel level = mScene.getLevel();
            if (level == null)
            {
                signDestroy();
                return;
            }

            bool level_isrun = level.isNormal();
            if (!level_isrun)
            {
                signDestroy();
                return;
            }

            CRenderFish fish = mScene.getLevel().getFishByObjId(mLockFishObjId);
            if (fish == null || fish.IsDie || !_isInScene(fish.Position))
            {
                mLockFishObjId = -1;
                signDestroy();
                return;
            }

            mTurret.updateTurretAngle(fish.Position);
        }

        //---------------------------------------------------------------------
        bool _isInScene(EbVector3 position, float border_size = 0)
        {
            return (position.x >= -(CCoordinate.LogicSceneLength / 2 + border_size)
                && position.x <= (CCoordinate.LogicSceneLength / 2 + border_size)
                && position.y >= -(CCoordinate.LogicSceneWidth / 2 + border_size)
                && position.y <= CCoordinate.LogicSceneWidth / 2 + border_size);
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();

            mLockFishObjId = -1;

            if (mSpriteFish != null)
            {
                mSpriteFish.destroy();
                mSpriteFish = null;
            }

            if (_isMe())
            {
                mScene.getProtocol().c2sUnlockFish(mScene.getMyPlayerId());
            }
        }

        //-------------------------------------------------------------------------
        public override string getName()
        {
            return "BufLock";
        }

        //-------------------------------------------------------------------------
        public override void setPosition(EbVector3 position, float angle)
        {
            base.setPosition(position, angle);

            if (mSpriteFish != null)
            {
                mSpriteFish.setPosition(position, angle);
            }
        }

        //-------------------------------------------------------------------------
        public override void onTouch(GameObject buffer)
        {
            if (!_isMe()) return;

            base.onTouch(buffer);

            FishStillSprite fish_still_sprite = buffer.GetComponent<FishStillSprite>();

            if ((mSprite != null && mSprite.gameObject == buffer) ||
                (mSpriteFish != null && mSpriteFish.hasFishStillSprite(fish_still_sprite)))
            {
                mLockFishObjId = -1;
                signDestroy();
            }
        }

        //-------------------------------------------------------------------------
        public int getLockFishObjId()
        {
            return mLockFishObjId;
        }

        //-------------------------------------------------------------------------
        public void resetLockedFishObjId(int lock_fish_objid)
        {
            if (mSpriteFish != null)
            {
                mSpriteFish.destroy();
                mSpriteFish = null;
            }

            mLockFishObjId = lock_fish_objid;

            CRenderFish fish = mScene.getLevel().findFish(mLockFishObjId);
            if (fish != null && !fish.IsDie)
            {
                int fish_vib_id = fish.FishVibId;
                string tag = "CSpriteBuffer" + mTurret.getTurretId().ToString();
                mSpriteFish = mScene.getSpriteFishFactory().buildSpriteFish(null, fish_vib_id);
                mSpriteFish.setTag(tag);
                mSpriteFish.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.BufferLockFish));
                if (EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id).Red == TbDataFish.IsRed.YES)
                {
                    mSpriteFish.setColor(new Color(1, 0, 0));
                }
                else
                {
                    mSpriteFish.setColor(new Color(1, 1, 1));
                }

                mSpriteFish.setScale(EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id).getLockCardFishScale());
                mSpriteFish.setTrigger(true);

                mTurret.displayLinkFish(fish);
            }

            if (_isMe())
            {
                mScene.getProtocol().c2sLockFish(mScene.getMyPlayerId(), lock_fish_objid);
            }
        }

        //-------------------------------------------------------------------------
        bool _isMe()
        {
            return (mTurret.getScenePlayerInfo().et_player_rpcid == mScene.getMyPlayerId());
        }
    }
}

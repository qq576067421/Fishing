using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderBullet : IDisposable
    {
        //-------------------------------------------------------------------------
        CRenderScene mScene = null;
        CSpriteBullet mSpriteBullet = null;
        TbDataBullet mVibBullet = null;
        uint mPlayerId;
        int mBulletObjId = 0;
        float mfAngle = 90.0f;
        EbVector3 mPos;
        float mfBulletSpeed = 1200.0f;// 子弹速度（米/秒）
        string mBulletAnim = string.Empty;
        int mLockedFishObjid = -1;
        int mBulletRate = 1;// 子弹倍率
        bool mIsSignDestroy = false;

        //-------------------------------------------------------------------------
        public CRenderBullet(CRenderScene render_scene)
        {
            mScene = render_scene;
        }

        //-------------------------------------------------------------------------
        ~CRenderBullet()
        {
            this.Dispose(false);
        }

        //-------------------------------------------------------------------------
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        //-------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            destroy();
        }

        //-------------------------------------------------------------------------
        public void create(uint et_player_rpcid, int bullet_objid, int bullet_vibid, float bullet_speed,
            int locked_fish_obj_id, EbVector3 turret_pos, float turret_angle, TbDataParticle hit_particle)
        {
            // vib
            mPlayerId = et_player_rpcid;
            mVibBullet = EbDataMgr.Instance.getData<TbDataBullet>(bullet_vibid);
            mPos = turret_pos;
            mfAngle = turret_angle;
            mBulletObjId = bullet_objid;
            mLockedFishObjid = locked_fish_obj_id;
            //mfBulletSpeed = mVibBullet.Speed;
            mfBulletSpeed = bullet_speed;

            // sprite bullet
            CRenderTurret turret = mScene.getTurret(mPlayerId);
            int turret_id = turret.getTurretId();
            string bullet_anim = mVibBullet.Bullet0Animation;
            if (turret.isPower())
            {
                bullet_anim = mVibBullet.ColorBulletAnimation;
            }
            else
            {
                switch (turret_id)
                {
                    case 1:
                        bullet_anim = mVibBullet.Bullet1Animation;
                        break;
                    case 2:
                        bullet_anim = mVibBullet.Bullet2Animation;
                        break;
                    case 3:
                        bullet_anim = mVibBullet.Bullet3Animation;
                        break;
                    case 4:
                        bullet_anim = mVibBullet.Bullet4Animation;
                        break;
                    case 5:
                        bullet_anim = mVibBullet.Bullet5Animation;
                        break;
                    case 6:
                        bullet_anim = mVibBullet.Bullet6Animation;
                        break;
                }
            }
            mBulletAnim = bullet_anim;
            mSpriteBullet = new CSpriteBullet();
            mSpriteBullet.create(mScene, this, mBulletAnim, hit_particle);
            mSpriteBullet.setTrigger(true, 200);

            mSpriteBullet.setPosition(mPos);
            mSpriteBullet.setDirection(mfAngle);
            mSpriteBullet.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.Bullet));
            mSpriteBullet.setScale((float)mVibBullet.BulletHeight / (float)mVibBullet.BulletPixelHeight);
            mScene.getLevel().addBullet(this);
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            mSpriteBullet.destroy();
            mSpriteBullet = null;
        }

        //-------------------------------------------------------------------------
        public bool isPlayerBullet()
        {
            return mScene.getMyPlayerId() == mPlayerId;
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            if (mIsSignDestroy) return;

            CRenderFish fish = getLockFish();
            if (fish == null || fish.IsDie)
            {
                EbVector3 cur_pos = CLogicUtility.getCurrentPos(mPos, mfAngle, mfBulletSpeed, elapsed_tm);
                mPos = cur_pos;
                mSpriteBullet.setPosition(mPos);

                mSpriteBullet.setDirection(mfAngle);
            }
            else
            {
                float angle = CLogicUtility.getAngle(fish.Position - mPos);
                mPos = CLogicUtility.getCurrentPos(mPos, angle, mfBulletSpeed, elapsed_tm);

                mSpriteBullet.setPosition(mPos);
                mSpriteBullet.setDirection(angle);
            }

            if (mScene.getSceneBox().check(ref mPos, ref mfAngle))
            {
                mLockedFishObjid = -1;
                mSpriteBullet.setPosition(mPos);
                mSpriteBullet.setDirection(mfAngle);
            }
        }

        //-------------------------------------------------------------------------
        public EbVector3 getPosition()
        {
            return mPos;
        }

        //-------------------------------------------------------------------------
        public void signDestroy()
        {
            mIsSignDestroy = true;
            mScene.getLevel().signDestroyBullet(mPlayerId, mBulletObjId);
        }

        //-------------------------------------------------------------------------
        public void setColor(Color c)
        {
            mSpriteBullet.setColor(c);
        }

        //-------------------------------------------------------------------------
        public CRenderScene getScene()
        {
            return mScene;
        }

        //-------------------------------------------------------------------------
        public int getBulletObjId()
        {
            return mBulletObjId;
        }

        //-------------------------------------------------------------------------
        public int getBulletRate()
        {
            return mBulletRate;
        }

        //-------------------------------------------------------------------------
        public uint getPlayerId()
        {
            return mPlayerId;
        }

        //-------------------------------------------------------------------------
        public CSpriteBullet getSpriteBullet()
        {
            return mSpriteBullet;
        }

        //-------------------------------------------------------------------------
        public TbDataBullet getBulletData()
        {
            return mVibBullet;
        }

        //-------------------------------------------------------------------------
        public CRenderFish getLockFish()
        {
            return mScene.getLevel().getFishByObjId(mLockedFishObjid);
        }
    }
}

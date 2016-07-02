using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class CLogicBullet : IDisposable
    {
        //---------------------------------------------------------------------
        CLogicScene mScene = null;// 场景指针
        uint mPlayerId;// 所属玩家
        int mBulletObjId = 0;// 子弹obj_id
        int miTurretRate = 1;
        float mfAngle = 90.0f;// 子弹当前初始角度
        EbVector3 mPos;// 子弹当前初始位置（米）
        float mfBulletSpeed = 1200.0f;// 子弹初始速度（米/秒）
        BulletCollider mBulletCollider = null;
        int mLockedFishObjid = -1;
        bool mDestroy = false;
        float mLifeCounter = 10.0f;

        //---------------------------------------------------------------------
        public CLogicBullet(CLogicScene logic_scene)
        {
            mScene = logic_scene;

        }

        //-----------------------------------------------------------------------------
        ~CLogicBullet()
        {
            this.Dispose(false);
        }

        //-----------------------------------------------------------------------------
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        //-----------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            destroy();
        }

        //---------------------------------------------------------------------
        public void create(uint et_player_rpcid, int bullet_objid, int turret_rate, int bullet_vibid,
            EbVector3 turret_pos, float turret_angle, float bullet_speed)
        {
            mPlayerId = et_player_rpcid;
            mBulletObjId = bullet_objid;
            mPos = turret_pos;
            mfAngle = turret_angle;
            miTurretRate = turret_rate;
            mfBulletSpeed = bullet_speed;

            CLogicTurret turret = mScene.getTurret(mPlayerId);
            if (turret == null || !turret.isBot()) return;

            mBulletCollider = mScene.getColliderMgr().newBulletCollider(0, 0, 41, 47);// 读取vib配置
            mBulletCollider.onCollision += onCollision;
            mBulletCollider.setDirection(mfAngle);
            update(0);
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            if (mDestroy) return;
            mDestroy = true;
            if (mBulletCollider != null)
            {
                mBulletCollider.onCollision -= onCollision;
                mScene.getColliderMgr().removeCollider(mBulletCollider);
                mBulletCollider = null;
            }
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            mLifeCounter -= elapsed_tm;
            if (mLifeCounter < 0)
            {
                CLogicTurret turret = mScene.getTurret(mPlayerId);
                if (turret != null) turret.removeBullet(mBulletObjId);
                return;
            }

            // 不是机器人则直接返回
            if (mBulletCollider == null) return;

            CLogicFish fish = getLockFish();
            if (fish == null || fish.IsDie)
            {
                EbVector3 cur_pos = CLogicUtility.getCurrentPos(mPos, mfAngle, mfBulletSpeed, elapsed_tm);
                mPos = cur_pos;
                mBulletCollider.setPosition(mPos);
            }
            else
            {
                mfAngle = CLogicUtility.getAngle(fish.Position - mPos);
                mPos = CLogicUtility.getCurrentPos(mPos, mfAngle, mfBulletSpeed, elapsed_tm);
                mBulletCollider.setPosition(mPos);
                mBulletCollider.setDirection(mfAngle);
            }

            if (mScene.getSceneBox().check(ref mPos, ref mfAngle))
            {
                mLockedFishObjid = -1;
                mBulletCollider.setPosition(mPos);
                mBulletCollider.setDirection(mfAngle);
            }
        }

        //-------------------------------------------------------------------------
        public CLogicFish getLockFish()
        {
            return mScene.getLevel().findFish(mLockedFishObjid);
        }

        //-------------------------------------------------------------------------
        public CLogicScene getScene()
        {
            return mScene;
        }

        //-------------------------------------------------------------------------
        public int getBulletObjId()
        {
            return mBulletObjId;
        }

        //-------------------------------------------------------------------------
        public uint getPlayerId()
        {
            return mPlayerId;
        }

        //-------------------------------------------------------------------------
        public int getRate()
        {
            return miTurretRate;
        }

        //-------------------------------------------------------------------------
        void onCollision(TagCollider other)
        {
            CLogicFish fish = ((FishCollider)other).LogicFish;
            if (fish == null) return;
            if (!isTargetFish(fish)) return;
            mScene.getLevel().c2sFishHit(mPlayerId, mBulletObjId, fish.FishObjId);// 暂时关闭
        }

        //-------------------------------------------------------------------------
        bool isTargetFish(CLogicFish fish)
        {
            return mLockedFishObjid < 0 || getLockFish() == fish;
        }
    }
}

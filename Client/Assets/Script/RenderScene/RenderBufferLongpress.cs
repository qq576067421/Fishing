using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderBufferLongpress : CRenderBuffer
    {
        //-------------------------------------------------------------------------
        float mfTotalSecond = 0.0f;// 状态运行总秒数
        const float mfFireTimeSpan = 0.2f;// 本地发炮间隔秒数
        float mfLastFireSecond = 0.0f;// 上次本地发炮时的秒数
        const float mfSyncTimeSpan = 0.2f;// 网络同步间隔秒数
        float mfLastSyncSecond = 0.0f;// 上次网络同步时的秒数
        Queue<_tBullet> mQueBullet = new Queue<_tBullet>();

        //-------------------------------------------------------------------------
        public CRenderBufferLongpress(CRenderScene scene, CRenderTurret turret, string name, List<object> param, string prefab_name)
            : base(scene, turret, name, param, prefab_name)
        {
            if (_isMe())
            {
                mScene.getProtocol().c2sBeginLongpress(mScene.getMyPlayerId());
            }

            //mScene.getListener().onSceneShowInfo("开始长按发炮");
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();

            mQueBullet.Clear();

            if (_isMe())
            {
                mScene.getProtocol().c2sEndLongpress(mScene.getMyPlayerId());
            }

            //mScene.getListener().onSceneShowInfo("结束长按发炮");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (canDestroy()) return;

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

            mfTotalSecond += elapsed_tm;

            bool is_me = _isMe();

            if (is_me)
            {
                // 本人发炮，子弹生产者
                if (mfTotalSecond - mfLastFireSecond > mfFireTimeSpan)
                {
                    mfLastFireSecond = mfTotalSecond;

                    if (!InputController.Instance.MouseDown)
                    {
                        signDestroy();
                        return;
                    }

                    if (mTurret.getPlayerGold() < mTurret.getTurretRate())
                    {
                        mTurret.goldNotEnough();
                        return;
                    }

                    if (mTurret.checkWhetherFire())
                    {
                        Vector2 mouse_pos = InputController.Instance.CurrentMousePosition;
                        int bullet_objid = mTurret.genBulletObjId(1);
                        mTurret.doLocalFire(mouse_pos, bullet_objid, CBulletConstant.AutoLongpressSpeed);

                        _tBullet bullet;
                        bullet.bullet_objid = bullet_objid;
                        bullet.turret_angle = mTurret.getTurretAngle();
                        bullet.turret_rate = mTurret.getTurretRate();
                        bullet.locked_fish_objid = mTurret.getLockFishObjId();
                        mQueBullet.Enqueue(bullet);
                    }
                }
            }
            else
            {
                // 他人发炮，子弹消费者
                if (mfTotalSecond - mfLastFireSecond > mfFireTimeSpan)
                {
                    mfLastFireSecond = mfTotalSecond;

                    if (mQueBullet.Count > 0)
                    {
                        _tBullet bullet = mQueBullet.Dequeue();
                        mTurret.setLockFishByFishObjId(bullet.locked_fish_objid);
                        mTurret.setTurretAngle(bullet.turret_angle);
                        mTurret.setTurretRate(bullet.turret_rate);
                        mTurret.doOtherFire(bullet.bullet_objid, CBulletConstant.AutoLongpressSpeed);
                    }
                }
            }

            // 如果是本人，则按5Hz的频率进行自动发炮的网络同步
            if (is_me)
            {
                if (mfTotalSecond - mfLastSyncSecond > mfSyncTimeSpan)
                {
                    mfLastSyncSecond = mfTotalSecond;

                    mScene.getProtocol().c2sAutoFire(mTurret.getScenePlayerInfo().et_player_rpcid, mQueBullet);
                }
            }
        }

        //-------------------------------------------------------------------------
        public override string getName()
        {
            return "BufLongpress";
        }

        //-------------------------------------------------------------------------
        public void addQueBullet(Queue<_tBullet> que_bullet)
        {
            while (que_bullet.Count > 0)
            {
                mQueBullet.Enqueue(que_bullet.Dequeue());
            }
        }

        //-------------------------------------------------------------------------
        bool _isMe()
        {
            return (mTurret.getScenePlayerInfo().et_player_rpcid == mScene.getMyPlayerId());
        }
    }
}

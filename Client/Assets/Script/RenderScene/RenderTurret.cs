using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderTurret : IDisposable
    {
        //-------------------------------------------------------------------------
        CRenderScene mScene = null;
        CRenderBufferMgr mRenderBufferMgr = null;
        CSpriteTurretShow mCSpriteTurretShow = null;
        _tScenePlayer mPlayer;
        TbDataTurret mVibTurret;
        int mTurretId = -1;
        int mTurretRate = 100;
        float mTurretAngle = 0.0f;
        int mBulletMaxObjid = 1;
        float mBaseAngle = 0.0f;
        EbVector3 mTurretPos;
        const float mBarrelOffset = 55f;
        EbVector3 mBulletFirePos;
        Color mTurretColor;
        float mfTotalSecond = 0.0f;
        float mfLastTouchSecond = 0.0f;
        float mfIdleSecond = 0.0f;
        float mfObCountdown = 60.0f;
        const float mfHighFrequencyTimeSpan = 0.5f;
        int miHighFrequencyCount = 0;
        const int mMaxBullet = 30;
        TbDataTurret.TurretType mTurretType = TbDataTurret.TurretType.NormalTurret;
        TurretDataTable mTurretDataTable = null;

        //-------------------------------------------------------------------------
        public CRenderTurret(CRenderScene render_scene)
        {
            mScene = render_scene;
        }

        //-------------------------------------------------------------------------
        ~CRenderTurret()
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
        public void create(int turret_id, ref _tScenePlayer scene_player, int player_gold,
            bool buffer_power, bool buffer_freeze, bool buffer_longpress, bool buffer_rapid, int turret_rate, float turret_angle, int locked_fish_objid
            , TbDataTurret.TurretType turret_type)
        {
            mTurretId = turret_id;
            mPlayer = scene_player;
            mTurretAngle = turret_angle;

            mTurretType = turret_type;
            mTurretDataTable = new TurretDataTable();

            _setTurretRate(turret_rate);

            CTurretHelper turret_helper = mScene.getTurretHelper();
            mTurretPos = turret_helper.getPositionByOffset(turret_id,
                mScene.getRenderConfigure().TurretOffset);
            mBaseAngle = turret_helper.getBaseAngleByTurretId(turret_id);
            mTurretColor = turret_helper.getBaseColorByTurretId(turret_id).convert();

            float scene_length = mScene.getSceneLength();
            float scene_width = mScene.getSceneWidth();

            mCSpriteTurretShow = new CSpriteTurretShow();
            mCSpriteTurretShow.create(mScene, this);

            _displayTurretRate();

            mRenderBufferMgr = new CRenderBufferMgr(mScene, this,
                turret_helper.getPositionByOffset(turret_id, mScene.getRenderConfigure().TurretBufferOffset), mBaseAngle);
            setLockFishByFishObjId(locked_fish_objid);

            if (buffer_power)
            {
                List<object> param_list = new List<object>();
                mRenderBufferMgr.addBuffer("BufPower", param_list);
            }

            if (buffer_freeze)
            {
                List<object> param_list = new List<object>();
                mRenderBufferMgr.addBuffer("BufFreeze", param_list);
            }

            if (buffer_longpress)
            {
                List<object> param_list = new List<object>();
                mRenderBufferMgr.addBuffer("BufLongpress", param_list);
            }

            if (buffer_rapid)
            {
                List<object> param_list = new List<object>();
                mRenderBufferMgr.addBuffer("BufRapid", param_list);
            }

            mScene.getListener().onSceneTurretCreated(mTurretId);
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            mCSpriteTurretShow.destroy();
            mCSpriteTurretShow = null;
            mRenderBufferMgr.destroy();
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            CRenderLevel level = mScene.getLevel();
            if (level == null) return;

            mfTotalSecond += elapsed_tm;

            if (mScene.getMyPlayerId() == mPlayer.et_player_rpcid && !mScene.isSingleMode())
            {
                mfIdleSecond += elapsed_tm;
                if (mfIdleSecond > 60.0f)
                {
                    mfObCountdown -= elapsed_tm;
                    if (mfObCountdown <= 0.0f)
                    {
                        mfIdleSecond = 0.0f;
                        mfObCountdown = 60.0f;
                        mScene.getListener().onScenePlayerChange2Ob();
                    }
                    else
                    {
                        string str = "由于您一分钟没有发射炮弹 系统将在[ff0000]" + ((int)mfObCountdown).ToString() + "秒[-]后进入[00ffff]观战模式[-]";
                        //mScene.getListener().onSceneShowMessageBox(str, false, "", 1, (int)_eMessageBoxLayer.Ob, false, false);
                    }
                }
                else
                {
                    mfObCountdown = 60.0f;
                }

                if (mRenderBufferMgr.hasBuffer("BufLongpress") || mRenderBufferMgr.hasBuffer("BufRapid"))
                {
                    mfIdleSecond = 0.0f;
                    mfObCountdown = 60.0f;
                }
            }

            mCSpriteTurretShow.update(elapsed_tm);
            mRenderBufferMgr.update(elapsed_tm);
        }

        //-------------------------------------------------------------------------
        public void s2allcTurretRate(int turret_rate)
        {
            mScene.getSoundMgr().play("jia1", _eSoundLayer.LayerReplace);
            _setTurretRate(turret_rate);
            _displayTurretRate();
        }

        //-------------------------------------------------------------------------
        public void s2allcManualFire(int bullet_objid, float turret_angle, int turret_rate, int locked_fish_id)
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (!is_me)
            {
                mTurretAngle = turret_angle;
                mTurretRate = turret_rate;
                mBulletFirePos = mTurretPos + CLogicUtility.getDirection(mTurretAngle).normalized * mBarrelOffset;

                mCSpriteTurretShow.fireAt(mTurretAngle);

                int bullet_vibid = mVibTurret.BulletDataKey.Id;
                CRenderBullet bullet = new CRenderBullet(mScene);
                bullet.create(mPlayer.et_player_rpcid, bullet_objid, bullet_vibid, CBulletConstant.ManualSpeed,
                    locked_fish_id, mBulletFirePos, mTurretAngle, mVibTurret.HitFishParticleDataKey);

                //int vib_compose_data_id = getVibTurret().EffectComposeFire.Data.ID;
                //mScene.addEffect(vib_compose_data_id, new Dictionary<string, object>(), EffectTypeEnum.Client);

                mScene.getListener().onSceneFire(mPlayer.et_player_rpcid, getPlayerGold());
            }
            else
            {
                mfIdleSecond = 0.0f;
                mfObCountdown = 60.0f;
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcAutoFire(Queue<_tBullet> que_bullet)
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (!is_me)
            {
                if (mRenderBufferMgr.hasBuffer("BufLongpress"))
                {
                    CRenderBuffer buf = mRenderBufferMgr.getBuffer("BufLongpress");
                    CRenderBufferLongpress b = (CRenderBufferLongpress)buf;
                    b.addQueBullet(que_bullet);
                }
                else if (mRenderBufferMgr.hasBuffer("BufRapid"))
                {
                    CRenderBuffer buf = mRenderBufferMgr.getBuffer("BufRapid");
                    CRenderBufferRapid b = (CRenderBufferRapid)buf;
                    b.addQueBullet(que_bullet);
                }
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcEfxFire(int bullet_vibid, int bullet_objid, float level_cur_second, float turret_angle, int turret_rate, EbVector3 pos)
        {
            //bool is_me = (mScene.getMyPlayerId() == mPlayer.player_entityid);
            //if (!is_me)
            {
                mTurretAngle = turret_angle;
                mTurretRate = turret_rate;

                mBulletFirePos = pos;
                mCSpriteTurretShow.aimAt(mTurretAngle);
                CRenderBullet bullet = new CRenderBullet(mScene);
                bullet.create(mPlayer.et_player_rpcid, bullet_objid, bullet_vibid, CBulletConstant.ManualSpeed,
                    _getLockedFishIdFromLockedBuffer(), mBulletFirePos, turret_angle, mVibTurret.HitFishParticleDataKey);
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcLockFish(int locked_fish_id)
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (is_me) return;

            setLockFishByFishObjId(locked_fish_id);
        }

        //-------------------------------------------------------------------------
        public void s2allcUnlockFish()
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (is_me) return;

            CRenderBuffer buf = mRenderBufferMgr.getBuffer("BufLock");
            if (buf != null)
            {
                mRenderBufferMgr.removeBuffer("BufLock");
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcBeginLongpress()
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (is_me) return;

            if (!mRenderBufferMgr.hasBuffer("BufLongpress"))
            {
                List<object> param_list = new List<object>();
                mRenderBufferMgr.addBuffer("BufLongpress", param_list);
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcEndLongpress()
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (is_me) return;

            if (mRenderBufferMgr.hasBuffer("BufLongpress"))
            {
                mRenderBufferMgr.removeBuffer("BufLongpress");
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcBeginRapid()
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (is_me) return;

            if (!mRenderBufferMgr.hasBuffer("BufRapid"))
            {
                List<object> param_list = new List<object>();
                mRenderBufferMgr.addBuffer("BufRapid", param_list);
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcEndRapid()
        {
            bool is_me = (mScene.getMyPlayerId() == mPlayer.et_player_rpcid);
            if (is_me) return;

            if (mRenderBufferMgr.hasBuffer("BufRapid"))
            {
                mRenderBufferMgr.removeBuffer("BufRapid");
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcBeginPower()
        {
            if (!mRenderBufferMgr.hasBuffer("BufPower"))
            {
                List<object> param_list = new List<object>();
                mRenderBufferMgr.addBuffer("BufPower", param_list);
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcEndPower()
        {
            if (mRenderBufferMgr.hasBuffer("BufPower"))
            {
                mRenderBufferMgr.removeBuffer("BufPower");
            }
        }

        //-------------------------------------------------------------------------
        public void onFingerTouch(Vector2 fire_goal_position)
        {
            CRenderLevel level = mScene.getLevel();
            if (level == null) return;

            bool level_isrun = level.isNormal();
            if (!level_isrun) return;

            if (getPlayerGold() < mTurretRate)
            {
                goldNotEnough();
                return;
            }

            if (mfTotalSecond - mfLastTouchSecond < mfHighFrequencyTimeSpan)
            {
                miHighFrequencyCount++;
            }
            else
            {
                miHighFrequencyCount = 0;
            }

            if (checkWhetherFire())
            {
                int bullet_objid = genBulletObjId(1);
                doLocalFire(fire_goal_position, bullet_objid, CBulletConstant.ManualSpeed);

                mScene.getProtocol().c2sManualFire(mPlayer.et_player_rpcid, bullet_objid,
                    mTurretAngle, mTurretRate, _getLockedFishIdFromLockedBuffer());
            }

            mfLastTouchSecond = mfTotalSecond;
        }

        //-------------------------------------------------------------------------
        public int getLockFishObjId()
        {
            return _getLockedFishIdFromLockedBuffer();
        }

        //-------------------------------------------------------------------------
        public bool isMyTurret()
        {
            return mScene.getMyPlayerId() == mPlayer.et_player_rpcid;
        }

        //-------------------------------------------------------------------------
        public void onFingerLongPress(Vector2 fire_goal_position)
        {
            CRenderLevel level = mScene.getLevel();
            if (level == null) return;

            bool level_isrun = level.isNormal();
            if (!level_isrun) return;

            if (miHighFrequencyCount > 2)
            {
                if (!mRenderBufferMgr.hasBuffer("BufRapid"))
                {
                    List<object> param_list = new List<object>();
                    mRenderBufferMgr.addBuffer("BufRapid", param_list);
                }

                miHighFrequencyCount = 0;
            }
            else
            {
                if (!mRenderBufferMgr.hasBuffer("BufLongpress"))
                {
                    List<object> param_list = new List<object>();
                    mRenderBufferMgr.addBuffer("BufLongpress", param_list);
                }
            }
        }

        //-------------------------------------------------------------------------
        public void onFingerUp()
        {
            CRenderLevel level = mScene.getLevel();
            if (level == null) return;

            if (mRenderBufferMgr.hasBuffer("BufRapid"))
            {
                mRenderBufferMgr.removeBuffer("BufRapid");
            }

            if (mRenderBufferMgr.hasBuffer("BufLongpress"))
            {
                mRenderBufferMgr.removeBuffer("BufLongpress");
            }

            mfLastTouchSecond = mfTotalSecond;
        }

        //-------------------------------------------------------------------------
        public void onFingerDragMove(Vector2 fire_goal_position)
        {
            CRenderLevel level = mScene.getLevel();
            if (level == null) return;

            bool level_isrun = level.isNormal();
            if (!level_isrun) return;
        }

        //-------------------------------------------------------------------------
        public void onFingerTouchBuffer(GameObject buffer)
        {
            mRenderBufferMgr.onFingerTouchBuffer(buffer);
        }

        //-------------------------------------------------------------------------
        public void onFingerTouchFish(List<FishStillSprite> fishs)
        {
            if (!canLockFish()) return;

            if (fishs.Count <= 0) return;
            CRenderFish lock_fish = null;
            foreach (var it in fishs)
            {
                if (lock_fish == null)
                {
                    lock_fish = it.getSpriteFish().getRenderFish();
                }
                else if (lock_fish.getVibData().LockFishWeight < it.getSpriteFish().getRenderFish().getVibData().LockFishWeight)
                {
                    lock_fish = it.getSpriteFish().getRenderFish();
                }
            }

            mCSpriteTurretShow.setAim(lock_fish);
            setLockFishByFishObjId(lock_fish.FishObjId);
        }

        //-------------------------------------------------------------------------
        public void requestSwitchTurretRate(int rate)
        {
            mScene.getProtocol().c2sTurretRate(mPlayer.et_player_rpcid, rate);
        }

        //-------------------------------------------------------------------------
        public EbVector3 getAvatarPosition()
        {
            return CCoordinate.logic2pixelPos(
              mScene.getTurretHelper().getPositionByOffset(mTurretId, mScene.getRenderConfigure().TurretAvatarOffset));
        }

        //-------------------------------------------------------------------------
        public void setTurret(TbDataTurret.TurretType turret_type)
        {
            if (turret_type == TbDataTurret.TurretType.None) return;
            mTurretType = turret_type;
            _updateVibTurret();
            mCSpriteTurretShow.reloadAnimation();
        }

        //-------------------------------------------------------------------------
        bool canLockFish()
        {
            return mTurretType == TbDataTurret.TurretType.DragonTurret;
        }

        //-------------------------------------------------------------------------
        public float getAvatarAngle()
        {
            return mBaseAngle;
        }

        //-------------------------------------------------------------------------
        public bool checkWhetherFire()
        {
            if (mScene.getLevel().getBulletCountByPlayerId(mPlayer.et_player_rpcid) >= mMaxBullet)
            {
                // 子弹上限已经到了，不能再发射
                string str = "炮弹已达上限，不能再发射";
                //mScene.getListener().onSceneShowMessageBox(str, false, "", 1, (int)_eMessageBoxLayer.Ob, false, false);
                return false;
            }
            return true;
        }

        //-------------------------------------------------------------------------
        public void doLocalFire(int bullet_objid, float bullet_speed)
        {
            mBulletFirePos = mTurretPos + CLogicUtility.getDirection(mTurretAngle).normalized * mBarrelOffset;

            mCSpriteTurretShow.fireAt(mTurretAngle);

            int bullet_vibid = mVibTurret.BulletDataKey.Id;
            CRenderBullet bullet = new CRenderBullet(mScene);
            bullet.create(mPlayer.et_player_rpcid, bullet_objid, bullet_vibid, bullet_speed,
                _getLockedFishIdFromLockedBuffer(), mBulletFirePos, mTurretAngle, mVibTurret.HitFishParticleDataKey);

            int vib_compose_data_id = getVibTurret().EffectComposeFire.Id;
            mScene.addEffect(vib_compose_data_id, new Dictionary<string, object>(), EffectTypeEnum.Client);

            mScene.getListener().onSceneFire(mPlayer.et_player_rpcid, getPlayerGold());
        }

        //-------------------------------------------------------------------------
        public void doOtherFire(int bullet_objid, float bullet_speed)
        {
            mBulletFirePos = mTurretPos + CLogicUtility.getDirection(mTurretAngle).normalized * mBarrelOffset;

            mCSpriteTurretShow.fireAt(mTurretAngle);

            int bullet_vibid = mVibTurret.BulletDataKey.Id;
            CRenderBullet bullet = new CRenderBullet(mScene);
            bullet.create(mPlayer.et_player_rpcid, bullet_objid, bullet_vibid, bullet_speed,
                _getLockedFishIdFromLockedBuffer(), mBulletFirePos, mTurretAngle, mVibTurret.HitFishParticleDataKey);

            mScene.getListener().onSceneFire(mPlayer.et_player_rpcid, getPlayerGold());
        }

        //-------------------------------------------------------------------------
        public void doLocalFire(Vector2 mouse_pos, int bullet_objid, float bullet_speed)
        {
            if (mScene.getLevel().getFishByObjId(_getLockedFishIdFromLockedBuffer()) == null)
            {
                updateTurretAngle(new EbVector3(mouse_pos.x, mouse_pos.y, 0));
            }

            doLocalFire(bullet_objid, bullet_speed);
        }

        //-------------------------------------------------------------------------
        public void setLockFishByFishObjId(int lock_fish_obj_id)
        {
            CRenderFish fish = mScene.getLevel().getFishByObjId(lock_fish_obj_id);
            if (fish != null)
            {
                if (!mRenderBufferMgr.hasBuffer("BufLock"))
                {
                    List<object> param_list = new List<object>();
                    param_list.Add(lock_fish_obj_id);
                    mRenderBufferMgr.addBuffer("BufLock", param_list);
                }
                else if (mRenderBufferMgr.hasBuffer("BufLock"))
                {
                    CRenderBufferLock buf_lock = (CRenderBufferLock)mRenderBufferMgr.getBuffer("BufLock");
                    if (buf_lock.getLockFishObjId() != lock_fish_obj_id)
                    {
                        buf_lock.resetLockedFishObjId(lock_fish_obj_id);
                    }
                }
            }
            else
            {
                if (mRenderBufferMgr.hasBuffer("BufLock"))
                {
                    mRenderBufferMgr.removeBuffer("BufLock");
                }
            }
        }

        //-------------------------------------------------------------------------
        public void displayLinkFish(CRenderFish fish)
        {
            mCSpriteTurretShow.displayLinkFish(fish);
        }

        //-------------------------------------------------------------------------
        public CRenderFish getLockFish()
        {
            return mScene.getLevel().getFishByObjId(_getLockedFishIdFromLockedBuffer());
        }

        //-------------------------------------------------------------------------
        public CRenderBufferMgr getBufferMgr()
        {
            return mRenderBufferMgr;
        }

        //-------------------------------------------------------------------------
        public EbVector3 getTurretPos()
        {
            return mTurretPos;
        }

        //-------------------------------------------------------------------------
        public float getTurretAngle()
        {
            return mTurretAngle;
        }

        //-------------------------------------------------------------------------
        public void setTurretAngle(float turret_angel)
        {
            mTurretAngle = turret_angel;
            mBulletFirePos = mTurretPos + CLogicUtility.getDirection(mTurretAngle).normalized * mBarrelOffset;
            mCSpriteTurretShow.aimAt(mTurretAngle);
        }

        //-------------------------------------------------------------------------
        public int getTurretRate()
        {
            return mTurretRate;
        }

        //-------------------------------------------------------------------------
        public void setTurretRate(int turret_rate)
        {
            mTurretRate = turret_rate;
            foreach (var i in EbDataMgr.Instance.getMapData<TbDataTurret>())
            {
                if (mTurretRate == ((TbDataTurret)i.Value).TurretRate)
                {
                    mVibTurret = (TbDataTurret)i.Value;
                    break;
                }
            }
        }

        //-------------------------------------------------------------------------
        public Color getTurretColor()
        {
            return mTurretColor;
        }

        //-------------------------------------------------------------------------
        public void setBarrelColor(Color c)
        {
            if (mCSpriteTurretShow != null)
            {
                mCSpriteTurretShow.setBarrelColor(c);
            }
        }

        //-------------------------------------------------------------------------
        public bool isPower()
        {
            return mRenderBufferMgr.hasBuffer("BufPower");
        }

        //-------------------------------------------------------------------------
        public TbDataTurret getVibTurret()
        {
            return mVibTurret;
        }

        //-------------------------------------------------------------------------
        public int getTurretId()
        {
            return mTurretId;
        }

        //-------------------------------------------------------------------------
        public _tScenePlayer getScenePlayerInfo()
        {
            return mPlayer;
        }

        //-------------------------------------------------------------------------
        public void setScore(int score)
        {
        }

        //-------------------------------------------------------------------------
        public void displayScore()
        {
            _displayTurretRate();
        }

        //-------------------------------------------------------------------------
        public void updateTurretAngle(EbVector3 target_position)
        {
            EbVector3 bullet_direction = target_position - mTurretPos;
            mTurretAngle = CLogicUtility.getAngle(bullet_direction);
            mBulletFirePos = mTurretPos + bullet_direction.normalized * mBarrelOffset;
            mCSpriteTurretShow.aimAt(mTurretAngle);
        }

        //-------------------------------------------------------------------------
        public int genBulletObjId(int count)
        {
            int k = mBulletMaxObjid;
            mBulletMaxObjid += count;
            return k;
        }

        //-------------------------------------------------------------------------
        public void displayScoreTurnplate(int score, TbDataParticle particle_data)
        {
            mCSpriteTurretShow.displayScoreTurnplate(score, particle_data);
        }

        //-------------------------------------------------------------------------
        public void displayChips(int score)
        {
            mCSpriteTurretShow.displayChips(score);
        }

        //-------------------------------------------------------------------------
        public void displayHypericum(bool active)
        {
        }

        //-------------------------------------------------------------------------
        public void goldNotEnough()
        {
            //int vib_compose_data_id = getVibTurret().EffectComposeFireNoMoney.Data.ID;
            //mScene.addEffect(vib_compose_data_id, new Dictionary<string, object>(), EffectTypeEnum.Client);

            if (mScene.getMyPlayerId() == mPlayer.et_player_rpcid)
            {
                mScene.getListener().onSceneNoBullet(mPlayer.et_player_rpcid);
            }
        }

        //-------------------------------------------------------------------------
        public int getPlayerGold()
        {
            return mScene.getListener().onGetPlayerGold(mPlayer.et_player_rpcid);
        }

        //-------------------------------------------------------------------------
        void _setTurretRate(int turret_rate)
        {
            mTurretRate = turret_rate;
            _updateVibTurret();
        }

        //-------------------------------------------------------------------------
        void _updateVibTurret()
        {
            mVibTurret = mTurretDataTable.getTurretData(mTurretType, mTurretRate);
        }

        //-------------------------------------------------------------------------
        void _displayTurretRate()
        {
            mCSpriteTurretShow.displayRate(mTurretRate);
            mCSpriteTurretShow.reloadAnimation();
            mScene.getListener().onSetPlayerTurretRate(mPlayer.et_player_rpcid, mTurretId, mTurretRate);
        }

        //-------------------------------------------------------------------------
        int _getLockedFishIdFromLockedBuffer()
        {
            CRenderBufferLock lock_buffer = getBufferMgr().getBuffer("BufLock") as CRenderBufferLock;
            if (lock_buffer == null)
            {
                return -1;
            }
            else
            {
                return lock_buffer.getLockFishObjId();
            }
        }
    }

    public static class CTurretHelperColor
    {
        public static Color convert(this CTurretHelper.Color color)
        {
            return new Color(color.r, color.g, color.b);
        }
    }
}

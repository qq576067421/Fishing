using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class CLogicBot
    {
        //---------------------------------------------------------------------
        enum _tAIState
        {
            observe,
            randomFire,
            attentionFire,
            strafeFire,
            longFire,
            rapidFire,
            lockFish,
            unLockFish,
        }

        //---------------------------------------------------------------------
        CLogicScene mScene = null;
        CLogicTurret mTurret = null;
        int mBulletObjId = 0;
        _tAIState AIState = _tAIState.observe;
        Counter mDecisionCounter = null;
        CLogicFish mAttentionFireFish = null;

        List<int> mBotLoveFishList = new List<int>();

        float mfTotalSecond = 0.0f;// 状态运行总秒数
        float mfFireTimeSpan = 0.2f;// 本地发炮间隔秒数
        float mfLastFireSecond = 0.0f;// 上次本地发炮时的秒数
        float mAutoFireTargetAngle;
        bool mIsLock = true;
        Counter mRandomRateCounter = null;
        TbDataTurret.TurretType mTurretType;

        //---------------------------------------------------------------------
        public void create(CLogicScene logic_scene, CLogicTurret turret, TbDataTurret.TurretType turret_type)
        {
            mScene = logic_scene;
            mTurret = turret;
            mTurretType = turret_type;

            // 机器人锁定鱼初始化，依赖这些顺序
            mBotLoveFishList.Add(24);
            mBotLoveFishList.Add(21);

            addBotLove(31, 36);
            addBotLove(25, 30);

            mBotLoveFishList.Add(20);
            mBotLoveFishList.Add(39);
            mBotLoveFishList.Add(37);
            mBotLoveFishList.Add(22);
            mBotLoveFishList.Add(23);

            mDecisionCounter = new Counter(0, true);
            mRandomRateCounter = new Counter(0, true);

            randomRateTime();
            mTurret.randomTurretRate();
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            switch (AIState)
            {
                case _tAIState.observe:
                    observe(elapsed_tm);
                    break;
                case _tAIState.randomFire:
                    randomFire(elapsed_tm);
                    break;
                case _tAIState.attentionFire:
                    attentionFire(elapsed_tm);
                    break;
                case _tAIState.strafeFire:
                    strafeFire(elapsed_tm);
                    break;
                case _tAIState.longFire:
                    longFire(elapsed_tm);
                    break;
                case _tAIState.rapidFire:
                    rapidFire(elapsed_tm);
                    break;
                case _tAIState.lockFish:
                    lockFish(elapsed_tm);
                    break;
            }

            randomUnlockFish();
            //updateRate(elapsed_tm);
        }

        //---------------------------------------------------------------------
        void updateRate(float elapsed_tm)
        {
            if (mDecisionCounter.notYet(elapsed_tm)) return;
            //mTurret.randomTurretRate();
            randomRateTime();
        }

        #region ai_state

        //---------------------------------------------------------------------
        void observe(float elapsed_tm)
        {
            if (mDecisionCounter.notYet(elapsed_tm)) return;

            float random = getRandom01();

            int max_index = 3;
            float base_value = 1f / max_index;

            if (random > 0.55f)
            {
                randomFireState(elapsed_tm);
            }
            else if (random > 0.1f)
            {
                if (mTurretType == TbDataTurret.TurretType.DragonTurret)
                {
                    change2lockFishState();
                }
            }
            else
            {
                change2observeState();
            }
        }

        //---------------------------------------------------------------------
        void randomFire(float elapsed_tm)
        {
            if (mDecisionCounter.notYet(elapsed_tm)) return;

            float random = getRandom01();
            if (random > 0.1f)
            {
                manualFire(randomTurretAngle());
            }
            else
            {
                change2observeState();
            }
        }

        //---------------------------------------------------------------------
        void attentionFire(float elapsed_tm)
        {
            if (mDecisionCounter.notYet(elapsed_tm)) return;

            float random = getRandom01();
            if (mAttentionFireFish == null || random < 0.1f)
            {
                mAttentionFireFish = null;
                change2observeState();
            }
            else
            {
                manualFire(mTurret.getTurretAngle());
            }
        }

        //---------------------------------------------------------------------
        void strafeFire(float elapsed_tm)
        {
            change2observeState();
            if (mDecisionCounter.notYet(elapsed_tm)) return;

            mCurrentDirection = regularAngle(mCurrentDirection + mStrafeSpeed * elapsed_tm);

            if (mStrafeSpeed > 0)
            {
                if (mCurrentDirection >= mMaxDirection)
                {
                    mStrafeSpeed = -mStrafeSpeed;
                }
            }
            else
            {
                if (mCurrentDirection <= mMinDirection)
                {
                    mStrafeSpeed = -mStrafeSpeed;
                }
            }

            manualFire(mCurrentDirection);
        }

        //---------------------------------------------------------------------
        void longFire(float elapsed_tm)
        {
            mfTotalSecond += elapsed_tm;

            if (mfTotalSecond - mfLastFireSecond > mfFireTimeSpan)
            {
                mfLastFireSecond = mfTotalSecond;

                float random = getRandom01();
                if (random < 0.1f)
                {
                    mTurret.c2sEndLongpress();
                    change2observeState();
                }

                int cur_gold = mScene.getListener().onLogicSceneGetPlayerGold(mTurret.getScenePlayerInfo().et_player_rpcid);
                if (cur_gold < mTurret.getTurretRate())
                {
                    mTurret.c2sEndLongpress();
                    change2observeState();
                    return;
                }
                else
                {
                    autoFire();
                }
            }
        }

        //---------------------------------------------------------------------
        void rapidFire(float elapsed_tm)
        {
            mfTotalSecond += elapsed_tm;

            if (mfTotalSecond - mfLastFireSecond > mfFireTimeSpan)
            {
                mfLastFireSecond = mfTotalSecond;

                float random = getRandom01();
                if (random < 0.1f)
                {
                    mTurret.c2sEndRapid();
                    change2observeState();
                }

                int cur_gold = mScene.getListener().onLogicSceneGetPlayerGold(mTurret.getScenePlayerInfo().et_player_rpcid);
                if (cur_gold < mTurret.getTurretRate())
                {
                    mTurret.c2sEndRapid();
                    change2observeState();
                    return;
                }
                else
                {
                    autoFire();
                }
            }
        }

        //---------------------------------------------------------------------
        void lockFish(float elapsed_tm)
        {
            if (mTurretType != TbDataTurret.TurretType.DragonTurret) return;
            if (mDecisionCounter.notYet(elapsed_tm)) return;

            mAttentionFireFish = findBotLoveFish();
            if (mAttentionFireFish == null)
            {
                change2observeState();
                return;
            }
            mIsLock = true;
            mTurret.c2sLockFish(mAttentionFireFish.FishObjId);
            randomFireState(elapsed_tm);
        }
        #endregion

        //---------------------------------------------------------------------
        void randomFireState(float elapsed_tm)
        {
            float random = getRandom01();

            int max_index = 4;
            float base_value = 1f / max_index;

            if (random > base_value * --max_index)
            {
                if (mAttentionFireFish == null) return;
                mAttentionFireFish = findMaxVibIdFish();
                change2attentionFireState();
            }
            else if (random > base_value * --max_index)
            {
                mfFireTimeSpan = 0.2f;
                mAutoFireTargetAngle = randomTurretAngle();
                mTurret.c2sBeginLongpress();
                change2longFireState();
            }
            else if (random > base_value * --max_index)
            {
                mfFireTimeSpan = 0.1f;
                mAutoFireTargetAngle = randomTurretAngle();
                mTurret.c2sBeginRapid();
                change2rapidFireState();
            }
            else
            {
                startStrafeFire(randomTurretAngle(), randomTurretAngle(), getRandomRange(10, 30));
                change2strafeFireState();
            }
        }

        //---------------------------------------------------------------------
        void randomUnlockFish()
        {
            if (!mIsLock) return;
            if (mAttentionFireFish == null || !_isInScene(mAttentionFireFish.Position, -100) || getRandom01() > 0.999f)
            {
                mTurret.c2sUnlockFish();
                mAttentionFireFish = null;
                mIsLock = false;
                change2observeState();
            }
        }

        //---------------------------------------------------------------------
        void manualFire(float angle)
        {
            if (mAttentionFireFish != null)
            {
                if (mIsLock)
                {
                    mTurret.c2sManualFire(++mBulletObjId, getTurretAngle(mAttentionFireFish.Position), mTurret.getTurretRate(), mAttentionFireFish.FishObjId);
                }
                else
                {
                    mTurret.c2sManualFire(++mBulletObjId, getTurretAngle(mAttentionFireFish.Position), mTurret.getTurretRate(), -1);
                }
            }
            else
            {
                mTurret.c2sManualFire(++mBulletObjId, angle, mTurret.getTurretRate(), -1);
            }
        }

        //---------------------------------------------------------------------
        void autoFire()
        {
            float turret_angle = mAutoFireTargetAngle;
            if (mIsLock && mAttentionFireFish != null)
            {
                turret_angle = getTurretAngle(mAttentionFireFish.Position);
            }

            _tBullet bullet;
            bullet.bullet_objid = ++mBulletObjId;
            bullet.turret_angle = turret_angle;
            bullet.turret_rate = mTurret.getTurretRate();
            bullet.locked_fish_objid = mTurret.getLockFishObjId();
            Queue<_tBullet> bullet_queue = new Queue<_tBullet>();
            bullet_queue.Enqueue(bullet);
            mTurret.c2sAutoFire(bullet_queue);
        }

        #region state_switch
        //---------------------------------------------------------------------
        void change2observeState()
        {
            mDecisionCounter.setDelayTime(getRandomRange(1, 3));
            AIState = _tAIState.observe;
        }

        //---------------------------------------------------------------------
        void change2randomFireState()
        {
            mDecisionCounter.setDelayTime(getRandomRange(1, 3));
            AIState = _tAIState.randomFire;
        }

        //---------------------------------------------------------------------
        void change2attentionFireState()
        {
            mDecisionCounter.setDelayTime(0.5f);
            AIState = _tAIState.attentionFire;
        }

        //---------------------------------------------------------------------
        void change2strafeFireState()
        {
            mDecisionCounter.setDelayTime(0.5f);
            AIState = _tAIState.strafeFire;
        }

        //---------------------------------------------------------------------
        void change2longFireState()
        {
            mDecisionCounter.setDelayTime(0.2f);
            AIState = _tAIState.longFire;
        }

        //---------------------------------------------------------------------
        void change2rapidFireState()
        {
            mDecisionCounter.setDelayTime(0.1f);
            AIState = _tAIState.rapidFire;
        }

        //---------------------------------------------------------------------
        void change2lockFishState()
        {
            mDecisionCounter.setDelayTime(0.1f);
            AIState = _tAIState.lockFish;
        }
        #endregion

        //---------------------------------------------------------------------
        CLogicFish findMaxVibIdFish()
        {
            List<CLogicFish> list_fish = mScene.getLevel().getAllFish();

            int max_fish_vib_id = 0;
            CLogicFish fish = null;

            foreach (var it in list_fish)
            {
                if (!_isInScene(it.Position, -80)) continue;
                if (max_fish_vib_id < it.FishVibId)
                {
                    max_fish_vib_id = it.FishVibId;
                    fish = it;
                }
            }

            return fish;
        }

        //---------------------------------------------------------------------
        CLogicFish findBotLoveFish()
        {
            List<CLogicFish> list_fish = mScene.getLevel().getAllFish();
            List<CLogicFish> find_list_fish = new List<CLogicFish>();

            foreach (var it in list_fish)
            {
                if (!_isInScene(it.Position, -80)) continue;
                if (isBotLove(it.FishVibId))
                {
                    find_list_fish.Add(it);
                }
            }

            foreach (var it in find_list_fish)
            {
                if (isBotLove(it.FishVibId))
                {
                    return it;
                }
            }

            return null;
        }

        //---------------------------------------------------------------------
        float mMinDirection = 0;
        float mMaxDirection = 0;
        float mCurrentDirection = 0;
        float mStrafeSpeed = 0f;

        //---------------------------------------------------------------------
        void startStrafeFire(float left, float right, float speed)
        {
            float random = getRandom01();
            if (random < 0.1f)
            {
                change2observeState();
                return;
            }

            if (left > right) startStrafeFire(right, left, speed);

            mMinDirection = left;
            mMaxDirection = right;
            mCurrentDirection = mMinDirection;
            mStrafeSpeed = _abs(speed);

            if (isInRange(left, -180, -90) && isInRange(right, 90, 180))
            {
                mStrafeSpeed = -mStrafeSpeed;
                return;
            }
        }

        //---------------------------------------------------------------------
        void randomRateTime()
        {
            mRandomRateCounter.setDelayTime(getRandomRange(30, 120));
        }

        //---------------------------------------------------------------------
        void addBotLove(int min_vib_id, int max_vib_id)
        {
            for (int i = max_vib_id; i >= min_vib_id; --i)
            {
                mBotLoveFishList.Add(i);
            }
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mTurret = null;
        }

        #region helper
        //---------------------------------------------------------------------
        float _abs(float v)
        {
            if (v < 0) return -v;
            return v;
        }

        //---------------------------------------------------------------------
        float regularAngle(float angle)
        {
            float regular_angle = angle % 360;

            if (regular_angle <= -180) regular_angle = -180 - regular_angle;
            if (regular_angle > 180) regular_angle = 180 - regular_angle;

            return regular_angle;
        }

        //---------------------------------------------------------------------
        float getTurretAngle(EbVector3 target_position)
        {
            EbVector3 bullet_direction = target_position - mTurret.getTurretPos();
            return CLogicUtility.getAngle(bullet_direction);
        }

        //---------------------------------------------------------------------
        bool isBotLove(int fish_id)
        {
            return mBotLoveFishList.Contains(fish_id);
        }

        //---------------------------------------------------------------------
        bool _isInScene(EbVector3 position, float border_size = 0)
        {
            return (position.x >= -(CCoordinate.LogicSceneLength / 2 + border_size)
                && position.x <= (CCoordinate.LogicSceneLength / 2 + border_size)
                && position.y >= -(CCoordinate.LogicSceneWidth / 2 + border_size)
                && position.y <= CCoordinate.LogicSceneWidth / 2 + border_size);
        }

        //---------------------------------------------------------------------
        bool isInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        //---------------------------------------------------------------------
        float randomTurretAngle()
        {
            EbVector3 bullet_direction = randomTarget() - mTurret.getTurretPos();
            return CLogicUtility.getAngle(bullet_direction);
        }

        //---------------------------------------------------------------------
        EbVector3 randomTarget()
        {
            return new EbVector3(getRandomFloat() * 700f, getRandomFloat() * 400f, 0);
        }

        //---------------------------------------------------------------------
        float getRandomRange(float min, float max)
        {
            if (min > max) return getRandomRange(max, min);
            return min + (max - min) * getRandom01();
        }

        //---------------------------------------------------------------------
        float getRandomFloat()
        {
            return getRandom01() - 0.5f;//-0.5f - 0.5f
        }

        //---------------------------------------------------------------------
        float getRandom01()
        {
            return (float)mTurret.getBotRandom().NextDouble();
        }
        #endregion
    }

    public class Counter
    {
        //-----------------------------------------------------------------------------
        float mDelayTime = 1;
        float mCurrentTime = 0;
        bool mAutoReset = false;

        //-----------------------------------------------------------------------------
        public Counter(float delay_time, bool auto_reset = false)
        {
            setDelayTime(delay_time);
            mAutoReset = auto_reset;
        }

        public void firstDecide(bool is_decide)
        {
            if (is_decide)
            {
                mCurrentTime = mDelayTime;
            }
            else
            {
                mCurrentTime = 0;
            }
        }

        //-----------------------------------------------------------------------------
        public void setDelayTime(float delay_time)
        {
            mDelayTime = delay_time;
        }

        //-----------------------------------------------------------------------------
        public void reset()
        {
            mCurrentTime = 0;
        }

        //-----------------------------------------------------------------------------
        public bool counter(float delta_time)
        {
            mCurrentTime += delta_time;
            if (mAutoReset && mCurrentTime >= mDelayTime)
            {
                reset();
                return true;
            }
            return (mCurrentTime >= mDelayTime);
        }

        //-----------------------------------------------------------------------------
        public bool notYet(float delta_time)
        {
            return !counter(delta_time);
        }
    }
}

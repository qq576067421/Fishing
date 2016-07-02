using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class CLogicTurret : IDisposable
    {
        //---------------------------------------------------------------------
        CLogicScene mScene = null;// 场景指针
        _tScenePlayer mPlayer;// 玩家信息
        TbDataTurret mVibTurret;// 炮台vib
        int mTurretId = -1;// 炮台id
        int mTurretRate = 1;// 炮台倍率
        float mTurretAngle = 0.0f;// 炮台角度
        EbVector3 mTurretPos;// 炮台位置
        const float mBarrelOffset = 55f;// 炮管相对于炮台的偏移量
        EbVector3 mBulletFirePos;// 子弹初始发射位置
        float mCurTmBufferPower = 0.0f;// 能量炮计时变量
        const float mMaxTmBufferPower = 15f;// 能量炮持续的时间
        float mfTotalSecond = 0.0f;// 总流逝秒数
        bool mBufferLongPress = false;// 长按buffer状态
        bool mBufferRapid = false;// 极速buffer状态
        bool mBufferPower = false;// 能量炮buffer状态
        bool mBufferFreeze = false;// 冻结buffer状态
        CLogicFish mLockedFish = null;// 被锁定的鱼
        Dictionary<int, CLogicBullet> mMapBullet = new Dictionary<int, CLogicBullet>();
        Queue<int> mQueDestroyedBullet = new Queue<int>();
        CLogicBot mLogicBot = null;
        Random mBotRandom = null;
        TbDataTurret.TurretType mTurretType = TbDataTurret.TurretType.NormalTurret;
        TurretDataTable mTurretDataTable = null;

        //---------------------------------------------------------------------
        public CLogicTurret(CLogicScene logic_scene)
        {
            mScene = logic_scene;
        }

        //---------------------------------------------------------------------
        ~CLogicTurret()
        {
            this.Dispose(false);
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        //---------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            destroy();
        }

        //---------------------------------------------------------------------
        public void create(int turret_id, uint et_player_rpcid, string nickname,
            bool is_bot, int default_turret_rate, TbDataTurret.TurretType turret_type)
        {
            mTurretId = turret_id;
            mLockedFish = null;
            mPlayer.et_player_rpcid = et_player_rpcid;
            mPlayer.nickname = nickname;
            mPlayer.is_bot = is_bot;
            mPlayer.rate = 1.0f;
            mTurretType = turret_type;
            mTurretDataTable = new TurretDataTable();

            mBotRandom = new System.Random(unchecked((int)System.DateTime.Now.Ticks) + turret_id * 1000);

            // 更新炮台倍率
            _setTurretRate(default_turret_rate);

            // 初始化炮台位置
            CTurretHelper turret_helper = new CTurretHelper();
            mTurretPos = turret_helper.getPositionByOffset(turret_id,
                new EbVector3(0, 68, 0));
            mTurretAngle = turret_helper.getBaseAngleByTurretId(turret_id);

            if (isBot())
            {
                mLogicBot = new CLogicBot();
                mLogicBot.create(mScene, this, turret_type);
            }
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mLockedFish = null;

            foreach (var i in mMapBullet)
            {
                i.Value.Dispose();
            }
            mMapBullet.Clear();

            if (mLogicBot != null)
            {
                mLogicBot.destroy();
                mLogicBot = null;
            }
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            mfTotalSecond += elapsed_tm;

            if (mLogicBot != null)
            {
                mLogicBot.update(elapsed_tm);
            }

            if (mBufferPower)
            {
                mCurTmBufferPower += elapsed_tm;
                if (mCurTmBufferPower >= mMaxTmBufferPower)
                {
                    mBufferPower = false;
                    mCurTmBufferPower = 0.0f;
                    mScene.getProtocol().s2allcEndPower(mPlayer.et_player_rpcid);
                }
            }

            foreach (var i in mMapBullet)
            {
                i.Value.update(elapsed_tm);
            }

            while (mQueDestroyedBullet.Count > 0)
            {
                int i = mQueDestroyedBullet.Dequeue();
                if (mMapBullet.ContainsKey(i))
                {
                    CLogicBullet bullet = mMapBullet[i];
                    bullet.Dispose();
                    mMapBullet.Remove(i);
                }
            }
        }

        //---------------------------------------------------------------------
        public void removeBullet(int bullet_object)
        {
            mQueDestroyedBullet.Enqueue(bullet_object);
        }

        //---------------------------------------------------------------------
        // 客户端请求炮台倍率切换
        public void c2sTurretRate(int rate)
        {
            List<int> list_rate = mScene.getListTurretRate();
            if (list_rate.Contains(rate))
            {
                _setTurretRate(rate);
                mScene.getProtocol().s2allcTurretRate(mPlayer.et_player_rpcid, mTurretRate);
            }
        }

        //---------------------------------------------------------------------
        public void randomTurretRate()
        {
            List<int> list_rate = mScene.getListTurretRate();
            int rate = list_rate[getBotRandom().Next() % list_rate.Count];
            _setTurretRate(rate);
            mScene.getProtocol().s2allcTurretRate(mPlayer.et_player_rpcid, mTurretRate);
        }

        //---------------------------------------------------------------------
        // 客户端请求手动发炮
        public void c2sManualFire(int bullet_objid, float turret_angle, int turret_rate, int locked_fish_id)
        {
            if (!_canLockFish())
            {
                locked_fish_id = -1;
            }

            // 扣币
            int cur_gold = mScene.getListener().onLogicSceneGetPlayerGold(mPlayer.et_player_rpcid);
            if (cur_gold < turret_rate)
            {
                return;
            }

            cur_gold -= turret_rate;
            mScene.getListener().onLogicSceneSetPlayerGold(mPlayer.et_player_rpcid, cur_gold, -1, "TurretFire", turret_rate);

            mTurretAngle = turret_angle;
            mTurretRate = turret_rate;

            // 创建子弹
            float level_cur_second = mScene.getLevel().getCurSecond();
            CLogicBullet bullet = new CLogicBullet(mScene);

            bullet.create(mPlayer.et_player_rpcid, bullet_objid, mTurretRate,
                -1, getFirePos(), mTurretAngle, CBulletConstant.ManualSpeed);
            if (!mMapBullet.ContainsKey(bullet_objid))
            {
                mMapBullet[bullet_objid] = bullet;
            }
            else
            {
                bullet.Dispose();
                bullet = null;
            }

            // 服务端广播发炮
            mScene.getProtocol().s2allcManualFire(mPlayer.et_player_rpcid,
                bullet_objid, turret_angle, turret_rate, locked_fish_id);
        }

        //---------------------------------------------------------------------
        EbVector3 getFirePos()
        {
            return mTurretPos + CLogicUtility.getDirection(mTurretAngle).normalized * mBarrelOffset;
        }

        //---------------------------------------------------------------------
        // 客户端请求自动发炮
        public void c2sAutoFire(Queue<_tBullet> que_bullet)
        {
            Queue<_tBullet> que_bullet2 = new Queue<_tBullet>();
            foreach (var bullet_struct in que_bullet)
            {
                // 扣币
                int cur_gold = mScene.getListener().onLogicSceneGetPlayerGold(mPlayer.et_player_rpcid);
                if (cur_gold < bullet_struct.turret_rate)
                {
                    return;
                }

                cur_gold -= bullet_struct.turret_rate;
                mScene.getListener().onLogicSceneSetPlayerGold(mPlayer.et_player_rpcid, cur_gold, -1, "TurretFire", bullet_struct.turret_rate);

                mTurretAngle = bullet_struct.turret_angle;
                mTurretRate = bullet_struct.turret_rate;

                // 创建子弹
                float level_cur_second = mScene.getLevel().getCurSecond();
                CLogicBullet bullet = new CLogicBullet(mScene);
                bullet.create(mPlayer.et_player_rpcid, bullet_struct.bullet_objid, mTurretRate, -1, getFirePos(), mTurretAngle, getBulletSpeed());
                if (!mMapBullet.ContainsKey(bullet_struct.bullet_objid))
                {
                    mMapBullet[bullet_struct.bullet_objid] = bullet;
                }
                else
                {
                    bullet.Dispose();
                    bullet = null;
                }

                que_bullet2.Enqueue(bullet_struct);
            }

            // 服务端广播自动发炮
            mScene.getProtocol().s2allcAutoFire(mPlayer.et_player_rpcid, que_bullet2);
        }

        //---------------------------------------------------------------------
        float getBulletSpeed()
        {
            if (mBufferRapid)
            {
                return CBulletConstant.AutoRapidSpeed;
            }
            if (mBufferLongPress)
            {
                return CBulletConstant.AutoLongpressSpeed;
            }
            return CBulletConstant.ManualSpeed;
        }

        //---------------------------------------------------------------------
        // 客户端请求锁定鱼
        public void c2sLockFish(int locked_fish_id)
        {
            // 锁定鱼
            if (locked_fish_id > 0)
            {
                mLockedFish = mScene.getLevel().findFish(locked_fish_id);
            }

            // 服务端广播锁定鱼
            mScene.getProtocol().s2allcLockFish(mPlayer.et_player_rpcid, locked_fish_id);
        }

        //---------------------------------------------------------------------
        // 客户端请求解锁鱼
        public void c2sUnlockFish()
        {
            // 解锁鱼
            mLockedFish = null;

            // 服务端广播解锁鱼
            mScene.getProtocol().s2allcUnlockFish(mPlayer.et_player_rpcid);
        }

        //---------------------------------------------------------------------
        // 客户端请求开始长按状态
        public void c2sBeginLongpress()
        {
            mBufferLongPress = true;

            // 服务端广播开始长按状态
            mScene.getProtocol().s2allcBeginLongpress(mPlayer.et_player_rpcid);
        }

        //---------------------------------------------------------------------
        // 客户端请求结束长按状态
        public void c2sEndLongpress()
        {
            mBufferLongPress = false;

            // 服务端广播结束长按状态
            mScene.getProtocol().s2allcEndLongpress(mPlayer.et_player_rpcid);
        }

        //---------------------------------------------------------------------
        // 客户端请求开始极速状态
        public void c2sBeginRapid()
        {
            mBufferRapid = true;

            // 服务端广播开始极速状态
            mScene.getProtocol().s2allcBeginRapid(mPlayer.et_player_rpcid);
        }

        //---------------------------------------------------------------------
        // 客户端请求结束极速状态
        public void c2sEndRapid()
        {
            mBufferRapid = false;

            // 服务端广播结束极速状态
            mScene.getProtocol().s2allcEndRapid(mPlayer.et_player_rpcid);
        }

        //---------------------------------------------------------------------
        public void activeBufferPower()
        {
            mBufferPower = true;
            mCurTmBufferPower = 0.0f;
            mScene.getProtocol().s2allcBeginPower(mPlayer.et_player_rpcid);
        }

        //---------------------------------------------------------------------
        public _tScenePlayer getScenePlayerInfo()
        {
            return mPlayer;
        }

        //---------------------------------------------------------------------
        public bool isBot()
        {
            return mPlayer.is_bot;
        }

        //---------------------------------------------------------------------
        // 玩家概率变更
        public void playerRateChanged(float rate)
        {
            mPlayer.rate = rate;
        }

        //---------------------------------------------------------------------
        public int getTurretId()
        {
            return mTurretId;
        }

        //---------------------------------------------------------------------
        public int getTurretRate()
        {
            return mTurretRate;
        }

        //---------------------------------------------------------------------
        public TbDataTurret.TurretType getTurretType()
        {
            return mTurretType;
        }

        //---------------------------------------------------------------------
        public float getTurretAngle()
        {
            return mTurretAngle;
        }

        //---------------------------------------------------------------------
        public CLogicFish getLockedFish()
        {
            return mLockedFish;
        }

        //---------------------------------------------------------------------
        public int getLockFishObjId()
        {
            if (mLockedFish == null) return -1;
            else return mLockedFish.FishObjId;
        }

        //---------------------------------------------------------------------
        public EbVector3 getTurretPos()
        {
            return mTurretPos;
        }

        //---------------------------------------------------------------------
        public bool getBufferLongpress()
        {
            return mBufferLongPress;
        }

        //---------------------------------------------------------------------
        public bool getBufferRapid()
        {
            return mBufferRapid;
        }

        //---------------------------------------------------------------------
        public Random getBotRandom()
        {
            return mBotRandom;
        }

        //---------------------------------------------------------------------
        public bool getBufferPower()
        {
            return mBufferPower;
        }

        //---------------------------------------------------------------------
        public bool getBufferFreeze()
        {
            return mBufferFreeze;
        }

        //-------------------------------------------------------------------------
        public void setTurret(TbDataTurret.TurretType turret_type)
        {
            if (turret_type == TbDataTurret.TurretType.None) return;
            mTurretType = turret_type;
            _updateVibTurretData();

            mScene.getProtocol().s2allcPlayerSetTurret(mPlayer.et_player_rpcid, mTurretType);
        }

        //---------------------------------------------------------------------
        public CLogicBullet getBullet(int bullet_objid)
        {
            if (mMapBullet.ContainsKey(bullet_objid))
            {
                return mMapBullet[bullet_objid];
            }
            else
            {
                return null;
            }
        }

        //-------------------------------------------------------------------------
        bool _canLockFish()
        {
            return mTurretType == TbDataTurret.TurretType.DragonTurret;
        }

        //---------------------------------------------------------------------
        // 玩家掉线
        void _playerDropped(uint et_player_rpcid)
        {
        }

        //---------------------------------------------------------------------
        // 玩家重连
        void _playerReConnect(uint et_player_rpcid)
        {
        }

        //---------------------------------------------------------------------
        // 更新炮台倍率
        void _setTurretRate(int rate)
        {
            if (rate == -1)
            {
                List<int> list_rate = mScene.getListTurretRate();

                if (list_rate.Count >= 3)
                {
                    mTurretRate = list_rate[2];
                }
                else {
                    mTurretRate = list_rate[0];
                }
            }
            else
            {
                mTurretRate = rate;
            }

            _updateVibTurretData();
        }

        //---------------------------------------------------------------------
        void _updateVibTurretData()
        {
            mVibTurret = mTurretDataTable.getTurretData(mTurretType, mTurretRate);
        }
    }
}

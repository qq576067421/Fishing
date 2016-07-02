using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderFish : IDisposable, BaseEntity
    {
        //-------------------------------------------------------------------------
        public bool IsDie { get { return mIsDie; } }
        public bool IsDestroy { get { return mIsDestroy; } }
        public int FishVibId { get { return mVibFish.Id; } }
        public int FishObjId { get { return mFishObjId; } }
        public EbVector3 Position { get { return mMassEntity.Position; } }

        CRenderScene mScene = null;// 场景指针
        TbDataFish mVibFish = null;// 鱼vib
        int mFishObjId = 0;// 鱼obj_id

        bool mIsDie = false;
        bool mIsDestroy = false;
        bool mFishStopMove = false;

        bool mIsSurvivalTime = false;
        float mSurvivalTime = 0;
        uint mPlayerId = 0;
        int mTotalScore = 0;

        MassEntity mMassEntity = null;
        ISpriteFish mISpriteFish = null;//鱼的表现层
        FishParticleMgr mFishParticleMgr = null;
        bool mNotNeedDestroyParticle = false;

        float mSecondsSinceFullScreenBomb = 3f;
        bool mSpriteFreeFromTimeFactor = false;

        //-------------------------------------------------------------------------
        public CRenderFish(CRenderScene render_scene)
        {
            mScene = render_scene;
        }

        //-------------------------------------------------------------------------
        public void create(int fish_vib_id, int fish_objid)
        {
            mVibFish = EbDataMgr.Instance.getData<TbDataFish>(fish_vib_id);
            mFishObjId = fish_objid;

            mISpriteFish = mScene.getSpriteFishFactory().buildSpriteFish(this, fish_vib_id);

            mMassEntity = new MassEntity();
            mMassEntity.setSpeed(mVibFish.getSpeed());

            mISpriteFish.setDirection(0);

            mFishParticleMgr = new FishParticleMgr(mScene, this, fish_vib_id, mISpriteFish);
            mFishParticleMgr.fishBorn();
            mFishParticleMgr.fishMoving();
        }

        //-------------------------------------------------------------------------
        public UnityEngine.GameObject FishGameObject { get { return mISpriteFish.FishGameObject; } }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            if (mISpriteFish == null) return;
            if (mIsDestroy) return;

            float update_delta_time = mSpriteFreeFromTimeFactor ? Time.deltaTime : elapsed_tm;

            mISpriteFish.update(update_delta_time);

            if (mMassEntity.IsOutScreen || mMassEntity.IsEndRoute)
            {
                mNotNeedDestroyParticle = true;
                signDestroy();
                return;
            }

            if (mIsSurvivalTime)
            {
                mSurvivalTime -= update_delta_time;
                if (mSurvivalTime < 0)
                {
                    signDestroy();
                    return;
                }
            }

            if (mFishStopMove) return;

            mMassEntity.update(update_delta_time);
            mISpriteFish.setPosition(mMassEntity.Position, mMassEntity.Angle);
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            if (mISpriteFish == null) return;
            // 播放鱼销毁后的粒子特效
            if (!mNotNeedDestroyParticle && IsDestroy)
            {
                mFishParticleMgr.fishDestroy(mPlayerId, mVibFish.Id, mTotalScore);
                mFishParticleMgr.fishCoins(mPlayerId);
            }
            mFishParticleMgr.destroy();

            mISpriteFish.destroy();
            mISpriteFish = null;
        }

        //-------------------------------------------------------------------------
        ~CRenderFish()
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
        public void dieWithRate(uint et_player_rpcid, int current_rate)
        {
            mFishStopMove = true;

            int total_score = current_rate * mVibFish.FishScore;
            _die(et_player_rpcid, total_score);
        }

        //-------------------------------------------------------------------------
        public void dieWithTotalScore(uint et_player_rpcid, int total_score)
        {
            mFishStopMove = true;

            _die(et_player_rpcid, total_score);
        }

        //-------------------------------------------------------------------------
        public void dieByFullBomb(EbVector3 dest_pos)
        {
            if (mIsDie) return;

            mNotNeedDestroyParticle = true;

            mISpriteFish.setTrigger(false);
            mISpriteFish.playRotationAnimation();
            mSpriteFreeFromTimeFactor = true;
            mMassEntity.setRoute(RouteHelper.buildLineRoute(mMassEntity.Position, dest_pos));
            float speed = mMassEntity.Position.getDistance(dest_pos) / mSecondsSinceFullScreenBomb;
            mMassEntity.setSpeed(speed);
            setSurvivalTime(mSecondsSinceFullScreenBomb);
            signDie();
        }

        //-------------------------------------------------------------------------
        public void signDestroy()
        {
            mIsDestroy = true;
        }

        //-------------------------------------------------------------------------
        public void signDie()
        {
            mIsDie = true;
        }

        //-------------------------------------------------------------------------
        public float getFishAngle()
        {
            return mMassEntity.Angle;
        }

        //---------------------------------------------------------------------
        public MassEntity getMassEntity()
        {
            return mMassEntity;
        }

        //-------------------------------------------------------------------------
        public bool canHitByFishNet()
        {
            return (mVibFish.mCanHitByFishNet == TbDataFish.CanHitByFishNet.YES);
        }

        //-------------------------------------------------------------------------
        public TbDataFish getVibData()
        {
            return mVibFish;
        }

        //-------------------------------------------------------------------------
        public CRenderScene getScene()
        {
            return mScene;
        }

        //-------------------------------------------------------------------------
        public void setDirection(float direction)
        {
            mMassEntity.setDirection(direction);
        }

        //-------------------------------------------------------------------------
        public void setPosition(EbVector3 position)
        {
            mMassEntity.setPosition(position);
        }

        //-------------------------------------------------------------------------
        public void setSpeed(float speed)
        {
            mMassEntity.setSpeed(speed);
        }

        //---------------------------------------------------------------------
        public void setAngleSpeed(float angle_speed)
        {
            mMassEntity.setAngleSpeed(angle_speed);
        }

        //-------------------------------------------------------------------------
        public void addRoute(IRoute route)
        {
            mMassEntity.setRoute(route);
        }

        public void addDynamicSystem(DynamicSystem system)
        {
            mMassEntity.setDynamicSystem(system);
        }

        //---------------------------------------------------------------------
        public void setSurvivalTime(float time)
        {
            mIsSurvivalTime = true;
            mSurvivalTime = time;
        }

        //---------------------------------------------------------------------
        public float jumpDistance()
        {
            return 25f;
        }

        //-------------------------------------------------------------------------
        void _die(uint et_player_rpcid, int total_score)
        {
            if (mIsDie) return;

            mPlayerId = et_player_rpcid;
            mTotalScore = total_score;

            mSpriteFreeFromTimeFactor = true;

            // 播放鱼死亡时候的粒子特效
            mFishParticleMgr.fishDie();

            // 显示掉落金币数字
            if (total_score > 0)
            {
                CSpriteFishDieScore score = CSpriteFishDieScore.getScore(mScene, mMassEntity.Position, total_score, 2);//待整理
            }

            // 播放鱼死亡时触发的特效
            foreach (var it in mVibFish.Effects)
            {
                int vib_compose_id = it.Id;

                if (vib_compose_id <= 0)
                {
                    continue;
                }

                Dictionary<string, object> param = new Dictionary<string, object>();
                param.Add("SourcePosition", mMassEntity.Position);
                param.Add("SourceAngle", 0f);
                param.Add("PlayerId", et_player_rpcid);
                CRenderTurret turret = mScene.getTurret(et_player_rpcid);
                if (turret != null)
                {
                    param.Add("DestPosition", turret.getTurretPos());
                }
                else
                {
                    param.Add("DestPosition", mMassEntity.Position);
                }
                param.Add("IsFishDie", true);
                mScene.addEffect(vib_compose_id, param, EffectTypeEnum.Client);
            }

            // 处理SpriteFish
            mFishStopMove = true;

            if (mISpriteFish != null)
            {
                mISpriteFish.playDieAnimation();
            }

            if (mVibFish.Red == TbDataFish.IsRed.YES)
            {
                mScene.getLevel().addRedFishPosition(mFishObjId, Position);
            }

            signDie();
        }
    }
}

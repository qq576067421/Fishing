using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CLogicFish : IDisposable, BaseEntity
    {
        //-------------------------------------------------------------------------
        CLogicScene mScene = null;// 场景指针
        TbDataFish mFishData = null;// 鱼vib
        int mFishObjId = 0;// 鱼obj_id
        float mSpeed = 0;// 鱼的速度
        TbDataFish.FishType mFishType = TbDataFish.FishType.ONE;
        MassEntity mMassEntity = null;
        bool mIsDie = false;
        FishCollider mFishCollider = null;
        bool mDestroy = false;

        //-------------------------------------------------------------------------
        public EbVector3 Position { get { return mMassEntity.Position; } }
        public bool IsDie { get { return mIsDie; } }
        public bool IsDestroy { get { return mIsDie; } }
        public int FishVibId { get { return mFishData.Id; } }
        public int FishObjId { get { return mFishObjId; } }

        //-------------------------------------------------------------------------
        public CLogicFish(CLogicScene logic_scene)
        {
            mScene = logic_scene;
        }

        //-------------------------------------------------------------------------
        ~CLogicFish()
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
        public void create(int fish_vibid, int fish_objid)
        {
            mFishData = EbDataMgr.Instance.getData<TbDataFish>(fish_vibid);
            mFishObjId = fish_objid;
            mSpeed = mFishData.getSpeed();

            mFishType = (TbDataFish.FishType)mFishData.Type;

            mMassEntity = new MassEntity();
            mMassEntity.setSpeed(mSpeed);

            mFishCollider = mScene.getColliderMgr().newFishCollider(0, 0, 80, 80, this);//读取vib配置

            update(0);
        }

        //-------------------------------------------------------------------------
        public MassEntity getMassEntity()
        {
            return mMassEntity;
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            if (mDestroy) return;
            mDestroy = true;
            TagColliderMgr collider_mgr = mScene.getColliderMgr();
            if (collider_mgr == null) return;
            collider_mgr.removeCollider(mFishCollider);
            mFishCollider = null;
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            mMassEntity.update(elapsed_tm);

            if (mFishCollider != null)
            {
                mFishCollider.setPosition(mMassEntity.Position);
                mFishCollider.setDirection(mMassEntity.Angle);
            }

            if (mMassEntity.IsOutScreen || mMassEntity.IsEndRoute)
            {
                signDestroy();
            }
        }

        //-------------------------------------------------------------------------
        public void signDestroy()
        {
            mIsDie = true;
        }

        //-------------------------------------------------------------------------
        // 被击中，返回是否鱼死亡
        public bool hit(uint et_player_rpcid, int bullet_rate, ref int score, ref int effect_fish_vib_id)
        {
            if (IsDie) return false;// 死鱼是打不死的

            double random_double = mScene.getLevel().getRandomScore();
            double die_chance = 1.0;

            // 鱼自身概率
            if (mFishData.FishDieChance >= 0)
            {
                die_chance = 1.0 / (double)mFishData.FishDieChance;
            }

            // 系统抽水率+玩家动态概率提升
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                die_chance *= mScene.getPumpingRate() * turret.getScenePlayerInfo().rate;
            }

            if (mScene.isFishMustDie())
            {
                // 调试需要，鱼必死
            }
            else if (random_double > die_chance)
            {
                // 根据概率计算，鱼不一定死
                return false;
            }

            // 使用固定分值
            score = mFishData.FishScore;

            // 极速发炮时命中鱼后有一定概率获得能量炮状态
            if (turret != null && turret.getBufferRapid())
            {
                if (mScene.getLevel().getRandoNumber(0, 10000) < 50)
                {
                    turret.activeBufferPower();
                }
            }

            // 需要在特效参数前加入销毁列表，aoe会根据这个列表判断鱼是不是已经死了死了就不计算。
            signDestroy();

            foreach (var it in mFishData.Effects)
            {
                int vib_compose_id = it.Id;
                if (vib_compose_id > 0)
                {
                    Dictionary<string, object> param = new Dictionary<string, object>();
                    param.Add("SourcePosition", mMassEntity.Position);
                    param.Add("PlayerID", et_player_rpcid);
                    param.Add("BulletRate", bullet_rate);
                    param.Add("RedFishObjId", mFishObjId);
                    param.Add("DieFishObjId", mFishObjId);

                    List<List<object>> obj = mScene.addEffect(vib_compose_id, param, EffectTypeEnum.Server);
                    if (obj == null) continue;
                    foreach (var l in obj)
                    {
                        if (l.Count == 0) continue;

                        string effect_name = (string)l[0];
                        if (effect_name == "Lighting")
                        {
                            score += (int)l[1];
                            effect_fish_vib_id = (int)l[2];
                        }
                        else if (effect_name == "FullScreenBomb")
                        {
                            score += (int)l[1];
                        }
                        else if (effect_name == "EffectAOE")
                        {
                            score += (int)l[1];
                        }
                    }
                }
            }

            return true;
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

        //-------------------------------------------------------------------------
        public void setAngleSpeed(float angle_speed)
        {
            mMassEntity.setAngleSpeed(angle_speed);
        }

        //-------------------------------------------------------------------------
        public void addRoute(IRoute route)
        {
            mMassEntity.setRoute(route);
        }

        //-------------------------------------------------------------------------
        public void addDynamicSystem(DynamicSystem system)
        {
            mMassEntity.setDynamicSystem(system);
        }
    }
}

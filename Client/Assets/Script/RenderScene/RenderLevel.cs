using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderLevel : EbFsm, IDisposable
    {
        //-------------------------------------------------------------------------
        CRenderScene mScene = null;// 场景指针
        CSpriteLevel mSpriteLevel = null;
        TbDataLevel mVibLevel = null;// 关卡vib
        Dictionary<uint, Dictionary<int, CRenderBullet>> mMapBullet = new Dictionary<uint, Dictionary<int, CRenderBullet>>();// 关卡中所有的子弹
        List<uint> mListCanDeleteKeyFromMapBullet = new List<uint>();
        Dictionary<uint, List<int>> mMapSignDestroyBullet = new Dictionary<uint, List<int>>();// 关卡中所有被标记为可删除的子弹
        List<int> mListSignDestroyFish = new List<int>();// 关卡中所有被标记为可删除的鱼
        float mTimeFactor = 1.0f;
        bool mLevelFreeze = false;
        System.Random mRandom = new System.Random(unchecked((int)System.DateTime.Now.Ticks));
        Dictionary<int, EbVector3> mDieRedFishPositionSaver = new Dictionary<int, EbVector3>();
        public int CurMapVibId { get; set; }
        public int NextMapVibId { get; set; }
        public float LevelCurRunSecond { get; set; }
        public float LevelMaxRunSecond { get; set; }
        BaseFishLordMgr mBaseFishLordMgr = null;

        //-------------------------------------------------------------------------
        public CRenderLevel(CRenderScene render_scene, string configure_filepath)
        {
            mScene = render_scene;
        }

        //-------------------------------------------------------------------------
        ~CRenderLevel()
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
        public void create(_eLevelState level_state, int level_vibid, int cur_map_vibid,
            int next_map_vibid, float level_run_totalsecond, float level_run_maxsecond,
            List<JsonPacket> json_packet_list, List<RouteJsonPacket> route_json_packet_list)
        {
            mVibLevel = EbDataMgr.Instance.getData<TbDataLevel>(level_vibid);
            CurMapVibId = cur_map_vibid;
            NextMapVibId = next_map_vibid;
            LevelCurRunSecond = level_run_totalsecond;
            LevelMaxRunSecond = level_run_maxsecond;

            mSpriteLevel = new CSpriteLevel();
            mSpriteLevel.create(mScene);

            addState(new CRenderLevelStateNormal(mScene, this));
            addState(new CRenderLevelStateSwitch(mScene, this));
            setupFsm();

            _initBaseFishLordMgr(json_packet_list, route_json_packet_list);

            if (getLevelState() != level_state)
            {
                processEvent("setState", level_state);
            }
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            processEvent("update", elapsed_tm);

            if (mSpriteLevel != null)
            {
                mSpriteLevel.update(elapsed_tm);
            }

            if (mBaseFishLordMgr != null)
            {
                mBaseFishLordMgr.update(elapsed_tm * mTimeFactor);
            }

            _updateBullet(elapsed_tm);
        }

        //-------------------------------------------------------------------------
        public void fixedUpdate(float elapsed_tm)
        {
            foreach (var i in mMapBullet)
            {
                Dictionary<int, CRenderBullet> m = i.Value;
                foreach (var j in m)
                {
                    j.Value.update(elapsed_tm);
                }
            }
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            destroyFsm();

            clearAllFish();

            // 销毁所有子弹
            foreach (var i in mMapBullet)
            {
                Dictionary<int, CRenderBullet> m = i.Value;
                foreach (var j in m)
                {
                    j.Value.Dispose();
                }
            }
            mMapBullet.Clear();
            mMapSignDestroyBullet.Clear();
            mListCanDeleteKeyFromMapBullet.Clear();

            if (mSpriteLevel != null)
            {
                mSpriteLevel.destroy();
            }

            DebugStatistics.Instance.display();
        }

        //-------------------------------------------------------------------------
        public void setup(_eLevelState level_state, int level_vibid, int cur_map_vibid,
            int next_map_vibid, float level_run_totalsecond, float level_run_maxsecond)
        {
            mVibLevel = EbDataMgr.Instance.getData<TbDataLevel>(level_vibid);
            CurMapVibId = cur_map_vibid;
            NextMapVibId = next_map_vibid;
            LevelCurRunSecond = level_run_totalsecond;
            LevelMaxRunSecond = level_run_maxsecond;

            if (getLevelState() == _eLevelState.Switch && level_state == _eLevelState.Normal)
            {
                mSpriteLevel.switchBackgroundEarly();
            }

            if (getLevelState() != level_state)
            {
                processEvent("setState", level_state);
            }
        }

        //-------------------------------------------------------------------------
        // 服务端广播消息：鱼被aoe打死
        public void s2allcAoeFishDie(uint et_player_rpcid, int bullet_rate,
            List<int> die_fish_obj_id, List<int> effect_fish_vib_id_list)
        {
            // 处理被aoe打死的鱼
            foreach (var it in die_fish_obj_id)
            {
                //_calculateFishByObjid(it, player_id, bullet_rate);
            }

            // 处理aoe打死的红鱼连死的鱼
            foreach (var it in effect_fish_vib_id_list)
            {
                if (it > 0)
                {
                    List<CRenderFish> fishs = getListFishById(it);
                    foreach (var f in fishs)
                    {
                        //_calculateFishByFish(f, player_id, bullet_rate);
                    }
                }
            }
        }

        //-------------------------------------------------------------------------
        public void s2allcCreateClientEffect(
            uint et_player_rpcid, int bullet_rate, EbVector3 position, int fish_id,
            int effect_id, string effect_name, int effect_type, float effect_delay_time,
            List<string> custom_param_list)
        {
            Dictionary<string, object> map_param = new Dictionary<string, object>();
            map_param.Add("PlayerId", et_player_rpcid);
            map_param.Add("Rate", bullet_rate);
            map_param.Add("FishId", fish_id);
            map_param.Add("EffectCustomParam", custom_param_list);

            // 根据死亡鱼的id查位置，查不到就用服务器的位置
            CRenderFish fish = (CRenderFish)mBaseFishLordMgr.findBaseEntity(fish_id);

            if (fish != null)
            {
                map_param.Add("SourcePosition", fish.Position);
            }
            else
            {
                map_param.Add("SourcePosition", position);
            }

            mScene.addSingleEffect(effect_id, effect_name, effect_type, effect_delay_time, map_param, EffectTypeEnum.Server2Client);
        }

        //-------------------------------------------------------------------------
        // 服务端广播消息：鱼死亡
        public void s2allcFishDie(uint et_player_rpcid, int total_score,
            int bullet_objid, int fish_objid, int effect_fish_vibid, int current_rate)
        {
            CRenderFish fish = (CRenderFish)mBaseFishLordMgr.findBaseEntity(fish_objid);
            if (fish != null)
            {
                //fish.signDestroy();
                // 销毁鱼
                fish.dieWithTotalScore(et_player_rpcid, total_score);

                // 更新金币和筹码
                CRenderTurret turret = mScene.getTurret(et_player_rpcid);
                if (turret != null)
                {
                    //turret.setPlayerGold(gold);

                    TbDataFish fish_data = EbDataMgr.Instance.getData<TbDataFish>(fish.FishVibId);
                    if (fish_data.FishDisplayScoreType == TbDataFish._eDisplayScoreType.Turnplate)
                    {
                        //turret.displayScoreTurnplate(total_score, fish_data.TurnplateParticle.Data);
                    }
                    else if (fish_data.FishDisplayScoreType == TbDataFish._eDisplayScoreType.Chips)
                    {
                        turret.displayChips(total_score);
                    }
                    else if (fish_data.FishDisplayScoreType == TbDataFish._eDisplayScoreType.ChipsAndTurnplate)
                    {
                        turret.displayChips(total_score);
                        //turret.displayScoreTurnplate(total_score, fish_data.TurnplateParticle.Data);
                    }
                }
            }

            // 销毁子弹
            if (mMapBullet.ContainsKey(et_player_rpcid))
            {
                Dictionary<int, CRenderBullet> m = mMapBullet[et_player_rpcid];
                if (!m.ContainsKey(bullet_objid)) return;
                m[bullet_objid].signDestroy();
            }
            else
            {
                //Debug.LogError("CRenderLevel::s2allcFishDie() not found player_id=" + player_id + " when signdestroy bullet");
            }
        }

        //---------------------------------------------------------------------
        public _eLevelState getLevelState()
        {
            List<EbState> list_state = getCurrentStateList();
            if (list_state.Count > 1)
            {
                EbState state = list_state[1];
                if (state._getStateName() == "CRenderLevelStateNormal")
                {
                    return _eLevelState.Normal;
                }
                else if (state._getStateName() == "CRenderLevelStateSwitch")
                {
                    return _eLevelState.Switch;
                }
            }

            return _eLevelState.Normal;
        }

        //-------------------------------------------------------------------------
        public CSpriteLevel getSpriteLevel()
        {
            return mSpriteLevel;
        }

        //---------------------------------------------------------------------
        public int getRandoNumber(int min, int max)// 闭区间，即包括最小和最大值
        {
            if (min > max)
            {
                return min;
            }
            return mRandom.Next(min, max + 1);
        }

        //-------------------------------------------------------------------------
        public void shockScreen()
        {
            mSpriteLevel.shockScreen();
        }

        //-------------------------------------------------------------------------
        public void stopShockScreen()
        {
            mSpriteLevel.stopShockScreen();
        }

        //-------------------------------------------------------------------------
        public bool isNormal()
        {
            return getLevelState() == _eLevelState.Normal;
        }

        //-------------------------------------------------------------------------
        public void addBullet(CRenderBullet bullet)
        {
            if (mMapBullet.ContainsKey(bullet.getPlayerId()))
            {
                Dictionary<int, CRenderBullet> m = mMapBullet[bullet.getPlayerId()];
                if (m.ContainsKey(bullet.getBulletObjId()))
                {
                    m[bullet.getBulletObjId()].Dispose();
                    m.Remove(bullet.getBulletObjId());
                }

                m[bullet.getBulletObjId()] = bullet;
            }
            else
            {
                Dictionary<int, CRenderBullet> m = new Dictionary<int, CRenderBullet>();
                m[bullet.getBulletObjId()] = bullet;
                mMapBullet[bullet.getPlayerId()] = m;
            }
        }

        //-------------------------------------------------------------------------
        public int getBulletCountByPlayerId(uint et_player_rpcid)
        {
            if (mMapBullet.ContainsKey(et_player_rpcid))
            {
                return mMapBullet[et_player_rpcid].Count;
            }
            return 0;
        }

        //-------------------------------------------------------------------------
        public void signDestroyBullet(uint et_player_rpcid, int bullet_objid)
        {
            if (mMapSignDestroyBullet.ContainsKey(et_player_rpcid))
            {
                mMapSignDestroyBullet[et_player_rpcid].Add(bullet_objid);
            }
            else
            {
                List<int> l = new List<int>();
                l.Add(bullet_objid);
                mMapSignDestroyBullet[et_player_rpcid] = l;
            }
        }

        //-------------------------------------------------------------------------
        public void newFishLord(string lord_file_name, List<string> server_param, int fish_begin_id)
        {
            mBaseFishLordMgr.addParticleSystem(lord_file_name, server_param, fish_begin_id);
        }

        //-------------------------------------------------------------------------
        public void setTimeFactor(float factor)
        {
            mTimeFactor = factor;
            if (mTimeFactor > 1)
            {
                mTimeFactor = 1;
            }
            if (mTimeFactor < 0)
            {
                mTimeFactor = 0;
            }
        }

        //-------------------------------------------------------------------------
        public float getTimeFactor()
        {
            return mTimeFactor;
        }

        //-------------------------------------------------------------------------
        public bool getLevelFreeze()
        {
            return mLevelFreeze;
        }

        //-------------------------------------------------------------------------
        public EbVector3 getRedFishPosition(int red_fish_obj_id)
        {
            if (mDieRedFishPositionSaver.ContainsKey(red_fish_obj_id))
            {
                return mDieRedFishPositionSaver[red_fish_obj_id];
            }

            CRenderFish fish = (CRenderFish)mBaseFishLordMgr.findBaseEntity(red_fish_obj_id);
            if (fish != null)
            {
                return fish.Position;
            }

            //ViDebuger.Error("getRedFishPosition error, coun't find the id " + red_fish_obj_id + " red fish.");
            return new EbVector3(10000, 0, 0);
        }

        //-------------------------------------------------------------------------
        public void addRedFishPosition(int red_fish_obj_id, EbVector3 position)
        {
            if (!mDieRedFishPositionSaver.ContainsKey(red_fish_obj_id))
            {
                mDieRedFishPositionSaver.Add(red_fish_obj_id, position);
            }
        }

        //-------------------------------------------------------------------------
        public void setLevelFreeze(bool freeze)
        {
            mLevelFreeze = freeze;
        }

        //-------------------------------------------------------------------------
        public void clearAllFish()
        {
            mBaseFishLordMgr.clearAllEntity();
            mScene.getEffectMgr().destroy();
        }

        //---------------------------------------------------------------------
        public void clearBaseEntityByPosition(float x)
        {
            mBaseFishLordMgr.clearBaseEntityByPosition(x);
        }

        //-------------------------------------------------------------------------
        public CRenderFish findFish(int fish_objid)
        {
            return (CRenderFish)mBaseFishLordMgr.findBaseEntity(fish_objid);
        }

        //-------------------------------------------------------------------------
        public List<CRenderFish> getListFishById(int fish_vib_id)
        {
            List<BaseEntity> entity_list = mBaseFishLordMgr.getEntityListById(fish_vib_id);

            List<CRenderFish> list_fish = new List<CRenderFish>();
            foreach (var entity in entity_list)
            {
                list_fish.Add((CRenderFish)entity);
            }
            return list_fish;
        }

        //-------------------------------------------------------------------------
        public CRenderFish getFishByObjId(int fish_obj_id)
        {
            CRenderFish fish = (CRenderFish)mBaseFishLordMgr.findBaseEntity(fish_obj_id);
            return fish;
        }

        //-------------------------------------------------------------------------
        public void openBulletCollision()
        {
            foreach (var i in mMapBullet)
            {
                Dictionary<int, CRenderBullet> m = i.Value;
                foreach (var j in m)
                {
                    j.Value.getSpriteBullet().setTrigger(true, 200);
                }
            }
        }

        //-------------------------------------------------------------------------
        public void closeBulletCollision()
        {
            foreach (var i in mMapBullet)
            {
                Dictionary<int, CRenderBullet> m = i.Value;
                foreach (var j in m)
                {
                    j.Value.getSpriteBullet().setTrigger(false);
                }
            }
        }

        //-------------------------------------------------------------------------
        public List<CRenderFish> getAllFish()
        {
            List<BaseEntity> entity_list = mBaseFishLordMgr.getAllEntity();

            List<CRenderFish> list_fish = new List<CRenderFish>();
            foreach (var entity in entity_list)
            {
                list_fish.Add((CRenderFish)entity);
            }
            return list_fish;
        }

        //-------------------------------------------------------------------------
        // 全屏炸弹的时候需要清除所有鱼群。
        public void destroyAllFishCrowd()
        {
            //foreach (var it in mListFishCrowd)
            //{
            //    it.destroy();
            //}
            //mListFishCrowd.Clear();

            //mBaseFishLordMgr.clearAllEntity();
        }

        //-------------------------------------------------------------------------
        bool _isNoBullet()
        {
            foreach (var i in mMapBullet)
            {
                if (i.Value.Count > 0)
                    return false;
            }
            return true;
        }

        //---------------------------------------------------------------------
        void _initBaseFishLordMgr(List<JsonPacket> json_packet_list, List<RouteJsonPacket> route_json_packet_list)
        {
            ParticleSystemFactory entity_particle_system_factory = new ParticleSystemFactory();

            // 发射器工厂
            entity_particle_system_factory.regGeneratorFactory(new DivergencePointGeneratorFactory());
            entity_particle_system_factory.regGeneratorFactory(new LineGeneratorFactory());
            entity_particle_system_factory.regGeneratorFactory(new CircleGeneratorFactory());
            entity_particle_system_factory.regGeneratorFactory(new IntervalDivergenceGeneratorFactory());
            entity_particle_system_factory.regGeneratorFactory(new RoundCircleGeneratorFactory());
            entity_particle_system_factory.regGeneratorFactory(new NormalGeneratorFactory());
            entity_particle_system_factory.regGeneratorFactory(new RedFishGeneratorFactory());
            entity_particle_system_factory.regGeneratorFactory(new FixGeneratorFactory());

            // 影响器工厂
            entity_particle_system_factory.regAffectorFactory(new StopAffectorFactory());
            entity_particle_system_factory.regAffectorFactory(new StartMoveAffectorFactory());
            entity_particle_system_factory.regAffectorFactory(new AngleSpeedAffectorFactory());

            mBaseFishLordMgr = new BaseFishLordMgr(new CRenderFishFactory(mScene), entity_particle_system_factory);
            mBaseFishLordMgr.create(json_packet_list, route_json_packet_list);
        }

        //-------------------------------------------------------------------------
        void _updateBullet(float elapsed_tm)
        {
            // 销毁可以销毁的子弹
            foreach (var i in mMapSignDestroyBullet)
            {
                if (mMapBullet.ContainsKey(i.Key))
                {
                    Dictionary<int, CRenderBullet> m = mMapBullet[i.Key];
                    List<int> l = i.Value;
                    foreach (var j in l)
                    {
                        if (m.ContainsKey(j))
                        {
                            m[j].Dispose();
                            m.Remove(j);
                        }
                    }
                }
            }
            mMapSignDestroyBullet.Clear();

            // 更新所有子弹
            foreach (var i in mMapBullet)
            {
                Dictionary<int, CRenderBullet> m = i.Value;
                //foreach (var j in m)
                //{
                //    j.Value.update(elapsed_tm);
                //}
                if (m.Count == 0 && !mScene.isExistPlayer(i.Key))
                {
                    mListCanDeleteKeyFromMapBullet.Add(i.Key);
                }
            }

            // 移除已经离开玩家并且该玩家所有子弹都消失了的pair
            foreach (var i in mListCanDeleteKeyFromMapBullet)
            {
                mMapBullet.Remove(i);
            }
            mListCanDeleteKeyFromMapBullet.Clear();
        }
    }
}

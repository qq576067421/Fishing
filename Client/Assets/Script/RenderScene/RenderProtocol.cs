using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderProtocol : IDisposable
    {
        //-------------------------------------------------------------------------
        delegate void onServerAoIMethod(List<string> vec_param);
        delegate void onServerMethod(List<string> vec_param);
        CRenderScene mScene = null;
        Dictionary<byte, onServerAoIMethod> mMapServerAoIMethod = new Dictionary<byte, onServerAoIMethod>();// key=method_name
        Dictionary<byte, onServerMethod> mMapServerMethod = new Dictionary<byte, onServerMethod>();// key=method_name
        Queue<List<string>> mQueProtocol = new Queue<List<string>>();
        float mfTotalSecond = 0.0f;
        float mfLastUpdateSecond = 0.0f;
        float mfUpdateTimeSpan = 0.2f;

        //-------------------------------------------------------------------------
        public CRenderProtocol(CRenderScene scene)
        {
            mScene = scene;

            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcLogicUpdate] = _s2allcLogicUpdate;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcPlayerEnter] = _s2allcPlayerEnter;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcPlayerLeave] = _s2allcPlayerLeave;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcPlayerDropped] = _s2allcPlayerDropped;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcPlayerReConnect] = _s2allcPlayerReConnect;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcLevelUpdate] = _s2allcLevelUpdate;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcCreateFishLord] = _s2allcCreateFishLord;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcFishDie] = _s2allcFishDie;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcCreateClientEffect] = _s2allcCreateClientEffect;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcAoeFishDie] = _s2allcAoeFishDie;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcTurretRate] = _s2allcTurretRate;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcManualFire] = _s2allcManualFire;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcAutoFire] = _s2allcAutoFire;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcEfxFire] = _s2allcEfxFire;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcLockFish] = _s2allcLockFish;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcUnlockFish] = _s2allcUnlockFish;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcBeginLongpress] = _s2allcBeginLongpress;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcEndLongpress] = _s2allcEndLongpress;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcBeginRapid] = _s2allcBeginRapid;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcEndRapid] = _s2allcEndRapid;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcBeginPower] = _s2allcBeginPower;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcEndPower] = _s2allcEndPower;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcSetTurret] = _s2allcSetTurret;
            mMapServerAoIMethod[(byte)_eProtocolDesktop.s2allcSyncAllPlayerGold] = _s2allcSyncAllPlayerGold;

            mMapServerMethod[(byte)_eProtocolDesktop.s2cSnapshotScene] = _s2cSnapshotScene;
        }

        //-------------------------------------------------------------------------
        ~CRenderProtocol()
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
        public void create()
        {
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            mfTotalSecond += elapsed_tm;
            if (mfTotalSecond - mfLastUpdateSecond > mfUpdateTimeSpan)
            {
                mfLastUpdateSecond = mfTotalSecond;

                List<string> vec_param = new List<string>();
                vec_param.Add(((byte)_eProtocolDesktop.c2sRenderUpdate).ToString());
                vec_param.Add(mQueProtocol.Count.ToString());
                while (mQueProtocol.Count > 0)
                {
                    List<string> l = mQueProtocol.Dequeue();
                    vec_param.Add(l.Count.ToString());
                    vec_param.AddRange(l);
                }

                mScene.getListener().onSceneRender2Logic(vec_param);
            }
        }

        //-------------------------------------------------------------------------
        public void onRecvAoI(List<string> vec_param)
        {
            try
            {
                byte server_aoi_methodname = byte.Parse(vec_param[0]);
                if (mMapServerAoIMethod.ContainsKey(server_aoi_methodname))
                {
                    mMapServerAoIMethod[server_aoi_methodname](vec_param);
                }
                else
                {
                    Debug.LogError("CRenderScene::onServerBroadcastMsg() not found methodname! methodname=" + server_aoi_methodname);
                }
            }
            catch (Exception e)
            {
                string str = "CRenderProtocol::onRecvAoI()\n";
                foreach (var it in vec_param)
                {
                    str += it.ToString() + "\n";
                }
                Debug.LogError(str + "\n" + e.ToString());
                throw e;
            }
        }

        //-------------------------------------------------------------------------
        public void onRecv(List<string> vec_param)
        {
            byte server_methodname = byte.Parse(vec_param[0]);
            if (mMapServerMethod.ContainsKey(server_methodname))
            {
                mMapServerMethod[server_methodname](vec_param);
            }
            else
            {
                Debug.LogError("CRenderScene::onServerMsg() not found methodname! methodname=" + server_methodname);
            }
        }

        //-------------------------------------------------------------------------
        public void c2sSnapshotScene(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sSnapshotScene).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            mScene.getListener().onSceneRender2Logic(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sFishHit(uint et_player_rpcid, int bullet_objid, int fish_objid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sFishHit).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(bullet_objid.ToString());
            vec_param.Add(fish_objid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sTurretRate(uint et_player_rpcid, int rate)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sTurretRate).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(rate.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sManualFire(uint et_player_rpcid, int bullet_objid, float turret_angle, int turret_rate, int lock_fish_objid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sManualFire).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(bullet_objid.ToString());
            vec_param.Add(turret_angle.ToString());
            vec_param.Add(turret_rate.ToString());
            vec_param.Add(lock_fish_objid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sAutoFire(uint et_player_rpcid, Queue<_tBullet> que_bullet)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sAutoFire).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(que_bullet.Count.ToString());
            while (que_bullet.Count > 0)
            {
                _tBullet bullet = que_bullet.Dequeue();
                vec_param.Add(bullet.bullet_objid.ToString());
                vec_param.Add(bullet.turret_angle.ToString());
                vec_param.Add(bullet.turret_rate.ToString());
                vec_param.Add(bullet.locked_fish_objid.ToString());
            }

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sLockFish(uint et_player_rpcid, int lock_fish_objid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sLockFish).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(lock_fish_objid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sUnlockFish(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sUnlockFish).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sBeginLongpress(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sBeginLongpress).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sEndLongpress(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sEndLongpress).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sBeginRapid(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sBeginRapid).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sEndRapid(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.c2sEndRapid).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //-------------------------------------------------------------------------
        public void _s2allcLogicUpdate(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            int protocol_num = int.Parse(vec_param[++index]);
            for (int i = 0; i < protocol_num; ++i)
            {
                int param_num = int.Parse(vec_param[++index]);
                List<string> l = new List<string>();
                for (int j = 0; j < param_num; ++j)
                {
                    l.Add(vec_param[++index]);
                }
                if (l.Count > 0)
                {
                    byte name = byte.Parse(l[0]);
                    if (mMapServerAoIMethod.ContainsKey(name))
                    {
                        mMapServerAoIMethod[name](l);
                    }
                    else
                    {
                        Debug.LogError("CRenderScene::_s2allcLogicUpdate() not found methodname! methodname=" + name);
                    }
                }
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcPlayerEnter(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            _tScenePlayer scene_player;
            scene_player.et_player_rpcid = uint.Parse(vec_param[++index]);
            scene_player.nickname = (string)vec_param[++index];
            scene_player.is_bot = false;
            int player_gold = int.Parse(vec_param[++index]);
            scene_player.rate = float.Parse(vec_param[++index]);
            int turret_id = int.Parse(vec_param[++index]);
            bool buffer_power = bool.Parse(vec_param[++index]);
            bool buffer_freeze = bool.Parse(vec_param[++index]);
            bool buffer_longpress = bool.Parse(vec_param[++index]);
            bool buffer_rapid = bool.Parse(vec_param[++index]);
            float turret_angle = float.Parse(vec_param[++index]);
            int turret_rate = int.Parse(vec_param[++index]);
            TbDataTurret.TurretType turret_type = (TbDataTurret.TurretType)(short.Parse(vec_param[++index]));
            int locked_fish_objid = int.Parse(vec_param[++index]);

            Dictionary<uint, CRenderTurret> map_turret = mScene.getMapTurret();
            if (map_turret.ContainsKey(scene_player.et_player_rpcid))
            {
                map_turret[scene_player.et_player_rpcid].Dispose();
                map_turret.Remove(scene_player.et_player_rpcid);
            }

            if (mScene.getMyPlayerId() == scene_player.et_player_rpcid)
            {
                mScene.setMyTurret(null);
            }

            CRenderTurret turret = new CRenderTurret(mScene);
            turret.create(turret_id, ref scene_player, player_gold, buffer_power, buffer_freeze, buffer_longpress,
                buffer_rapid, turret_rate, turret_angle, locked_fish_objid, turret_type);
            map_turret[scene_player.et_player_rpcid] = turret;

            if (mScene.getMyPlayerId() == scene_player.et_player_rpcid)
            {
                mScene.setMyTurret(turret);
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcPlayerLeave(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            uint et_player_rpcid = uint.Parse(vec_param[1]);

            Dictionary<uint, CRenderTurret> map_turret = mScene.getMapTurret();
            if (map_turret.ContainsKey(et_player_rpcid))
            {
                map_turret[et_player_rpcid].Dispose();
                map_turret.Remove(et_player_rpcid);
            }

            if (mScene.getMyPlayerId() == et_player_rpcid)
            {
                mScene.setMyTurret(null);
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcPlayerDropped(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            string player_id = (string)vec_param[1];
        }

        //-------------------------------------------------------------------------
        void _s2allcPlayerReConnect(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            string player_id = (string)vec_param[1];
        }

        //-------------------------------------------------------------------------
        void _s2allcLevelUpdate(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            _eLevelState level_state = (_eLevelState)(byte.Parse(vec_param[++index]));
            int level_vibid = int.Parse(vec_param[++index]);
            int cur_map_vibid = int.Parse(vec_param[++index]);
            int next_map_vibid = int.Parse(vec_param[++index]);
            float level_run_totalsecond = float.Parse(vec_param[++index]);
            float level_run_maxsecond = float.Parse(vec_param[++index]);

            CRenderLevel level = mScene.getLevel();
            level.setup(level_state, level_vibid, cur_map_vibid, next_map_vibid, level_run_totalsecond, level_run_maxsecond);
        }

        //-------------------------------------------------------------------------
        void _s2allcCreateFishLord(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            string lord_file_name = (string)vec_param[++index];
            int fish_begin_id = int.Parse(vec_param[++index]);

            int server_param_count = int.Parse(vec_param[++index]);
            List<string> server_param = new List<string>();

            for (int i = 0; i < server_param_count; i++)
            {
                server_param.Add(vec_param[++index]);
            }

            CRenderLevel level = mScene.getLevel();

            if (lord_file_name == "RedFish.lord")
            {
                int red_fish_obj_id = int.Parse(server_param[server_param.Count - 1]);
                EbVector3 pos = level.getRedFishPosition(red_fish_obj_id);

                if (pos.x < 10000)
                {
                    server_param.Add(pos.x.ToString());
                    server_param.Add(pos.y.ToString());
                }
            }
            mScene.resetRunInFormation();
            level.newFishLord(lord_file_name, server_param, fish_begin_id);
        }

        //-------------------------------------------------------------------------
        void _s2allcFishDie(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int total_score = int.Parse(vec_param[++index]);
            int bullet_objid = int.Parse(vec_param[++index]);
            int fish_objid = int.Parse(vec_param[++index]);
            int effect_fish_vibid = int.Parse(vec_param[++index]);
            int current_rate = int.Parse(vec_param[++index]);

            CRenderLevel level = mScene.getLevel();
            level.s2allcFishDie(et_player_rpcid, total_score, bullet_objid, fish_objid, effect_fish_vibid, current_rate);
        }

        //-------------------------------------------------------------------------
        void _s2allcCreateClientEffect(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;

            // 创建客户端的固定参数
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int bullet_rate = int.Parse(vec_param[++index]);
            float x = float.Parse(vec_param[++index]);
            float y = float.Parse(vec_param[++index]);
            EbVector3 position = new EbVector3(x, y, 0);
            int die_fish_id = int.Parse(vec_param[++index]);

            // 特效数据参数
            int effect_id = int.Parse(vec_param[++index]);
            string effect_name = (string)vec_param[++index];
            int effect_type = int.Parse(vec_param[++index]);
            float effect_delay_time = float.Parse(vec_param[++index]);

            // 特效自定义的参数
            int custom_param_count = int.Parse(vec_param[++index]);
            List<string> custom_param_list = new List<string>();
            for (int i = 0; i < custom_param_count; i++)
            {
                custom_param_list.Add(vec_param[++index]);
            }

            // 创建特效
            mScene.getLevel().s2allcCreateClientEffect(
                et_player_rpcid, bullet_rate, position, die_fish_id,
                effect_id, effect_name, effect_type, effect_delay_time,
                custom_param_list);
        }

        //-------------------------------------------------------------------------
        void _s2allcAoeFishDie(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int bullet_rate = int.Parse(vec_param[++index]);
            int die_fish_count = int.Parse(vec_param[++index]);

            List<int> die_fish_obj_id = new List<int>();

            for (int i = 0; i < die_fish_count; i++)
            {
                die_fish_obj_id.Add(int.Parse(vec_param[++index]));
            }

            int effect_fish_vib_id_list_count = int.Parse(vec_param[++index]);
            List<int> effect_fish_vib_id_list = new List<int>();

            for (int i = 0; i < effect_fish_vib_id_list_count; i++)
            {
                effect_fish_vib_id_list.Add(int.Parse(vec_param[++index]));
            }

            CRenderLevel level = mScene.getLevel();
            level.s2allcAoeFishDie(et_player_rpcid, bullet_rate, die_fish_obj_id, effect_fish_vib_id_list);
        }

        //-------------------------------------------------------------------------
        void _s2allcTurretRate(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int turret_rate = int.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcTurretRate(turret_rate);
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcManualFire(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int bullet_objid = int.Parse(vec_param[++index]);
            float turret_angle = float.Parse(vec_param[++index]);
            int turret_rate = int.Parse(vec_param[++index]);
            int locked_fish_id = int.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcManualFire(bullet_objid, turret_angle, turret_rate, locked_fish_id);
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcAutoFire(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int bullet_count = int.Parse(vec_param[++index]);

            Queue<_tBullet> que_bullet = new Queue<_tBullet>();
            for (int i = 0; i < bullet_count; ++i)
            {
                _tBullet bullet;
                bullet.bullet_objid = int.Parse(vec_param[++index]);
                bullet.turret_angle = float.Parse(vec_param[++index]);
                bullet.turret_rate = int.Parse(vec_param[++index]);
                bullet.locked_fish_objid = int.Parse(vec_param[++index]);
                que_bullet.Enqueue(bullet);
            }

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcAutoFire(que_bullet);
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcEfxFire(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int bullet_vibid = int.Parse(vec_param[++index]);
            int bullet_objid = int.Parse(vec_param[++index]);
            float level_total_second = float.Parse(vec_param[++index]);
            float turret_angle = float.Parse(vec_param[++index]);
            int turret_rate = int.Parse(vec_param[++index]);
            float posX = float.Parse(vec_param[++index]);
            float posY = float.Parse(vec_param[++index]);
            float posZ = float.Parse(vec_param[++index]);
            EbVector3 pos = new EbVector3(posX, posY, posZ);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcEfxFire(bullet_vibid, bullet_objid, level_total_second, turret_angle, turret_rate, pos);
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcLockFish(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int locked_fish_id = int.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcLockFish(locked_fish_id);
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcUnlockFish(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcUnlockFish();
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcBeginLongpress(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcBeginLongpress();
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcEndLongpress(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcEndLongpress();
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcBeginRapid(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcBeginRapid();
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcEndRapid(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcEndRapid();
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcBeginPower(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcBeginPower();
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcEndPower(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            CRenderTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.s2allcEndPower();
            }
        }

        //-------------------------------------------------------------------------
        void _s2allcSetTurret(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;

            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            TbDataTurret.TurretType turret_type = (TbDataTurret.TurretType)(short.Parse(vec_param[++index]));

            mScene.setTurret(et_player_rpcid, turret_type);
        }

        //-------------------------------------------------------------------------
        void _s2allcSyncAllPlayerGold(List<string> vec_param)
        {
            if (!mScene.isInit()) return;

            int index = 0;

            int turret_count = int.Parse(vec_param[++index]);

            for (int i = 0; i < turret_count; i++)
            {
                uint et_player_rpcid = uint.Parse(vec_param[++index]);
                int gold = int.Parse(vec_param[++index]);

                mScene.setPlayerGold(et_player_rpcid, gold);
            }
        }

        //-------------------------------------------------------------------------
        void _s2cSnapshotScene(List<string> vec_param)
        {
            EbLog.Note("CRenderProtocol._s2cSnapshotScene()");

            mScene.setInit();

            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            if (et_player_rpcid != mScene.getMyPlayerId()) return;

            _eLevelState level_state = (_eLevelState)(short.Parse(vec_param[++index]));
            int level_vibid = int.Parse(vec_param[++index]);
            int cur_map_vibid = int.Parse(vec_param[++index]);
            int next_map_vibid = int.Parse(vec_param[++index]);
            float level_run_totalsecond = float.Parse(vec_param[++index]);
            float level_run_maxsecond = float.Parse(vec_param[++index]);

            bool level_run_in_formation = bool.Parse(vec_param[++index]);

            mScene.destroyLevel();
            mScene.createLevel(level_state, level_vibid, cur_map_vibid, next_map_vibid, level_run_totalsecond, level_run_maxsecond, level_run_in_formation);

            mScene.destroyAllTurret();

            int rate_list_count = int.Parse(vec_param[++index]);
            List<int> list_rate = new List<int>(rate_list_count);
            for (int i = 0; i < rate_list_count; ++i)
            {
                list_rate.Add(int.Parse(vec_param[++index]));
            }

            mScene.setRateList(list_rate);

            int turret_count = int.Parse(vec_param[++index]);

            List<CRenderTurret> turret_list = new List<CRenderTurret>();

            for (int i = 0; i < turret_count; ++i)
            {
                _tScenePlayer scene_player;
                scene_player.et_player_rpcid = uint.Parse(vec_param[++index]);
                scene_player.nickname = (string)vec_param[++index];
                int player_gold = int.Parse(vec_param[++index]);
                scene_player.is_bot = false;
                scene_player.rate = float.Parse(vec_param[++index]);
                int turret_id = int.Parse(vec_param[++index]);
                bool buffer_power = bool.Parse(vec_param[++index]);
                bool buffer_freeze = bool.Parse(vec_param[++index]);
                bool buffer_longpress = bool.Parse(vec_param[++index]);
                bool buffer_rapid = bool.Parse(vec_param[++index]);
                float turret_angle = float.Parse(vec_param[++index]);
                int turret_rate = int.Parse(vec_param[++index]);
                TbDataTurret.TurretType turret_type = (TbDataTurret.TurretType)(byte.Parse(vec_param[++index]));
                int locked_fish_objid = int.Parse(vec_param[++index]);

                CRenderTurret turret = mScene.createTurret(turret_id, ref scene_player, player_gold, buffer_power, buffer_freeze, buffer_longpress,
                    buffer_rapid, turret_rate, turret_angle, locked_fish_objid, turret_type);

                turret_list.Add(turret);

                if (mScene.getMyPlayerId() == scene_player.et_player_rpcid)
                {
                    mScene.setMyTurret(turret);
                }
            }

            // 通知场景初始化完成
            mScene.getListener().onSceneSnapshot();

            foreach (var turret in turret_list)
            {
                turret.displayScore();
            }
        }
    }
}

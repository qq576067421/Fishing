using System;
using System.Collections.Generic;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    public class CLogicProtocol : IDisposable
    {
        //-------------------------------------------------------------------------
        CLogicScene mScene = null;
        delegate void onClientMethod(List<string> vec_param);// 委托，客户端方法
        Dictionary<byte, onClientMethod> mMapClientMethod = new Dictionary<byte, onClientMethod>();// key=method_name
        Queue<List<string>> mQueProtocol = new Queue<List<string>>();
        float mfTotalSecond = 0.0f;
        float mfLastUpdateSecond = 0.0f;
        float mfUpdateTimeSpan = 0.2f;

        //-------------------------------------------------------------------------
        public CLogicProtocol(CLogicScene scene)
        {
            mScene = scene;

            mMapClientMethod[(byte)_eProtocolDesktop.c2sRenderUpdate] = _c2sRenderUpdate;// 客户端更新消息
            mMapClientMethod[(byte)_eProtocolDesktop.c2sSnapshotScene] = _c2sSnapshotScene;// 客户端请求初始化场景
            mMapClientMethod[(byte)_eProtocolDesktop.c2sFishHit] = _c2sFishHit;// 客户端子弹命中鱼的消息
            mMapClientMethod[(byte)_eProtocolDesktop.c2sFishNetHit] = _c2sFishNetHit;// 客户端渔网命中鱼的消息
            mMapClientMethod[(byte)_eProtocolDesktop.c2sTurretRate] = _c2sTurretRate;// 客户端炮台倍率更新消息
            mMapClientMethod[(byte)_eProtocolDesktop.c2sManualFire] = _c2sManualFire;// 客户端提交手动发炮
            mMapClientMethod[(byte)_eProtocolDesktop.c2sAutoFire] = _c2sAutoFire;// 客户端提交自动发炮
            mMapClientMethod[(byte)_eProtocolDesktop.c2sLockFish] = _c2sLockFish;// 客户端提交锁定鱼
            mMapClientMethod[(byte)_eProtocolDesktop.c2sUnlockFish] = _c2sUnlockFish;// 客户端提交解锁鱼
            mMapClientMethod[(byte)_eProtocolDesktop.c2sBeginLongpress] = _c2sBeginLongpress;// 客户端提交开始长按状态
            mMapClientMethod[(byte)_eProtocolDesktop.c2sEndLongpress] = _c2sEndLongpress;// 客户端提交结束长按状态
            mMapClientMethod[(byte)_eProtocolDesktop.c2sBeginRapid] = _c2sBeginRapid;// 客户端提交开始极速状态
            mMapClientMethod[(byte)_eProtocolDesktop.c2sEndRapid] = _c2sEndRapid;// 客户端提交结束极速状态
        }

        //-------------------------------------------------------------------------
        ~CLogicProtocol()
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
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            // 定时向客户端广播消息
            mfTotalSecond += elapsed_tm;
            if (mfTotalSecond - mfLastUpdateSecond > mfUpdateTimeSpan)
            {
                mfLastUpdateSecond = mfTotalSecond;

                // 5Hz同步所有玩家金币
                Dictionary<uint, CLogicTurret> map_turret = mScene.getMapTurret();
                List<string> vec_param_ret = new List<string>();
                vec_param_ret.Add(((byte)_eProtocolDesktop.s2allcSyncAllPlayerGold).ToString());
                vec_param_ret.Add(map_turret.Count.ToString());
                foreach (var turret in map_turret)
                {
                    uint et_player_rpcid = turret.Value.getScenePlayerInfo().et_player_rpcid;
                    vec_param_ret.Add(et_player_rpcid.ToString());
                    int player_gold = mScene.getListener().onLogicSceneGetPlayerGold(et_player_rpcid);
                    vec_param_ret.Add(player_gold.ToString());
                }
                mQueProtocol.Enqueue(vec_param_ret);

                // 5Hz广播协议，每次最多发送10条
                int per_msgcount = 10;
                if (per_msgcount > mQueProtocol.Count)
                {
                    per_msgcount = mQueProtocol.Count;
                }

                List<string> vec_param = new List<string>();
                vec_param.Add(((byte)_eProtocolDesktop.s2allcLogicUpdate).ToString());
                vec_param.Add(per_msgcount.ToString());
                while (per_msgcount > 0)
                {
                    List<string> l = mQueProtocol.Dequeue();
                    vec_param.Add(l.Count.ToString());
                    vec_param.AddRange(l);
                    --per_msgcount;
                }

                mScene.getListener().onLogicScene2RenderAll(vec_param);
            }
        }

        //---------------------------------------------------------------------
        public void onRecv(List<string> vec_param)
        {
            if (vec_param.Count < 1)
            {
                // log error
                return;
            }

            byte client_methodname = byte.Parse(vec_param[0]);
            if (mMapClientMethod.ContainsKey(client_methodname))
            {
                mMapClientMethod[client_methodname](vec_param);
            }
            else
            {
                // log error
            }
        }

        //---------------------------------------------------------------------
        // 服务端响应玩家获取场景快照
        public void s2cSnapshotScene(uint et_player_rpcid)
        {
            List<string> vec_param_ret = new List<string>();
            vec_param_ret.Add(((byte)_eProtocolDesktop.s2cSnapshotScene).ToString());
            vec_param_ret.Add(et_player_rpcid.ToString());

            // 关卡信息
            CLogicLevel level = mScene.getLevel();
            vec_param_ret.Add(((byte)level.getLevelState()).ToString());
            vec_param_ret.Add(level.getLevelVibId().ToString());
            vec_param_ret.Add(level.CurMapVibId.ToString());
            vec_param_ret.Add(level.NextMapVibId.ToString());
            vec_param_ret.Add(level.getCurSecond().ToString());
            vec_param_ret.Add(level.getMaxSecond().ToString());
            vec_param_ret.Add(level.isFormation().ToString());

            // 同步房间炮台倍率信息。
            List<int> list_rate = mScene.getListTurretRate();
            vec_param_ret.Add(list_rate.Count.ToString());
            foreach (var i in list_rate)
            {
                vec_param_ret.Add(i.ToString());
            }

            // 炮台信息
            Dictionary<uint, CLogicTurret> map_turret = mScene.getMapTurret();
            vec_param_ret.Add(map_turret.Count.ToString());
            foreach (var i in map_turret)
            {
                _tScenePlayer scene_player = i.Value.getScenePlayerInfo();
                vec_param_ret.Add(scene_player.et_player_rpcid.ToString());
                vec_param_ret.Add(scene_player.nickname);
                int player_gold = mScene.getListener().onLogicSceneGetPlayerGold(scene_player.et_player_rpcid);
                vec_param_ret.Add(player_gold.ToString());
                vec_param_ret.Add(scene_player.rate.ToString());
                vec_param_ret.Add(i.Value.getTurretId().ToString());
                vec_param_ret.Add(i.Value.getBufferPower().ToString());
                vec_param_ret.Add(i.Value.getBufferFreeze().ToString());
                vec_param_ret.Add(i.Value.getBufferLongpress().ToString());
                vec_param_ret.Add(i.Value.getBufferRapid().ToString());
                vec_param_ret.Add(i.Value.getTurretAngle().ToString());
                vec_param_ret.Add(i.Value.getTurretRate().ToString());
                vec_param_ret.Add(((byte)(i.Value.getTurretType())).ToString());
                vec_param_ret.Add(i.Value.getLockFishObjId().ToString());
            }

            mScene.getListener().onLogicScene2Render(et_player_rpcid, vec_param_ret);
        }

        //---------------------------------------------------------------------
        // 服务端广播玩家进入
        public void s2allcPlayerEnter(CLogicTurret turret)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcPlayerEnter).ToString());

            _tScenePlayer scene_player = turret.getScenePlayerInfo();
            vec_param.Add(scene_player.et_player_rpcid.ToString());
            vec_param.Add(scene_player.nickname);
            int player_gold = mScene.getListener().onLogicSceneGetPlayerGold(scene_player.et_player_rpcid);
            vec_param.Add(player_gold.ToString());
            vec_param.Add(scene_player.rate.ToString());
            vec_param.Add(turret.getTurretId().ToString());
            vec_param.Add(turret.getBufferPower().ToString());
            vec_param.Add(turret.getBufferFreeze().ToString());
            vec_param.Add(turret.getBufferLongpress().ToString());
            vec_param.Add(turret.getBufferRapid().ToString());
            vec_param.Add(turret.getTurretAngle().ToString());
            vec_param.Add(turret.getTurretRate().ToString());
            vec_param.Add(turret.getTurretType().ToString());
            vec_param.Add(turret.getLockFishObjId().ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播玩家离开
        public void s2allcPlayerLeave(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcPlayerLeave).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播玩家炮台数据跟新
        public void s2allcPlayerSetTurret(uint et_player_rpcid, TbDataTurret.TurretType turret_type)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcSetTurret).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(turret_type.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播玩家掉线
        public void s2allcPlayerDropped(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcPlayerDropped).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播玩家重连
        public void s2allcPlayerReConnect(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcPlayerReConnect).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播关卡更新
        public void s2allcLevelUpdate(_eLevelState level_state, int level_vibid,
            int map_vibid, int next_map_vibid, float cur_second, float max_second)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcLevelUpdate).ToString());
            vec_param.Add(((byte)level_state).ToString());
            vec_param.Add(level_vibid.ToString());
            vec_param.Add(map_vibid.ToString());
            vec_param.Add(next_map_vibid.ToString());
            vec_param.Add(cur_second.ToString());
            vec_param.Add(max_second.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播创建阵型
        public void s2allcCreateFishLord(string lord_file_name, List<string> server_param, int fish_begin_id)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcCreateFishLord).ToString());
            vec_param.Add(lord_file_name);
            vec_param.Add(fish_begin_id.ToString());
            vec_param.Add(server_param.Count.ToString());
            foreach (var it in server_param)
            {
                vec_param.Add(it);
            }

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播鱼死亡
        public void s2allcFishDie(uint et_player_rpcid, int total_score,
            int bullet_objid, int fish_objid, int effect_fish_vibid, int current_rate)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcFishDie).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(total_score.ToString());
            vec_param.Add(bullet_objid.ToString());
            vec_param.Add(fish_objid.ToString());
            vec_param.Add(effect_fish_vibid.ToString());
            vec_param.Add(current_rate.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播创建客户端特效
        public void s2allcCreateClientEffect(
            uint et_player_rpcid, int bullet_rate, EbVector3 position, int die_fish_id,
            int effect_id, string effect_name, int effect_type, float effect_delay_time,
            List<string> param)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcCreateClientEffect).ToString());

            // 创建客户端的固定参数
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(bullet_rate.ToString());
            vec_param.Add(position.x.ToString());
            vec_param.Add(position.y.ToString());
            vec_param.Add(die_fish_id.ToString());

            // 特效数据参数
            vec_param.Add(effect_id.ToString());
            vec_param.Add(effect_name.ToString());
            vec_param.Add(effect_type.ToString());
            vec_param.Add(effect_delay_time.ToString());

            // 特效自定义的参数
            vec_param.Add(param.Count.ToString());
            foreach (var it in param)
            {
                vec_param.Add(it);
            }

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播炮台倍率更新
        public void s2allcTurretRate(uint et_player_rpcid, int turret_rate)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcTurretRate).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(turret_rate.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播手动发炮
        public void s2allcManualFire(uint et_player_rpcid, int buffet_objid, float turret_angle, int turret_rate, int locked_fish_id)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcManualFire).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(buffet_objid.ToString());
            vec_param.Add(turret_angle.ToString());
            vec_param.Add(turret_rate.ToString());
            vec_param.Add(locked_fish_id.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播自动发炮
        public void s2allcAutoFire(uint et_player_rpcid, Queue<_tBullet> que_bullet)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcAutoFire).ToString());
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

        //---------------------------------------------------------------------
        // 服务端广播效果发炮
        public void s2allcEfxFire(uint et_player_rpcid, int bullet_vibid, int buffet_objid,
            float level_cur_second, float turret_angle, int turret_rate, EbVector3 pos)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcEfxFire).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(bullet_vibid.ToString());
            vec_param.Add(buffet_objid.ToString());
            vec_param.Add(level_cur_second.ToString());
            vec_param.Add(turret_angle.ToString());
            vec_param.Add(turret_rate.ToString());
            vec_param.Add(pos.x.ToString());
            vec_param.Add(pos.y.ToString());
            vec_param.Add(pos.z.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播锁定鱼
        public void s2allcLockFish(uint et_player_rpcid, int locked_fish_id)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcLockFish).ToString());
            vec_param.Add(et_player_rpcid.ToString());
            vec_param.Add(locked_fish_id.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播解锁鱼
        public void s2allcUnlockFish(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcUnlockFish).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播开始长按状态
        public void s2allcBeginLongpress(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcBeginLongpress).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播结束长按状态
        public void s2allcEndLongpress(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcEndLongpress).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播开始极速状态
        public void s2allcBeginRapid(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcBeginRapid).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播结束极速状态
        public void s2allcEndRapid(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcEndRapid).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播开始能量炮状态
        public void s2allcBeginPower(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcBeginPower).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 服务端广播结束能量炮状态
        public void s2allcEndPower(uint et_player_rpcid)
        {
            List<string> vec_param = new List<string>();
            vec_param.Add(((byte)_eProtocolDesktop.s2allcEndPower).ToString());
            vec_param.Add(et_player_rpcid.ToString());

            mQueProtocol.Enqueue(vec_param);
        }

        //---------------------------------------------------------------------
        // 客户端请求更新消息
        void _c2sRenderUpdate(List<string> vec_param)
        {
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
                    if (mMapClientMethod.ContainsKey(name))
                    {
                        mMapClientMethod[name](l);
                    }
                    else
                    {
                        // log error
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        // 玩家请求获取场景快照
        void _c2sSnapshotScene(List<string> vec_param)
        {
            // 解析数据
            uint et_player_rpcid = uint.Parse(vec_param[1]);

            // 服务端响应玩家获取场景快照
            s2cSnapshotScene(et_player_rpcid);
        }

        //---------------------------------------------------------------------
        // 玩家请求命中计算，无需广播给客户端
        void _c2sFishHit(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int bullet_objid = int.Parse(vec_param[++index]);
            int fish_objid = int.Parse(vec_param[++index]);

            // 处理请求
            mScene.getLevel().c2sFishHit(et_player_rpcid, bullet_objid, fish_objid);
        }

        //---------------------------------------------------------------------
        // 玩家请求渔网命中计算，无需广播给客户端
        void _c2sFishNetHit(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int fish_objid = int.Parse(vec_param[++index]);

            // 处理请求
            mScene.getLevel().c2sFishNetHit(et_player_rpcid, fish_objid);
        }

        //---------------------------------------------------------------------
        // 玩家请求炮台倍率切换，需要广播给客户端
        void _c2sTurretRate(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int rate = int.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sTurretRate(rate);
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交手动发炮，需要广播给客户端
        void _c2sManualFire(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int bullet_objid = int.Parse(vec_param[++index]);
            float turret_angle = float.Parse(vec_param[++index]);
            int turret_rate = int.Parse(vec_param[++index]);
            int locked_fish_id = int.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sManualFire(bullet_objid, turret_angle, turret_rate, locked_fish_id);
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交自动发炮，需要广播给客户端
        void _c2sAutoFire(List<string> vec_param)
        {
            // 解析数据
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

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sAutoFire(que_bullet);
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交锁定鱼，需要广播给客户端
        void _c2sLockFish(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);
            int locked_fish_objid = int.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sLockFish(locked_fish_objid);
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交解锁鱼，需要广播给客户端
        void _c2sUnlockFish(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sUnlockFish();
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交开始长按状态，需要广播给客户端
        void _c2sBeginLongpress(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sBeginLongpress();
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交结束长按状态，需要广播给客户端
        void _c2sEndLongpress(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sEndLongpress();
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交开始极速状态，需要广播给客户端
        void _c2sBeginRapid(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sBeginRapid();
            }
        }

        //---------------------------------------------------------------------
        // 客户端提交结束极速状态，需要广播给客户端
        void _c2sEndRapid(List<string> vec_param)
        {
            // 解析数据
            int index = 0;
            uint et_player_rpcid = uint.Parse(vec_param[++index]);

            // 处理请求
            CLogicTurret turret = mScene.getTurret(et_player_rpcid);
            if (turret != null)
            {
                turret.c2sEndRapid();
            }
        }
    }
}

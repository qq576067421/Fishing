using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public interface IRenderListener
    {
        //---------------------------------------------------------------------
        void onSceneRender2Logic(List<string> vec_param);

        //-------------------------------------------------------------------------
        void onSceneCreated();

        //-------------------------------------------------------------------------
        void onSceneTurretCreated(int index);

        //-------------------------------------------------------------------------
        void onSceneSnapshot();

        //-------------------------------------------------------------------------
        void onSceneShowMessageBox(string content, bool is_ok, string btn_text, int count_down, int info_level, bool is_colider, bool is_change_sit);

        //-------------------------------------------------------------------------
        void onSceneFishDie(uint et_player_rpcid, int player_gold, int fish_vibid, int fish_gold);

        //-------------------------------------------------------------------------
        void onSceneFire(uint et_player_rpcid, int player_gold);

        //-------------------------------------------------------------------------
        void onSceneNoBullet(uint et_player_rpcid);

        //-------------------------------------------------------------------------
        void onScenePlayerChange2Ob();

        //---------------------------------------------------------------------
        void onSetPlayerGold(uint et_player_rpcid, int new_gold);

        //---------------------------------------------------------------------
        void onSetPlayerTurretRate(uint et_player_rpcid, int turret_id, int rate);

        //---------------------------------------------------------------------
        int onGetPlayerGold(uint et_player_rpcid);
    }

    public class CRenderScene : IDisposable
    {
        //-------------------------------------------------------------------------
        IRenderListener mListener = null;
        CSceneBox mSceneBox = null;// 场景盒
        CEffectMgr mEffectMgr = null;// 效果管理器
        CSoundMgr mSoundMgr = null;
        CRenderObjectPool mRenderObjectPool = null;// 资源对象池
        ParticleManager mParticlemanager = null;// 粒子系统管理器，生命周期由他管理。
        CRenderLevel mLevel = null;// 关卡
        CRenderProtocol mProtocol = null;
        Dictionary<uint, CRenderTurret> mMapPlayerTurret = new Dictionary<uint, CRenderTurret>();// key=et_player_rpcid
        uint mMyPlayerId = 0;// 本人玩家id
        CRenderTurret mMyTurret = null;// 本人炮台
        CTurretHelper mTurretHelper;// 用来计算炮台方位及炮台周边物件位置的辅助类
        bool mbSingle = false;// 是否是单机模式
        bool mbInit = false;
        SpriteFishFactory mSpriteFishFactory = null;
        RenderLayerAlloter mLayerAlloter = null;
        string mConfigurePath = "";
        List<JsonPacket> mJsonPacketList = null;
        List<RouteJsonPacket> mRouteJsonPacketList = null;
        public delegate void onSceneLoadingDelegate(float progress, string loading_info);
        public onSceneLoadingDelegate onSceneLoading;
        bool mIsLoadingScene = true;
        LoadableManager mLoadableManager = null;
        RenderConfigure mRenderConfigure = null;
        Queue<ParticleTurnplateCaller> mParticleTurnplateCallerQueue = new Queue<ParticleTurnplateCaller>();
        bool mIsBot = false;
        List<int> mListTurretRate = new List<int>();
        bool mIsRunForeground = true;
        public bool IsRunForeground { get { return mIsRunForeground; } }
        bool mLevelRunInFormation = false;
        float mDisplayStateInfoTime = 0;

        //-------------------------------------------------------------------------
        public CRenderScene()
        {
        }

        //-------------------------------------------------------------------------
        ~CRenderScene()
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
        public void create(uint my_et_player_rpcid, bool single, bool is_bot, IRenderListener listener,
            string configure_filepath, List<JsonPacket> json_packet_list, List<RouteJsonPacket> route_json_packet_list)
        {
            mMyPlayerId = my_et_player_rpcid;
            mbSingle = single;
            mListener = listener;
            mbInit = false;
            mIsBot = is_bot;
            mConfigurePath = configure_filepath;

            InputController.Instance.onFingerTouch += _onFingerTouch;
            InputController.Instance.onFingerLongPress += _onFingerLongPress;
            InputController.Instance.onFingerUp += _onFingerUp;
            InputController.Instance.onFingerDragMove += _onFingerDragMove;
            InputController.Instance.onFingerTouchTurret += _onFingerTouchTurret;
            InputController.Instance.onFingerTouchBuffer += _onFingerTouchBuffer;
            InputController.Instance.onFingerTouchFish += _onFingerTouchFish;

            InputController.Instance.ActiveInput = true;

            CCoordinate.setCoordinate(new EbVector3(Screen.width, Screen.height, 0),
                EbVector3.Zero, new EbVector3(Screen.width, Screen.height, 0));
            mSceneBox = new CSceneBox();
            mProtocol = new CRenderProtocol(this);
            mTurretHelper = new CTurretHelper();
            mLayerAlloter = new RenderLayerAlloter();

            mEffectMgr = new CEffectMgr();
            if (!isBot())
            {
                mEffectMgr.regEffectFactory(new EffectShockScreenFactory());
                mEffectMgr.regEffectFactory(new EffectFrameAnimationFactory());
                mEffectMgr.regEffectFactory(new EffectPlayAudioFactory());
                mEffectMgr.regEffectFactory(new EffectLightingFactory());
                mEffectMgr.regEffectFactory(new EffectLockScreenFactory());
                mEffectMgr.regEffectFactory(new EffectFullScreenFactory());
                mEffectMgr.regEffectFactory(new EffectAOEFactory());
            }
            else
            {
                //mEffectMgr.regEffectFactory(new EffectShockScreenFactory());
                mEffectMgr.regEffectFactory(new EffectFrameAnimationFactory());
                //mEffectMgr.regEffectFactory(new EffectPlayAudioFactory());
                mEffectMgr.regEffectFactory(new EffectLightingFactory());
                mEffectMgr.regEffectFactory(new EffectLockScreenFactory());
                mEffectMgr.regEffectFactory(new EffectFullScreenFactory());
                mEffectMgr.regEffectFactory(new EffectAOEFactory());
            }

            mSpriteFishFactory = new SpriteFishFactory(this);
            mJsonPacketList = json_packet_list;
            mRouteJsonPacketList = route_json_packet_list;

            mRenderConfigure = new RenderConfigure(mConfigurePath);

            mSoundMgr = MbMain.Instance.getSoundMgr();
            mRenderObjectPool = new CRenderObjectPool(this);
            mParticlemanager = new ParticleManager(this);
            mLoadableManager = new LoadableManager();
            mLoadableManager.create(mRenderObjectPool);
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            destroyLevel();

            destroyAllTurret();

            mParticleTurnplateCallerQueue.Clear();

            if (mEffectMgr != null)
            {
                mEffectMgr.destroy();
                mEffectMgr = null;
            }

            CSpriteFishDieScore.DestroyAll();

            mSceneBox = null;
            mListener = null;

            if (mProtocol != null)
            {
                mProtocol.Dispose();
                mProtocol = null;
            }

            if (mSoundMgr != null)
            {
                mSoundMgr.destroyAllSceneSound();
                mSoundMgr = null;
            }

            if (mRenderObjectPool != null)
            {
                mRenderObjectPool.destroy();
                mRenderObjectPool = null;
            }

            if (mParticlemanager != null)
            {
                mParticlemanager.destroy();
                mParticlemanager = null;
            }

            InputController.Instance.onFingerTouch -= _onFingerTouch;
            InputController.Instance.onFingerLongPress -= _onFingerLongPress;
            InputController.Instance.onFingerUp -= _onFingerUp;
            InputController.Instance.onFingerDragMove -= _onFingerDragMove;
            InputController.Instance.onFingerTouchTurret -= _onFingerTouchTurret;
            InputController.Instance.onFingerTouchBuffer -= _onFingerTouchBuffer;
            InputController.Instance.onFingerTouchFish -= _onFingerTouchFish;
            InputController.Instance.ActiveInput = true;

            _uninstallMemory();
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            if (mIsLoadingScene)
            {
                if (mLoadableManager.Loaded)
                {
                    getListener().onSceneCreated();
                    mIsLoadingScene = false;
                    mLoadableManager.destroy();
                    //UiMgr.Instance.destroyCurrentUi<UiMbLoading>();
                    return;
                }

                mLoadableManager.update(elapsed_tm);
                _loadingInfo(mLoadableManager.Progress, mLoadableManager.LoadingInfo);
                return;
            }

            mRenderObjectPool.update();// 要先于Level Update

            mProtocol.update(elapsed_tm);

            foreach (var it in mParticleTurnplateCallerQueue)
            {
                it.update(elapsed_tm);
            }

            mParticlemanager.update(elapsed_tm);

            // 根据屏幕分辨率更新坐标
            Resolution cur_resolution = Screen.currentResolution;
            CCoordinate.setCoordinate(new EbVector3(Screen.width, Screen.height, 0),
                EbVector3.Zero, new EbVector3(Screen.width, Screen.height, 0));

            // 更新炮台
            foreach (var i in mMapPlayerTurret)
            {
                i.Value.update(elapsed_tm);
            }

            // 更新关卡
            if (mLevel != null)
            {
                mLevel.update(elapsed_tm);
            }

            // 更新效果管理器
            mEffectMgr.update(elapsed_tm);

            // 更新所有鱼死亡分数显示
            CSpriteFishDieScore.updateall(elapsed_tm);

            // 本人正在观战
            if (mMyTurret == null && mLevel != null)
            {
                string str = "正在观战中．．．";
                //getListener().onSceneShowMessageBox(str, false, "", 1, (int)_eMessageBoxLayer.Ob, true, true);
            }

            displaySceneStateInfo(elapsed_tm);
        }

        //-------------------------------------------------------------------------
        public void sceneOnRecvAoIFromLogic(List<string> vec_param)
        {
            mProtocol.onRecvAoI(vec_param);
        }

        //-------------------------------------------------------------------------
        public void sceneOnRecvFromLogic(List<string> vec_param)
        {
            mProtocol.onRecv(vec_param);
        }

        //-------------------------------------------------------------------------
        public void c2sSnapshotScene()
        {
            // 客户端请求获取场景快照
            mProtocol.c2sSnapshotScene(mMyPlayerId);
        }

        //-------------------------------------------------------------------------
        // 使能场景输入
        public void enableSceneInput(bool enable)
        {
            InputController.Instance.ActiveInput = enable;
        }

        //-------------------------------------------------------------------------
        // 获取场景输入的使能状态
        public bool enableSceneInput()
        {
            return InputController.Instance.ActiveInput;
        }

        //-------------------------------------------------------------------------
        // 是否是单机模式
        public bool isSingleMode()
        {
            return mbSingle;
        }

        //-------------------------------------------------------------------------
        // 是否是机器人模式
        public bool isBot()
        {
            return mIsBot;
        }

        //-------------------------------------------------------------------------
        // 是否初始化
        public bool isInit()
        {
            return mbInit;
        }

        //-------------------------------------------------------------------------
        // 设置为已初始化
        public void setInit()
        {
            mbInit = true;
        }

        //-------------------------------------------------------------------------
        public RenderLayerAlloter getLayerAlloter()
        {
            return mLayerAlloter;
        }

        //-------------------------------------------------------------------------
        public CRenderObjectPool getRenderObjectPool()
        {
            return mRenderObjectPool;
        }

        //-------------------------------------------------------------------------
        public RenderConfigure getRenderConfigure()
        {
            return mRenderConfigure;
        }

        //-------------------------------------------------------------------------
        public ParticleManager getParticlemanager()
        {
            return mParticlemanager;
        }

        //-------------------------------------------------------------------------
        public IRenderListener getListener()
        {
            return mListener;
        }

        //-------------------------------------------------------------------------
        public CRenderLevel getLevel()
        {
            return mLevel;
        }

        //-------------------------------------------------------------------------
        public EbVector3 getAvatarPosition(int turret_id)
        {
            return CCoordinate.logic2pixelPos(
                  getTurretHelper().getPositionByOffset(turret_id, getRenderConfigure().TurretAvatarOffset));
        }

        //-------------------------------------------------------------------------
        public float getAvatarAngle(int turret_id)
        {
            return getTurretHelper().getBaseAngleByTurretId(turret_id);
        }

        //-------------------------------------------------------------------------
        public CRenderProtocol getProtocol()
        {
            return mProtocol;
        }

        //-------------------------------------------------------------------------
        public float getSceneLength()
        {
            return CCoordinate.LogicSceneLength;
        }

        //-------------------------------------------------------------------------
        public float getSceneWidth()
        {
            return CCoordinate.LogicSceneWidth;
        }

        //-------------------------------------------------------------------------
        public void setRateList(List<int> rate_list)
        {
            mListTurretRate = rate_list;
        }

        //-------------------------------------------------------------------------
        public List<int> getRateList()
        {
            return mListTurretRate;
        }

        //-------------------------------------------------------------------------
        public CTurretHelper getTurretHelper()
        {
            return mTurretHelper;
        }

        //-------------------------------------------------------------------------
        public CSceneBox getSceneBox()
        {
            return mSceneBox;
        }

        //-------------------------------------------------------------------------
        public uint getMyPlayerId()
        {
            return mMyPlayerId;
        }

        //-------------------------------------------------------------------------
        public void setMyPlayerId(uint my_et_player_rpcid)
        {
            mMyPlayerId = my_et_player_rpcid;
        }

        //-------------------------------------------------------------------------
        public void setPlayerGold(uint et_player_rpcid, int gold)
        {
            mListener.onSetPlayerGold(et_player_rpcid, gold);
        }

        //-------------------------------------------------------------------------
        public void setTurret(uint et_player_rpcid, TbDataTurret.TurretType turret_type)
        {
            CRenderTurret turret = getTurret(et_player_rpcid);
            if (turret == null) return;
            turret.setTurret(turret_type);
        }

        //-------------------------------------------------------------------------
        public CRenderTurret getMyTurret()
        {
            return mMyTurret;
        }

        //-------------------------------------------------------------------------
        public void setMyTurret(CRenderTurret my_turret)
        {
            mMyTurret = my_turret;
        }

        //-------------------------------------------------------------------------
        public void addEffect(int effect_vib_id, Dictionary<string, object> map_param, EffectTypeEnum effect_type)
        {
            if (mEffectMgr != null && map_param != null)
            {
                map_param.Add("RenderScene", this);
                mEffectMgr.createCombineEffect(effect_vib_id, map_param, effect_type);
            }
        }

        //-------------------------------------------------------------------------
        public void addSingleEffect(int effect_id, string effect_name, int effect_type, float delay_time,
            Dictionary<string, object> map_param, EffectTypeEnum effect_generate_type)
        {
            if (mEffectMgr != null && map_param != null)
            {
                map_param.Add("RenderScene", this);
                mEffectMgr.createEffect(effect_id, effect_name, effect_type, delay_time, map_param, effect_generate_type);
            }
        }

        //-------------------------------------------------------------------------
        public CSoundMgr getSoundMgr()
        {
            return mSoundMgr;
        }

        //-------------------------------------------------------------------------
        public SpriteFishFactory getSpriteFishFactory()
        {
            return mSpriteFishFactory;
        }

        //-------------------------------------------------------------------------
        public CEffectMgr getEffectMgr()
        {
            return mEffectMgr;
        }

        //-------------------------------------------------------------------------
        public Dictionary<uint, CRenderTurret> getMapTurret()
        {
            return mMapPlayerTurret;
        }

        //-------------------------------------------------------------------------
        public CRenderTurret getTurret(uint et_player_rpcid)
        {
            if (mMapPlayerTurret.ContainsKey(et_player_rpcid))
            {
                return mMapPlayerTurret[et_player_rpcid];
            }
            else return null;
        }

        //-------------------------------------------------------------------------
        public void addParticleTurnplateCaller(ParticleTurnplateCaller caller)
        {
            mParticleTurnplateCallerQueue.Enqueue(caller);
        }

        //-------------------------------------------------------------------------
        public bool isExistPlayer(uint et_player_rpcid)
        {
            if (mMapPlayerTurret.ContainsKey(et_player_rpcid))
            {
                return true;
            }
            else return false;
        }

        //-------------------------------------------------------------------------
        public void destroyLevel()
        {
            if (mLevel != null)
            {
                mLevel.Dispose();
                mLevel = null;
            }
        }

        //-------------------------------------------------------------------------
        public void createLevel(_eLevelState level_state, int level_vibid, int cur_map_vibid,
            int next_map_vibid, float level_run_totalsecond, float level_run_maxsecond, bool level_run_in_formation)
        {
            mLevel = new CRenderLevel(this, mConfigurePath);
            mLevel.create(level_state, level_vibid, cur_map_vibid, next_map_vibid, level_run_totalsecond, level_run_maxsecond, mJsonPacketList, mRouteJsonPacketList);

            if (level_run_in_formation)
            {
                //getListener().onSceneShowMessageBox("请稍等，大鱼正在游来。。。", false, "", 10, (int)_eMessageBoxLayer.SwitchLevel, false, false);
            }

            mLevelRunInFormation = level_run_in_formation;
        }

        //-------------------------------------------------------------------------
        public void displaySceneStateInfo(float delta_tm = 0)
        {
            if (mLevelRunInFormation)
            {
                mDisplayStateInfoTime -= delta_tm;
                if (mDisplayStateInfoTime > 0) return;
                mDisplayStateInfoTime = 3;
                //getListener().onSceneShowMessageBox("请稍等，大鱼正在游来。。。", false, "", (int)mDisplayStateInfoTime, (int)_eMessageBoxLayer.SwitchLevel, false, false);
            }
        }

        //-------------------------------------------------------------------------
        public void resetRunInFormation()
        {
            mLevelRunInFormation = false;
        }

        //-------------------------------------------------------------------------
        public void destroyAllTurret()
        {
            foreach (var i in mMapPlayerTurret)
            {
                i.Value.Dispose();
            }
            mMapPlayerTurret.Clear();
            mMyTurret = null;
        }

        //-------------------------------------------------------------------------
        public CRenderTurret createTurret(int turret_id, ref _tScenePlayer scene_player, int player_gold,
            bool buffer_power, bool buffer_freeze, bool buffer_longpress, bool buffer_rapid,
            int turret_rate, float turret_angle, int locked_fish_objid, TbDataTurret.TurretType turret_type)
        {
            CRenderTurret turret = new CRenderTurret(this);
            turret.create(turret_id, ref scene_player, player_gold, buffer_power, buffer_freeze, buffer_longpress,
                    buffer_rapid, turret_rate, turret_angle, locked_fish_objid, turret_type);
            mMapPlayerTurret[scene_player.et_player_rpcid] = turret;
            return turret;
        }

        //-----------------------------------------------------------------------------
        public void switch2RunBackground()
        {
            mIsRunForeground = false;
        }

        //-----------------------------------------------------------------------------
        public void switch2RunForeground()
        {
            mIsRunForeground = true;
        }

        //-----------------------------------------------------------------------------
        public void fixedUpdate(float fixed_elapsed_tm)
        {
            if (mLevel == null) return;
            mLevel.fixedUpdate(fixed_elapsed_tm);
        }

        //-------------------------------------------------------------------------
        // 手势触摸消息
        void _onFingerTouch(Vector2 fire_goal_position)
        {
            if (mMyTurret != null)
            {
                mMyTurret.onFingerTouch(fire_goal_position);
            }
            else
            {
                Debug.LogWarning("CRenderScene::_onFingerTouch() mMyTurret==null");
            }
        }

        //-------------------------------------------------------------------------
        // 手势滑动消息
        void _onFingerTouchFish(List<FishStillSprite> fishs)
        {
            if (mMyTurret != null)
            {
                mMyTurret.onFingerTouchFish(fishs);
            }
            else
            {
                Debug.LogWarning("CRenderScene::_onFingerTouch() mMyTurret==null");
            }
        }

        //-------------------------------------------------------------------------
        // 手势触摸到炮台对应的物件
        void _onFingerTouchTurret(int turret_id)
        {
            if (mMyTurret != null && mMyTurret.getTurretId() == turret_id)
            {
                //mMyTurret.requestSwitchTurretRate();
            }
        }

        //-------------------------------------------------------------------------
        public void setMyTurretRate(int rate)
        {
            if (mMyTurret != null)
            {
                mMyTurret.requestSwitchTurretRate(rate);
            }
        }

        //-------------------------------------------------------------------------
        // 触摸到buffer
        void _onFingerTouchBuffer(GameObject buffer)
        {
            foreach (var it in mMapPlayerTurret)
            {
                it.Value.onFingerTouchBuffer(buffer);
            }
        }

        //-------------------------------------------------------------------------
        // 手势抬起消息
        void _onFingerUp()
        {
            if (mMyTurret != null)
            {
                mMyTurret.onFingerUp();
            }
        }

        //-------------------------------------------------------------------------
        // 手势拖动消息
        void _onFingerDragMove(Vector2 fire_goal_position)
        {
            if (mMyTurret != null)
            {
                mMyTurret.onFingerDragMove(fire_goal_position);
            }
        }

        //-------------------------------------------------------------------------
        // 手势长按消息
        void _onFingerLongPress(Vector2 fire_goal_position)
        {
            if (mMyTurret != null)
            {
                mMyTurret.onFingerLongPress(fire_goal_position);
            }
        }

        //-------------------------------------------------------------------------
        // 应用程序切换后台
        void _appRunInBackground()
        {
            if (mMyTurret != null)
            {
            }
        }

        //-------------------------------------------------------------------------
        // 应用程序切换前台
        void _appRunInFront()
        {
            if (mMyTurret != null)
            {
            }
        }

        //-------------------------------------------------------------------------
        void _uninstallMemory()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            //Resources.UnloadUnusedAssets();
        }

        //-------------------------------------------------------------------------
        void _loadingInfo(float progress, string loading_info)
        {
            if (onSceneLoading == null) return;
            onSceneLoading(progress, loading_info);
        }
    }
}

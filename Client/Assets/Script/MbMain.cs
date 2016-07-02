using UnityEngine;
using System.Collections;
using GF.Common;
using Ps;

public class EcEngineListener : IEcEngineListener
{
    //-------------------------------------------------------------------------
    public void init(EntityMgr entity_mgr, Entity et_root)
    {
        entity_mgr.regComponent<ClientActor<DefActor>>();
        entity_mgr.regComponent<ClientActorMirror<DefActorMirror>>();
        entity_mgr.regComponent<ClientApp<DefApp>>();
        entity_mgr.regComponent<ClientBag<DefBag>>();
        entity_mgr.regComponent<ClientDataEye<DefDataEye>>();
        entity_mgr.regComponent<ClientEquip<DefEquip>>();
        entity_mgr.regComponent<ClientEquipMirror<DefEquipMirror>>();
        entity_mgr.regComponent<ClientLaunch<DefLaunch>>();
        entity_mgr.regComponent<ClientLogin<DefLogin>>();
        entity_mgr.regComponent<ClientNetMonitor<DefNetMonitor>>();
        entity_mgr.regComponent<ClientPlayer<DefPlayer>>();
        entity_mgr.regComponent<ClientPlayerChat<DefPlayerChat>>();
        entity_mgr.regComponent<ClientPlayerDesktop<DefPlayerDesktop>>();
        entity_mgr.regComponent<ClientPlayerFriend<DefPlayerFriend>>();
        entity_mgr.regComponent<ClientPlayerLobby<DefPlayerLobby>>();
        entity_mgr.regComponent<ClientPlayerMirror<DefPlayerMirror>>();
        entity_mgr.regComponent<ClientPlayerMailBox<DefPlayerMailBox>>();
        entity_mgr.regComponent<ClientPlayerTask<DefPlayerTask>>();
        entity_mgr.regComponent<ClientPlayerTrade<DefPlayerTrade>>();
        entity_mgr.regComponent<ClientPlayerRanking<DefPlayerRanking>>();
        entity_mgr.regComponent<ClientSound<DefSound>>();
        entity_mgr.regComponent<ClientStatus<DefStatus>>();
        entity_mgr.regComponent<ClientStatusMirror<DefStatusMirror>>();
        entity_mgr.regComponent<ClientSysNotice<DefSysNotice>>();
        entity_mgr.regComponent<ClientUCenterSDK<DefUCenterSDK>>();

        entity_mgr.regEntityDef<EtApp>();
        entity_mgr.regEntityDef<EtFishing>();
        entity_mgr.regEntityDef<EtLaunch>();
        entity_mgr.regEntityDef<EtLogin>();
        entity_mgr.regEntityDef<EtPlayer>();
        entity_mgr.regEntityDef<EtPlayerMirror>();
        entity_mgr.regEntityDef<EtUCenterSDK>();
    }

    //-------------------------------------------------------------------------
    public void release()
    {
    }
}

public class MbMain : MonoBehaviour
{
    //-------------------------------------------------------------------------
    UiMgr UiMgr { get; set; }

    //-------------------------------------------------------------------------
    static MbMain mMbMain;
    ClientConfig mClientConfig;
    EcEngine mEngine;
    CSoundMgr mSoundMgr;
    PathMgr mPathMgr;
    AsyncLoadAssetMgr mAsyncLoadAssetMgr;

    //-------------------------------------------------------------------------
    public PathMgr PathMgr { get { return mPathMgr; } }
    public ClientConfig ClientConfig { get { return mClientConfig; } }
    public AsyncLoadAssetMgr AsyncLoadAssetMgr { get { return mAsyncLoadAssetMgr; } }

    //-------------------------------------------------------------------------
    static public MbMain Instance
    {
        get { return mMbMain; }
    }

    //-------------------------------------------------------------------------
    void Awake()
    {
        mMbMain = this;
    }

    //-------------------------------------------------------------------------
    void Start()
    {
        // 初始化系统参数
        {
            Application.runInBackground = true;
            Time.fixedDeltaTime = 0.03f;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        // 初始化日志
        {
            EbLog.NoteCallback = Debug.Log;
            EbLog.WarningCallback = Debug.LogWarning;
            EbLog.ErrorCallback = Debug.LogError;
        }

        EbLog.Note("MbMain.Start()");

        // 初始化PathMgr
        if (mPathMgr == null)
        {
            mPathMgr = new PathMgr();
        }

        if (mClientConfig == null)
        {
            mClientConfig = new ClientConfig();
        }

        if (mAsyncLoadAssetMgr == null)
        {
            mAsyncLoadAssetMgr = new AsyncLoadAssetMgr();
            mAsyncLoadAssetMgr.init();
        }

        if (mEngine == null)
        {
            EcEngineSettings settings;
            settings.ProjectName = "Fishing";
            settings.RootEntityType = "EtRoot";
            settings.EnableCoSuperSocket = true;
            mEngine = new EcEngine(ref settings, new EcEngineListener());
        }

        UiMgr = new UiMgr();
        UiMgr.create(EntityMgr.Instance);

        mSoundMgr = new CSoundMgr();

        // 创建EtClientApp
        EntityMgr.Instance.createEntity<EtApp>(null, EcEngine.Instance.EtNode);
    }

    //-------------------------------------------------------------------------
    void Update()
    {
        if (mEngine != null)
        {
            mEngine.update(Time.deltaTime);
        }

        if (UiMgr != null)
        {
            UiMgr.update(Time.deltaTime);
        }

        if (mSoundMgr != null)
        {
            mSoundMgr.update();
        }

        if (mAsyncLoadAssetMgr != null)
        {
            mAsyncLoadAssetMgr.update(Time.deltaTime);
        }
    }

    //-------------------------------------------------------------------------
    void FixedUpdate()
    {
        var ev = EntityMgr.Instance.getDefaultEventPublisher().genEvent<EvEntityFixedUpdate>();
        ev.tm = Time.fixedDeltaTime;
        ev.send(null);
    }

    //-------------------------------------------------------------------------
    void OnDestroy()
    {
        _destory();
    }

    //-------------------------------------------------------------------------
    void OnApplicationQuit()
    {
        _destory();
    }

    //-------------------------------------------------------------------------
    void OnApplicationFocus(bool focusStatus)
    {
    }

    //-------------------------------------------------------------------------
    public void setupMain()
    {
        // 初始化数据管理
        string db_filename = mPathMgr.combinePersistentDataPath("/NotPackAsset/Media/Fishing/Config/Fishing.db");
        TbDataMgr.setup(db_filename);

        // 初始化单位模块
        UnitSys.setup(true);

        // 初始化效果系统
        //EffectSys.regEffect(new EffectMaterialCompound());
        //EffectSys.regEffect(new EffectSceneProduceMonster());
        //EffectSys.regEffect(new EffectEquipPropFireAttackPoint());
        //EffectSys.regEffect(new EffectEquipPropFireAttackPointRange());
        //EffectSys.regEffect(new EffectEquipPropFireDefensePoint());
        //EffectSys.regEffect(new EffectEquipPropFireDefensePointRange());
    }

    //-------------------------------------------------------------------------
    public CSoundMgr getSoundMgr()
    {
        return mSoundMgr;
    }

    //-------------------------------------------------------------------------
    void _destory()
    {
        if (mEngine == null) return;

        if (mEngine != null)
        {
            mEngine.close();
            mEngine = null;
        }

        if (UiMgr != null)
        {
            UiMgr.destroy();
            UiMgr = null;
        }

        if (mSoundMgr != null)
        {
            mSoundMgr.destroy();
            mSoundMgr = null;
        }

        if (mAsyncLoadAssetMgr != null)
        {
            mAsyncLoadAssetMgr.destory();
            mAsyncLoadAssetMgr = null;
        }

        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        EbLog.Note("MbMain._destory()");
    }
}

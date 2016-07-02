using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Eb;
using Es;
using Ps;

public class CellApp
{
    //-------------------------------------------------------------------------
    static CellApp mCellApp;
    //EbScriptMgr mScriptMgr = new EbScriptMgr();
    Dictionary<string, BtFactory> mMapBtFactory = new Dictionary<string, BtFactory>();

    //-------------------------------------------------------------------------
    public static CellApp Instance { get { return mCellApp; } }
    public ServerConfig ServerConfig { get; private set; }
    public EffectSys EffectSys { get; private set; }

    //-------------------------------------------------------------------------
    public CellApp()
    {
        mCellApp = this;
        ServerConfig = new ServerConfig();

        // 初始化DataMgr
        {
            string path_media = ServerPath.getPathMediaRoot();
            string db_filename = Path.Combine(path_media, "Dragon\\Config\\Dragon.db");
            EbLog.Note(db_filename);
            TbDataMgr.setup(db_filename);
        }

        // 初始化ScriptMgr
        //{
        //string path_media = ServerPath.getPathMediaRoot();
        //string dir_script = Path.Combine(path_media, "Dragon\\Script\\S\\");
        //mScriptMgr.create(dir_script);

        //List<string> list_param = new List<string>();
        //list_param.Add("102");
        //Effect.doEffect(Entity, 1901, list_param);
        //}

        // 初始化单位模块
        UnitSys.setup(false);

        // 初始化效果系统
        EffectSys = new EffectSys(true);
        EffectSys.regEffect(new EffectActorPropAttackPoint());
        EffectSys.regEffect(new EffectActorPropAttackSpeed());
        EffectSys.regEffect(new EffectActorPropCriticalHitPoint());
        EffectSys.regEffect(new EffectActorPropDefencePoint());
        EffectSys.regEffect(new EffectActorPropDodgePoint());
        EffectSys.regEffect(new EffectActorPropEnergyPointCur());
        EffectSys.regEffect(new EffectActorPropEnergyPointMax());
        EffectSys.regEffect(new EffectActorPropFireEnhancementPoint());
        EffectSys.regEffect(new EffectActorPropFireResistancePoint());
        EffectSys.regEffect(new EffectActorPropHealthPointCur());
        EffectSys.regEffect(new EffectActorPropHealthPointMax());
        EffectSys.regEffect(new EffectActorPropHitPoint());
        EffectSys.regEffect(new EffectActorPropMetalEnhancementPoint());
        EffectSys.regEffect(new EffectActorPropMetalResistancePoint());
        EffectSys.regEffect(new EffectActorPropSoilEnhancementPoint());
        EffectSys.regEffect(new EffectActorPropSoilResistancePoint());
        EffectSys.regEffect(new EffectActorPropTenacityPoint());
        EffectSys.regEffect(new EffectActorPropWaterEnhancementPoint());
        EffectSys.regEffect(new EffectActorPropWaterResistancePoint());
        EffectSys.regEffect(new EffectActorPropWoodEnhancementPoint());
        EffectSys.regEffect(new EffectActorPropWoodResistancePoint());
        EffectSys.regEffect(new EffectCreateStatus());
        EffectSys.regEffect(new EffectLearnSkill());
        EffectSys.regEffect(new EffectSkillPropAttackPoint());
        EffectSys.regEffect(new EffectSkillPropAttackSpeed());
        EffectSys.regEffect(new EffectSkillPropCriticalHitPoint());
        EffectSys.regEffect(new EffectSkillPropDefencePoint());
        EffectSys.regEffect(new EffectSkillPropDodgePoint());
        EffectSys.regEffect(new EffectSkillPropEnergyPointCur());
        EffectSys.regEffect(new EffectSkillPropEnergyPointMax());
        EffectSys.regEffect(new EffectSkillPropFireEnhancementPoint());
        EffectSys.regEffect(new EffectSkillPropFireResistancePoint());
        EffectSys.regEffect(new EffectSkillPropHealthPointCur());
        EffectSys.regEffect(new EffectSkillPropHealthPointMax());
        EffectSys.regEffect(new EffectSkillPropHitPoint());
        EffectSys.regEffect(new EffectSkillPropMetalEnhancementPoint());
        EffectSys.regEffect(new EffectSkillPropMetalResistancePoint());
        EffectSys.regEffect(new EffectSkillPropSoilEnhancementPoint());
        EffectSys.regEffect(new EffectSkillPropSoilResistancePoint());
        EffectSys.regEffect(new EffectSkillPropTenacityPoint());
        EffectSys.regEffect(new EffectSkillPropWaterEnhancementPoint());
        EffectSys.regEffect(new EffectSkillPropWaterResistancePoint());
        EffectSys.regEffect(new EffectSkillPropWoodEnhancementPoint());
        EffectSys.regEffect(new EffectSkillPropWoodResistancePoint());
        EffectSys.regEffect(new EffectTakeoffEquip());
        EffectSys.regEffect(new EffectTakeonEquip());

        //// 创建EtWorld子Entity
        //Entity et_world = EntityMgr.createEntity<EtWorld>(null, Entity);
        //CoWorld = et_world.getComponent<CellWorld<ComponentDef>>();

        // 注册BtFactory
        _regBtFactory(new BtFactoryBossNoraml());
        _regBtFactory(new BtFactoryBossNoramlMirror());
        _regBtFactory(new BtFactoryBot());
        _regBtFactory(new BtFactoryBotMirror());
        _regBtFactory(new BtFactoryMonsterNormal());
        _regBtFactory(new BtFactoryMonsterNormalMirror());
        _regBtFactory(new BtFactoryPlayer());
        _regBtFactory(new BtFactoryPlayerMirror());

        // 加载所有Bot
        var map_databot = EbDataMgr.Instance.getMapData<TbDataBot>();
        foreach (var i in map_databot)
        {
            TbDataBot data_bot = (TbDataBot)i.Value;

            var player = GrainFactory.GetGrain<ICellPlayer>(new Guid(data_bot.EtGuid));
            player.botNewAndEnterWorld(data_bot.NickName);
        }
    }

    //-------------------------------------------------------------------------
    public void close()
    {
    }

    //-------------------------------------------------------------------------
    public Bt createBt(string bt_name, Entity self)
    {
        BtFactory bt_factory = null;
        mMapBtFactory.TryGetValue(bt_name, out bt_factory);

        if (bt_factory == null) return null;
        else return bt_factory.createBt(self);
    }

    //-------------------------------------------------------------------------
    void _regBtFactory(BtFactory bt_factory)
    {
        mMapBtFactory[bt_factory.getName()] = bt_factory;
    }
}

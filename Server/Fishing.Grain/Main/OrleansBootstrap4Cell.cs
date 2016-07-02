using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Orleans;
using Orleans.Providers;
using Couchbase;
using Couchbase.Core;
using Couchbase.N1QL;
using GF.Common;
using GF.Server;

namespace Ps
{
    public class OrleansBootstrap4Cell : IBootstrapProvider
    {
        //---------------------------------------------------------------------
        public string Name { get { return "OrleansBootstrap4Cell"; } }
        public DbClientCouchbase DbClientCouchbase { get; private set; }
        public EntityMgr EntityMgr { get; private set; }
        public CellApp CellApp { get; private set; }

        //---------------------------------------------------------------------
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            EntityMgr = new EntityMgr((byte)NodeType.Cell, NodeType.Cell.ToString());

            EntityMgr.regComponent<CellActor<DefActor>>();
            EntityMgr.regComponent<CellActorMirror<DefActorMirror>>();
            EntityMgr.regComponent<CellActorMirrorAi<DefActorMirrorAi>>();
            EntityMgr.regComponent<CellBag<DefBag>>();
            EntityMgr.regComponent<CellDesktop<DefDesktop>>();
            EntityMgr.regComponent<CellEquip<DefEquip>>();
            EntityMgr.regComponent<CellPlayer<DefPlayer>>();
            EntityMgr.regComponent<CellPlayerChat<DefPlayerChat>>();
            EntityMgr.regComponent<CellPlayerDesktop<DefPlayerDesktop>>();
            EntityMgr.regComponent<CellPlayerFriend<DefPlayerFriend>>();
            EntityMgr.regComponent<CellPlayerLobby<DefPlayerLobby>>();
            EntityMgr.regComponent<CellPlayerMailBox<DefPlayerMailBox>>();
            EntityMgr.regComponent<CellPlayerTask<DefPlayerTask>>();
            EntityMgr.regComponent<CellPlayerTrade<DefPlayerTrade>>();
            EntityMgr.regComponent<CellPlayerRanking<DefPlayerRanking>>();
            EntityMgr.regComponent<CellStatus<DefStatus>>();

            EntityMgr.regEntityDef<EtDesktop>();
            EntityMgr.regEntityDef<EtPlayer>();
            EntityMgr.regEntityDef<EtPlayerMirror>();

            DbClientCouchbase = new DbClientCouchbase();
            EntityCouchbase et_couchbase = new EntityCouchbase(EntityMgr, DbClientCouchbase.Bucket);

            // 创建视图
            //var couchbase_mgr = DbClientCouchbase.Instance.Bucket.CreateManager("Cragon", "123321");
            //var get = couchbase_mgr.GetDesignDocument("dev_team");
            //if (!get.Success)
            //{
            //    var design_doc = File.ReadAllText(@"..\\..\\..\\Media\\Fishing\\CouchbaseView\\dev_team.json");
            //    var inserted = couchbase_mgr.InsertDesignDocument("dev_team", design_doc);
            //    if (inserted.Success)
            //    {
            //        EbLog.Note("Created 'team' design doc.  Success");
            //    }
            //    else
            //    {
            //        EbLog.Note("Created 'team' design doc.  Failed, Msg=" + inserted.Message);
            //    }
            //}

            // 初始化DataMgr
            {
                string path_media = ServerPath.getPathMediaRoot();
                string db_filename = Path.Combine(path_media, "Fishing\\Config\\Fishing.db");
                EbLog.Note(db_filename);
                TbDataMgr.setup(db_filename);
            }

            // 初始化单位模块
            UnitSys.setup(false);

            // 初始化效果系统
            EffectSys.regEffect(new EffectCreateStatus());
            EffectSys.regEffect(new EffectStatus1());
            EffectSys.regEffect(new EffectStatus2());
            EffectSys.regEffect(new EffectStatusCreator1());
            EffectSys.regEffect(new EffectStatusCreator2());
            EffectSys.regEffect(new EffectTakeoffEquip());
            EffectSys.regEffect(new EffectTakeonEquip());
            EffectSys.regEffect(new EffectUseConsumable());
            EffectSys.regEffect(new EffectUseSkillBook());

            // 初始化CellApp
            CellApp = new CellApp();

            //// 加载所有Bot
            //var map_databot = EbDataMgr.Instance.getMapData<TbDataBot>();
            //foreach (var i in map_databot)
            //{
            //    TbDataBot data_bot = (TbDataBot)i.Value;

            //    //var grain_player = CellPlayerFactory.GetGrain(new Guid(data_bot.EtGuid));
            //    //var grain_player = GrainFactory.GetGrain<ICellPlayer>(new Guid(data_bot.EtGuid));
            //    //await grain_player.botNewAndEnterWorld(data_bot.NickName);
            //}

            //return TaskDone.Done;
        }

        //---------------------------------------------------------------------
        public Task Close()
        {
            return TaskDone.Done;
        }
    }
}

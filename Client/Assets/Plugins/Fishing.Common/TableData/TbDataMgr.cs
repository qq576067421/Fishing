using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    //-----------------------------------------------------------------------------
    // Fishing Db中所有表名
    public enum _eTableName
    {
        ActorLevel,
        ActorType,
        Bot,
        ColorCode,
        ChatIco,
        ChatIcoType,
        DesktopInfo,
        Effect,
        Item,
        ItemType,
        OperateType,
        Player,
        PlayerProfileSkin,
        Status,
        Task,
        TaskCollectItem,
        TaskDialogue,
        TaskKillMonster,
        Tips,
        Trade,
        TradeInfo,
        UnitConsumables,

        //fish
        Particle,
        EffectName,
        Buff,
        EffectAOE,
        EffectCompose,
        EffectCreateCoin,
        EffectFlySword,
        EffectFrameAnimation,
        EffectFullScreen,
        EffectLockScreen,
        EffectPlayAudio,
        EffectScreenShock,
        EffectSeaWave,
        EffectTrace,

        Bullet,
        EffectHypericumBullet,


        FishEvenFour,
        FishEvenFive,
        OutFishGroup,
        FishEachCompose,
        FishCompose,
        Fish,
        EffectRadiationLighting,
        EffectSpreadFish,

        Formation,

        Route,
        Shoal,

        BackgroundSprite,

        Fishnet,

        SingleDuringLevelProp,
        SinglePreLevelProp,
        GeneralLevel,
        Level,

        Map,
        PlayerExp,
        PlayerHead,
        PlayerProp,
        PlayerTitle,
        Room,

        SinglePlayerLife,
        SpecialLevel,
        VipLevel,
        Turret,
    }

    public static class TbDataMgr
    {
        //-------------------------------------------------------------------------
        static EbDataMgr mDataMgr = new EbDataMgr();

        //-------------------------------------------------------------------------
        public static void setup(string db_filename)
        {
            mDataMgr.setup("Fishing", db_filename);

            mDataMgr.loadData<TbDataOperateType>(_eTableName.OperateType.ToString());
            mDataMgr.loadData<TbDataActorLevel>(_eTableName.ActorLevel.ToString());
            mDataMgr.loadData<TbDataBot>(_eTableName.Bot.ToString());
            mDataMgr.loadData<TbDataChatIco>(_eTableName.ChatIco.ToString());
            mDataMgr.loadData<TbDataChatIcoType>(_eTableName.ChatIcoType.ToString());
            mDataMgr.loadData<TbDataColorCode>(_eTableName.ColorCode.ToString());
            mDataMgr.loadData<TbDataDesktopInfo>(_eTableName.DesktopInfo.ToString());
            mDataMgr.loadData<TbDataEffect>(_eTableName.Effect.ToString());
            mDataMgr.loadData<TbDataItem>(_eTableName.Item.ToString());
            mDataMgr.loadData<TbDataItemType>(_eTableName.ItemType.ToString());
            mDataMgr.loadData<TbDataPlayer>(_eTableName.Player.ToString());
            mDataMgr.loadData<TbDataPlayerProfileSkin>(_eTableName.PlayerProfileSkin.ToString());
            mDataMgr.loadData<TbDataStatus>(_eTableName.Status.ToString());
            mDataMgr.loadData<TbDataTask>(_eTableName.Task.ToString());
            mDataMgr.loadData<TbDataTaskCollectItem>(_eTableName.TaskCollectItem.ToString());
            mDataMgr.loadData<TbDataTaskDialogue>(_eTableName.TaskDialogue.ToString());
            mDataMgr.loadData<TbDataTaskKillMonster>(_eTableName.TaskKillMonster.ToString());
            mDataMgr.loadData<TbDataTips>(_eTableName.Tips.ToString());
            mDataMgr.loadData<TbDataTrade>(_eTableName.Trade.ToString());
            mDataMgr.loadData<TbDataTradeInfo>(_eTableName.TradeInfo.ToString());
            mDataMgr.loadData<TbDataUnitConsumables>(_eTableName.UnitConsumables.ToString());

            //fish
            mDataMgr.loadData<TbDataParticle>(_eTableName.Particle.ToString());
            mDataMgr.loadData<TbDataEffectName>(_eTableName.EffectName.ToString());
            mDataMgr.loadData<TbDataBuff>(_eTableName.Buff.ToString());
            mDataMgr.loadData<TbDataEffectAOE>(_eTableName.EffectAOE.ToString());
            mDataMgr.loadData<TbDataEffectCompose>(_eTableName.EffectCompose.ToString());
            mDataMgr.loadData<TbDataEffectCreateCoin>(_eTableName.EffectCreateCoin.ToString());
            mDataMgr.loadData<TbDataEffectFlySword>(_eTableName.EffectFlySword.ToString());
            mDataMgr.loadData<TbDataEffectFrameAnimation>(_eTableName.EffectFrameAnimation.ToString());
            mDataMgr.loadData<TbDataEffectFullScreen>(_eTableName.EffectFullScreen.ToString());
            mDataMgr.loadData<TbDataEffectLockScreen>(_eTableName.EffectLockScreen.ToString());
            mDataMgr.loadData<TbDataEffectPlayAudio>(_eTableName.EffectPlayAudio.ToString());
            mDataMgr.loadData<TbDataEffectScreenShock>(_eTableName.EffectScreenShock.ToString());
            mDataMgr.loadData<TbDataEffectSeaWave>(_eTableName.EffectSeaWave.ToString());
            mDataMgr.loadData<TbDataEffectTrace>(_eTableName.EffectTrace.ToString());
            mDataMgr.loadData<TbDataBullet>(_eTableName.Bullet.ToString());
            mDataMgr.loadData<TbDataEffectHypericumBullet>(_eTableName.EffectHypericumBullet.ToString());
            mDataMgr.loadData<TbDataFishEvenFour>(_eTableName.FishEvenFour.ToString());
            mDataMgr.loadData<TbDataFishEvenFive>(_eTableName.FishEvenFive.ToString());
            mDataMgr.loadData<TbDataOutFishGroup>(_eTableName.OutFishGroup.ToString());
            mDataMgr.loadData<TbDataFishEachCompose>(_eTableName.FishEachCompose.ToString());
            mDataMgr.loadData<TbDataFishCompose>(_eTableName.FishCompose.ToString());
            mDataMgr.loadData<TbDataFish>(_eTableName.Fish.ToString());
            mDataMgr.loadData<TbDataEffectRadiationLighting>(_eTableName.EffectRadiationLighting.ToString());
            mDataMgr.loadData<TbDataEffectSpreadFish>(_eTableName.EffectSpreadFish.ToString());
            mDataMgr.loadData<TbDataFormation>(_eTableName.Formation.ToString());

            mDataMgr.loadData<TbDataRoute>(_eTableName.Route.ToString());

            mDataMgr.loadData<TbDataShoal>(_eTableName.Shoal.ToString());
            mDataMgr.loadData<TbDataBackgroundSprite>(_eTableName.BackgroundSprite.ToString());

            mDataMgr.loadData<TbDataFishnet>(_eTableName.Fishnet.ToString());

            mDataMgr.loadData<TbDataSingleDuringLevelProp>(_eTableName.SingleDuringLevelProp.ToString());
            mDataMgr.loadData<TbDataSinglePreLevelProp>(_eTableName.SinglePreLevelProp.ToString());
            mDataMgr.loadData<TbDataGeneralLevel>(_eTableName.GeneralLevel.ToString());
            mDataMgr.loadData<TbDataLevel>(_eTableName.Level.ToString());
            mDataMgr.loadData<TbDataMap>(_eTableName.Map.ToString());

            mDataMgr.loadData<TbDataPlayerExp>(_eTableName.PlayerExp.ToString());
            mDataMgr.loadData<TbDataPlayerHead>(_eTableName.PlayerHead.ToString());
            mDataMgr.loadData<TbDataPlayerProp>(_eTableName.PlayerProp.ToString());
            mDataMgr.loadData<TbDataPlayerTitle>(_eTableName.PlayerTitle.ToString());
            mDataMgr.loadData<TbDataRoom>(_eTableName.Room.ToString());
            mDataMgr.loadData<TbDataSinglePlayerLife>(_eTableName.SinglePlayerLife.ToString());

            mDataMgr.loadData<TbDataSpecialLevel>(_eTableName.SpecialLevel.ToString());
            mDataMgr.loadData<TbDataVipLevel>(_eTableName.VipLevel.ToString());
            mDataMgr.loadData<TbDataTurret>(_eTableName.Turret.ToString());
        }
    }
}

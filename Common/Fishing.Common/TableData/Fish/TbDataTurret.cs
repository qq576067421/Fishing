using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataTurret : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum TurretType
        {
            Default = -1,
            None = 0,
            NormalTurret = 1,
            DragonTurret = 2
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public int TurretBasePixelHeight { get; private set; }
        public int TurretBaseHeight { get; private set; }
        public string TurretBaseAnimationName { get; private set; }
        public int TurretBarrelPixelHeight { get; private set; }
        public int TurretBarrelHeight { get; private set; }
        public string TurretBarrelAnimationName { get; private set; }
        public int TurretTopCoverPixelHeight { get; private set; }
        public int TurretTopCoverHeight { get; private set; }
        public string TurretTopCoverAnimationName { get; private set; }
        public int TurretFireBlazeixelHeight { get; private set; }
        public int TurretFireBlazeHeight { get; private set; }
        public string TurretFireBlazeAnimationName { get; private set; }
        public TbDataEffectCompose EffectCompose { get; private set; }
        public int TurretMarginal { get; private set; }
        public TbDataEffectCompose EffectComposeFire { get; private set; }
        public TbDataBullet BulletDataKey { get; private set; }
        public TbDataFishnet FishnetDataKey { get; private set; }
        public int TurretRate { get; private set; }
        public TbDataEffectCompose EffectComposeFireNoMoney { get; private set; }
        public TbDataParticle AimParticle { get; private set; }
        public TurretType mTurretType { get; private set; }
        public TbDataParticle HitFishParticleDataKey { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            TurretBasePixelHeight = prop_set.getPropInt("I_TurretBasePixelHeight").get();
            TurretBaseHeight = prop_set.getPropInt("I_TurretBaseHeight").get();
            TurretBaseAnimationName = prop_set.getPropString("T_TurretBaseAnimationName").get();
            TurretBarrelPixelHeight = prop_set.getPropInt("I_TurretBarrelPixelHeight").get();
            TurretBarrelHeight = prop_set.getPropInt("I_TurretBarrelHeight").get();
            TurretBarrelAnimationName = prop_set.getPropString("T_TurretBarrelAnimationName").get();
            TurretTopCoverPixelHeight = prop_set.getPropInt("I_TurretTopCoverPixelHeight").get();
            TurretTopCoverHeight = prop_set.getPropInt("I_TurretTopCoverHeight").get();
            TurretTopCoverAnimationName = prop_set.getPropString("T_TurretTopCoverAnimationName").get();
            TurretFireBlazeixelHeight = prop_set.getPropInt("I_TurretFireBlazeixelHeight").get();
            TurretFireBlazeHeight = prop_set.getPropInt("I_TurretFireBlazeHeight").get();
            TurretFireBlazeAnimationName = prop_set.getPropString("T_TurretFireBlazeAnimationName").get();

            EffectCompose = EbDataMgr.Instance.getData<TbDataEffectCompose>(prop_set.getPropInt("I_EffectCompose").get());
            TurretMarginal = prop_set.getPropInt("I_TurretMarginal").get();
            EffectComposeFire = EbDataMgr.Instance.getData<TbDataEffectCompose>(prop_set.getPropInt("I_EffectComposeFire").get());
            BulletDataKey = EbDataMgr.Instance.getData<TbDataBullet>(prop_set.getPropInt("I_BulletDataKey").get());
            FishnetDataKey = EbDataMgr.Instance.getData<TbDataFishnet>(prop_set.getPropInt("I_FishnetDataKey").get());
            TurretRate = prop_set.getPropInt("I_TurretRate").get();
            EffectComposeFireNoMoney = EbDataMgr.Instance.getData<TbDataEffectCompose>(prop_set.getPropInt("I_EffectComposeFireNoMoney").get());
            AimParticle = EbDataMgr.Instance.getData<TbDataParticle>(prop_set.getPropInt("I_AimParticle").get());
            var prop_turrettype = prop_set.getPropInt("I_TurretType");
            mTurretType = prop_turrettype == null ? TurretType.Default : (TurretType)prop_turrettype.get();
            HitFishParticleDataKey = EbDataMgr.Instance.getData<TbDataParticle>(prop_set.getPropInt("I_HitFishParticleDataKey").get());
        }
    }
}

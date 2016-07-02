using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataFish : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        public enum IsFullScreen
        {
            Default = -1,
            NO = 0,
            YES = 1
        }

        public enum FishRouteCategory
        {
            Default = -1,
            Small = 0,
            Medium = 1,
            Big = 2
        }

        public enum IsRed
        {
            Default = -1,
            NO = 0,
            YES = 1
        }

        public enum CanHitByFishNet
        {
            Default = -1,
            NO = 0,
            YES = 1
        }

        public enum FishType
        {
            Default = -1,
            ONE = 1,
            TWO = 2,
            THREE = 3,
            FOUR = 4,
            Custom = 5,
            EvenFour = 6,
            EvenFive = 7,
            ChainFish = 8
        }

        public enum _eDisplayScoreType
        {
            Default = -1,
            None = 0,
            Chips = 1,
            Turnplate = 2,
            ChipsAndTurnplate = 3
        }

        public enum ParticleProduceTimeEnum
        {
            Default = -1,
            FishMoving = 0,     //鱼游动时候的动画,loop
            FishBorn = 1,       //鱼出生时候的粒子效果
            FishHit = 2,        //鱼被击中时候的粒子效果
            FishDie = 3,        //鱼死亡时候的粒子效果
            FishDestroy = 4     //鱼销毁后的粒子效果
        }

        public enum FishDieTypeEnum
        {
            Default = -1,
            Normal = 0,
            BigFish = 1
        }

        //-------------------------------------------------------------------------
        public struct FishIdAndScaleStruct
        {
            public int FishId;
            public int Scale;
        }

        public struct ParticlePointStruct
        {
            public enum ParticlePointTypeEnum
            {
                Default = -1,
                None = 0,
                Fish = 1,
                Turret = 2,
                Fixed = 3
            }
            public ParticlePointTypeEnum ParticlePointType;
            public int x;
            public int y;
        }

        public struct ParticleDataStruct
        {
            public TbDataParticle TbDataParticle;
            public ParticleProduceTimeEnum ParticleProduceTime;
            public ParticlePointStruct StartPoint;
            public ParticlePointStruct TargetPoint;
        }

        public struct CoinParticleStruct
        {
            public TbDataParticle CointParticleData;
            public int CointCount;
            public int Radius;//金币散落的最大半径
        }

        //-------------------------------------------------------------------------
        public float getSpeed()
        {
            return (float)FishSpeed / 100;
        }

        public float getLockCardFishScale()
        {
            return (float)LockCardFishScale / 100;
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public string FishAnimMove { get; private set; }
        public string FishAnimDie { get; private set; }
        public int FishLevel { get; private set; }
        public int FishDieChance { get; private set; }
        public int FishSpeed { get; private set; }
        public int FishHitPoint { get; private set; }
        public int FishScore { get; private set; }
        public int FishPixelHeight { get; private set; }
        public int FishHeight { get; private set; }
        public int FishNetCount { get; private set; }//鱼被打中的时候需要网的个数，0和1是一样代表只有一张网
                                                     //public string FishTrackPoint { get; private set; }//表达方式和路径点一样，如果没有那么默认的追踪点就是HitPoint点
        public int FishDepth { get; private set; }
        public List<TbDataEffectCompose> Effects { get; private set; }
        public IsRed Red { get; private set; }
        public FishType Type { get; private set; }
        public _eDisplayScoreType FishDisplayScoreType { get; private set; }
        public CanHitByFishNet mCanHitByFishNet { get; private set; }
        public int FishMinScore { get; private set; }
        public int FishMaxScore { get; private set; }
        public int OutFishWeight { get; private set; }
        public int OutFishUpperBound { get; private set; }//0表示没有上限
        public string CycleAnimationName { get; private set; }
        public int CyclePixelHeight { get; private set; }
        public int CycleHeight { get; private set; }
        public IsFullScreen IsFullScreenAnimation { get; private set; }
        public TbDataFishCompose FishCompose { get; private set; }
        public int LockFishWeight { get; private set; }
        public int LockCardFishScale { get; private set; }//100倍精确两位小数
        public List<ParticleDataStruct> ParticleArray { get; private set; }
        public TbDataFishEvenFour FishEvenFour { get; private set; }
        public TbDataFishEvenFive FishEvenFive { get; private set; }
        public FishRouteCategory fishRouteCategory { get; private set; }
        public TbDataParticle TurnplateParticle { get; private set; }
        public TbDataOutFishGroup dataOutFishGroup { get; private set; }
        public string FishPropKey { get; private set; }// 属性key
        public string FishIcon { get; private set; }// 鱼显示图片
        public FishDieTypeEnum FishDieType { get; private set; }
        public int ChainFishNumber { get; private set; }
        public TbDataFish ChainFish { get; private set; }
        public CoinParticleStruct mCoinParticle { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            FishAnimMove = prop_set.getPropString("T_FishAnimMove").get();
            FishAnimDie = prop_set.getPropString("T_FishAnimDie").get();
            FishLevel = prop_set.getPropInt("I_FishLevel").get();
            FishDieChance = prop_set.getPropInt("I_FishDieChance").get();
            FishSpeed = prop_set.getPropInt("I_FishSpeed").get();
            FishHitPoint = prop_set.getPropInt("I_FishHitPoint").get();
            FishScore = prop_set.getPropInt("I_FishScore").get();
            FishPixelHeight = prop_set.getPropInt("I_FishPixelHeight").get();
            FishHeight = prop_set.getPropInt("I_FishHeight").get();
            FishNetCount = prop_set.getPropInt("I_FishNetCount").get();
            //FishTrackPoint = prop_set.getPropString("T_FishTrackPoint").get();
            FishDepth = prop_set.getPropInt("I_FishNetCount").get();

            Effects = new List<TbDataEffectCompose>();
            string strEffects = prop_set.getPropString("T_EffectsFishEffectCompose").get();
            string[] arrayEffects = strEffects.Split(';');
            foreach (string str in arrayEffects)
            {
                TbDataEffectCompose dataEffectCompose = EbDataMgr.Instance.getData<TbDataEffectCompose>(int.Parse(str));
                if (null != dataEffectCompose)
                {
                    Effects.Add(dataEffectCompose);
                }
            }

            var prop_red = prop_set.getPropInt("I_Red");
            Red = prop_red == null ? IsRed.Default : (IsRed)prop_red.get();
            var prop_type = prop_set.getPropInt("I_Type");
            Type = prop_type == null ? FishType.Default : (FishType)prop_type.get();
            var prop_displayscoretype = prop_set.getPropInt("I_FishDisplayScoreType");
            FishDisplayScoreType = prop_displayscoretype == null ? _eDisplayScoreType.Default : (_eDisplayScoreType)prop_displayscoretype.get();
            var prop_canhitbyfishnet = prop_set.getPropInt("I_mCanHitByFishNet");
            mCanHitByFishNet = prop_canhitbyfishnet == null ? CanHitByFishNet.Default : (CanHitByFishNet)prop_canhitbyfishnet.get();
            FishMinScore = prop_set.getPropInt("I_FishMinScore").get();
            FishMaxScore = prop_set.getPropInt("I_FishMaxScore").get();
            OutFishWeight = prop_set.getPropInt("I_OutFishWeight").get();
            OutFishUpperBound = prop_set.getPropInt("I_OutFishUpperBound").get();
            CycleAnimationName = prop_set.getPropString("T_CycleAnimationName").get();
            CyclePixelHeight = prop_set.getPropInt("I_CyclePixelHeight").get();
            CycleHeight = prop_set.getPropInt("I_CycleHeight").get();
            var prop_isfullscreen = prop_set.getPropInt("I_IsFullScreenAnimation");
            IsFullScreenAnimation = prop_isfullscreen == null ? IsFullScreen.Default : (IsFullScreen)prop_isfullscreen.get();
            FishCompose = EbDataMgr.Instance.getData<TbDataFishCompose>(prop_set.getPropInt("I_FishCompose").get());
            LockFishWeight = prop_set.getPropInt("I_LockFishWeight").get();
            LockCardFishScale = prop_set.getPropInt("I_LockCardFishScale").get();

            ParticleArray = new List<ParticleDataStruct>();
            for (int i = 1; i <= 10; ++i)
            {
                string strParticles = prop_set.getPropString("T_ParticleArray" + i.ToString()).get();
                string[] arrayStrParticles = strParticles.Split(';');
                ParticleDataStruct particleDataStruct = new ParticleDataStruct();
                particleDataStruct.TbDataParticle = EbDataMgr.Instance.getData<TbDataParticle>(int.Parse(arrayStrParticles[0]));
                particleDataStruct.ParticleProduceTime = string.IsNullOrEmpty(arrayStrParticles[1]) ? ParticleProduceTimeEnum.Default : (ParticleProduceTimeEnum)int.Parse(arrayStrParticles[1]);

                particleDataStruct.StartPoint = new ParticlePointStruct();
                particleDataStruct.StartPoint.ParticlePointType = string.IsNullOrEmpty(arrayStrParticles[2]) ? ParticlePointStruct.ParticlePointTypeEnum.Default : (ParticlePointStruct.ParticlePointTypeEnum)int.Parse(arrayStrParticles[2]);
                particleDataStruct.StartPoint.x = int.Parse(arrayStrParticles[3]);
                particleDataStruct.StartPoint.y = int.Parse(arrayStrParticles[4]);

                particleDataStruct.TargetPoint = new ParticlePointStruct();
                particleDataStruct.TargetPoint.ParticlePointType = string.IsNullOrEmpty(arrayStrParticles[5]) ? ParticlePointStruct.ParticlePointTypeEnum.Default : (ParticlePointStruct.ParticlePointTypeEnum)int.Parse(arrayStrParticles[5]);
                particleDataStruct.TargetPoint.x = int.Parse(arrayStrParticles[6]);
                particleDataStruct.TargetPoint.y = int.Parse(arrayStrParticles[7]);

                ParticleArray.Add(particleDataStruct);
            }

            FishEvenFour = EbDataMgr.Instance.getData<TbDataFishEvenFour>(prop_set.getPropInt("I_FishEvenFour").get());
            FishEvenFive = EbDataMgr.Instance.getData<TbDataFishEvenFive>(prop_set.getPropInt("I_FishEvenFive").get());
            var prop_fishroutecategory = prop_set.getPropInt("I_FishRouteCategory");
            fishRouteCategory = prop_fishroutecategory == null ? FishRouteCategory.Default : (FishRouteCategory)prop_fishroutecategory.get();
            TurnplateParticle = EbDataMgr.Instance.getData<TbDataParticle>(prop_set.getPropInt("I_TurnplateParticle").get());
            dataOutFishGroup = EbDataMgr.Instance.getData<TbDataOutFishGroup>(prop_set.getPropInt("I_OutFishGroupData").get());
            FishPropKey = prop_set.getPropString("T_FishPropKey").get();
            FishIcon = prop_set.getPropString("T_FishIcon").get();
            var prop_fishdietype = prop_set.getPropInt("I_FishDieType");
            FishDieType = prop_fishdietype == null ? FishDieTypeEnum.Default : (FishDieTypeEnum)prop_fishdietype.get();
            ChainFishNumber = prop_set.getPropInt("I_ChainFishNumber").get();
            ChainFish = EbDataMgr.Instance.getData<TbDataFish>(prop_set.getPropInt("I_ChainFish").get());


            if (null != prop_set.getPropString("T_CoinParticle"))
            {
                CoinParticleStruct coinParticleStruct = new CoinParticleStruct();
                string strCointParticleDatas = prop_set.getPropString("T_CoinParticle").get();
                string[] arrayStrCointParticleDatas = strCointParticleDatas.Split(';');
                if (arrayStrCointParticleDatas.Length == 3)
                {
                    coinParticleStruct.CointParticleData = EbDataMgr.Instance.getData<TbDataParticle>(int.Parse(arrayStrCointParticleDatas[0]));
                    coinParticleStruct.CointCount = int.Parse(arrayStrCointParticleDatas[1]);
                    coinParticleStruct.Radius = int.Parse(arrayStrCointParticleDatas[2]);
                    mCoinParticle = coinParticleStruct;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class EffectSpreadFish : CEffect
    {
        //---------------------------------------------------------------------
        CLogicScene mScene = null;
        int mWaveCount = 0;
        float mEffectGapTime = 0;
        float mTimeCounter = 0;
        TbDataEffectSpreadFish mEffectSpreadFishData = null;
        EbVector3 mSourcePosition;
        int mRedObjId = -1;

        //---------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            mMapParam = param;
        }

        //---------------------------------------------------------------------
        public override void start()
        {
            base.start();

            mEffectSpreadFishData = EbDataMgr.Instance.getData<TbDataEffectSpreadFish>(mEffectId);

            mScene = (CLogicScene)mMapParam["LogicScene"];
            mWaveCount = mEffectSpreadFishData.SpreadCount;
            mSourcePosition = (EbVector3)mMapParam["SourcePosition"];
            mEffectGapTime = mEffectSpreadFishData.GapTime * 0.01f; ;
            mTimeCounter = mEffectGapTime;

            mRedObjId = (int)mMapParam["RedFishObjId"];
        }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            mTimeCounter += elapsed_tm * mScene.getLevel().getTimeFactor();
            if (mTimeCounter > mEffectGapTime)
            {
                if (mWaveCount > 0)
                {
                    _outFishCrowd(mWaveCount == 1);
                    --mWaveCount;
                    mTimeCounter = 0;
                }
                else
                {
                    signDestroy();
                }
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
        }

        //---------------------------------------------------------------------
        void _outFishCrowd(bool has_red_fish)
        {
            if (has_red_fish)
            {
                mScene.getLevel().outRedFish(mEffectSpreadFishData.NormalFish.Id,
                    mEffectSpreadFishData.RedFish.Id, mEffectSpreadFishData.FishCount, mSourcePosition, mRedObjId);
                return;
            }

            mScene.getLevel().outRedFish(mEffectSpreadFishData.NormalFish.Id,
                -1, mEffectSpreadFishData.FishCount, mSourcePosition, mRedObjId);
        }
    }

    public class EffectSpreadFishFactory : IEffectFactory
    {
        //---------------------------------------------------------------------
        public string getEffectName()
        {
            return "SpreadFish";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server;
        }

        //---------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            EffectSpreadFish effect = new EffectSpreadFish();
            return effect;
        }
    }
}

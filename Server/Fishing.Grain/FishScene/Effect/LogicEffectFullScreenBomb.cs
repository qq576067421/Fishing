using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class LogicEffectFullScreenBomb : CEffect
    {
        //---------------------------------------------------------------------
        private CLogicScene mScene = null;
        private EbVector3 mPos = EbVector3.Zero;
        float mDelayTime = 0;

        //---------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            mMapParam = param;
            mScene = mMapParam["LogicScene"] as CLogicScene;
            mPos = (EbVector3)mMapParam["SourcePosition"];

            int score = 0;

            List<CLogicFish> all_fish = mScene.getLevel().getAllFish();
            if (all_fish != null && all_fish.Count > 0)
            {
                foreach (var f in all_fish)
                {
                    score += EbDataMgr.Instance.getData<TbDataFish>(f.FishVibId).FishScore;
                    f.signDestroy();
                }
            }

            mReturnValue = new List<object>();
            ReturnValue.Add("FullScreenBomb");
            ReturnValue.Add(score);
            mDelayTime = 8;
            mScene.getLevel().setPauseCreateFishCrowd(true);
        }

        //---------------------------------------------------------------------
        public override void start()
        {
            base.start();
        }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            mDelayTime -= elapsed_tm;
            if (mDelayTime <= 0)
            {
                mDelayTime = 10000000;
                //mScene.getLevel().setPauseCreateFishCrowd(false);
                signDestroy();
            }
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
        }
    }

    public class LogicEffectFullScreenBombFactory : IEffectFactory
    {
        //---------------------------------------------------------------------
        public string getEffectName()
        {
            return "FullScreenBomb";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server;
        }

        //---------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new LogicEffectFullScreenBomb();
            return effect;
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EffectFullScreen : ClientEffectComponent
    {
        //-------------------------------------------------------------------------
        private EbVector3 mPosition = EbVector3.Zero;
        private EbVector3 mDestPosition = EbVector3.Zero;
        private int mFishCount = 0;
        private int mCalculate = 0;

        //-------------------------------------------------------------------------
        public EffectFullScreen() { }

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            base.create(param);
            mPosition = (EbVector3)mMapParam["SourcePosition"];
            mDestPosition = (EbVector3)mMapParam["DestPosition"];
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();
            mScene.getLevel().destroyAllFishCrowd();
            List<CRenderFish> all_fish = mScene.getLevel().getAllFish();
            if (all_fish.Count >= 1)
            {
                mFishCount = all_fish.Count - 1;
            }
            if (all_fish != null && all_fish.Count > 0)
            {
                foreach (var f in all_fish)
                {
                    f.dieByFullBomb(mPosition);
                }
            }
            signDestroy();
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            if (mHasDestroy) return;
            mHasDestroy = true;
        }
    }

    public class EffectFullScreenFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "FullScreenBomb";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Client;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectFullScreen();
            return effect;
        }
    }
}

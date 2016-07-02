using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EffectTimeStop : ClientEffectComponent
    {
        float mFunDelayTime = 0;
        float mTimeCounter = 0;
        bool isBegin = false;

        //-------------------------------------------------------------------------
        public EffectTimeStop()
        {
        }

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            base.create(param);
            mScene = (CRenderScene)mMapParam["RenderScene"];
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();
            TbDataEffectLockScreen effect_data = EbDataMgr.Instance.getData<TbDataEffectLockScreen>(mEffectId);
            mFunDelayTime = effect_data.LastTime;
            isBegin = true;
            mScene.getLevel().setTimeFactor(0.0f);
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (!isBegin) return;

            mTimeCounter += elapsed_tm;
            if (mTimeCounter >= mFunDelayTime)
            {
                end();
            }
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            if (mHasDestroy) return;
            mHasDestroy = true;
        }

        //---------------------------------------------------------------------
        void end()
        {
            mScene.getLevel().setTimeFactor(1.0f);
            isBegin = false;
            signDestroy();
        }
    }

    public class EffectLockScreenFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "LockScreen";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server2Client;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectTimeStop();
            return effect;
        }
    }
}

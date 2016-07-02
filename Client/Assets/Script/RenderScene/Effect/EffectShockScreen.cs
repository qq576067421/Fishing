using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EffectShockScreen : ClientEffectComponent
    {
        //-------------------------------------------------------------------------
        private float shakeTime = 0.0f;

        //-------------------------------------------------------------------------
        public EffectShockScreen()
        {
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();

            TbDataEffectScreenShock effec_data = EbDataMgr.Instance.getData<TbDataEffectScreenShock>(mEffectId);
            shakeTime = effec_data.LastTime / 100.0f;

            mScene.getLevel().shockScreen();
        }

        bool mIsUpShake = true;
        float mShakeAngle = 30;

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                mScene.getLevel().stopShockScreen();
                signDestroy();
                return;
            }
        }

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            base.create(param);
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            if (mHasDestroy) return;
            mHasDestroy = true;
        }
    }

    public class EffectShockScreenFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "ShockScreen";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Client;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectShockScreen();
            return effect;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class EffectTimeStop : CEffect
    {
        //-------------------------------------------------------------------------
        CLogicScene mScene = null;
        float mFunDelayTime = 0;
        float mTimeCounter = 0;

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            mMapParam = param;
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();

            mScene = (CLogicScene)mMapParam["LogicScene"];
            TbDataEffectLockScreen effect_data = EbDataMgr.Instance.getData<TbDataEffectLockScreen>(mEffectId);

            mScene.getLevel().setTimeFactor(0.0f);
            mFunDelayTime = effect_data.LastTime;

            uint et_player_rpcid = (uint)mMapParam["PlayerID"];
            int bullet_rate = (int)mMapParam["BulletRate"];
            EbVector3 source_position = (EbVector3)mMapParam["SourcePosition"];
            int current_die_fish_id = (int)mMapParam["DieFishObjId"];//触发这个效果的鱼 

            mScene.getProtocol().s2allcCreateClientEffect(
                et_player_rpcid, bullet_rate, source_position, current_die_fish_id,
                mEffectId, mEffectName, (int)mEffectType, mDelayTime,
                new List<string>());
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            mTimeCounter += elapsed_tm;
            if (mTimeCounter >= mFunDelayTime)
            {
                end();
            }
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
        }

        //-------------------------------------------------------------------------
        void end()
        {
            mScene.getLevel().setTimeFactor(1.0f);
            signDestroy();
        }
    }

    public class EffectTimeStopFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "LockScreen";
        }

        //-------------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectTimeStop();
            return effect;
        }
    }
}

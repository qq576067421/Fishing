using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class LogicEffectLightingChain : CEffect
    {
        //-------------------------------------------------------------------------
        private CLogicScene mScene = null;
        private EbVector3 mPos = EbVector3.Zero;

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            mMapParam = param;
            mScene = mMapParam["LogicScene"] as CLogicScene;
            mPos = (EbVector3)mMapParam["SourcePosition"];

            uint et_player_rpcid = (uint)mMapParam["PlayerID"];
            int bullet_rate = (int)mMapParam["BulletRate"];
            int die_fish_id = (int)mMapParam["DieFishObjId"];

            int score = 0;

            TbDataEffectRadiationLighting effect_data = EbDataMgr.Instance.getData<TbDataEffectRadiationLighting>(mEffectId);
            List<CLogicFish> fish_list = mScene.getLevel().getListFishById(effect_data.NormalFish.Id);
            int each_fish_score = EbDataMgr.Instance.getData<TbDataFish>(effect_data.NormalFish.Id).FishScore;
            foreach (var n in fish_list)
            {
                score += each_fish_score;
                n.signDestroy();
            }

            // 服务端广播创建特效
            List<string> custom_param_list = new List<string>();
            custom_param_list.Add(effect_data.NormalFish.Id.ToString());

            mScene.getProtocol().s2allcCreateClientEffect(
                et_player_rpcid, bullet_rate, mPos, die_fish_id,
                mEffectId, mEffectName, (int)mEffectType, mDelayTime,
                custom_param_list);

            mReturnValue = new List<object>();
            ReturnValue.Add("Lighting");
            ReturnValue.Add(score);
            ReturnValue.Add(effect_data.NormalFish.Id);
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();
            signDestroy();
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
        }
    }

    public class LogicEffectLightingChainFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "Lighting";
        }

        //-------------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new LogicEffectLightingChain();
            return effect;
        }
    }
}

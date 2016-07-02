using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class EffectAOE : CEffect
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
            TbDataEffectAOE effect_data = EbDataMgr.Instance.getData<TbDataEffectAOE>(mEffectId);

            int total_score = 0;
            int fish_score = 0;
            int effect_fish_vib_id = -1;
            bool fish_die = false;

            List<CLogicFish> fish_list = getFishByPositionAndRadius(mPos, (float)effect_data.Radius);
            List<string> custom_param_list = new List<string>();

            // 遍历aoe范围内的活鱼
            foreach (var fish in fish_list)
            {
                // 检查是否在鱼的销毁列表，如果在则跳过，防止重复计算。
                if (fish.IsDie) continue;
                // 击中鱼测试，击中则把分数包括特效分数计下来，一起计算到aoe特效分数里面去。
                fish_score = 0;
                effect_fish_vib_id = -1;
                fish_die = fish.hit(et_player_rpcid, bullet_rate, ref fish_score, ref effect_fish_vib_id);

                if (fish_die)
                {
                    custom_param_list.Add(fish.FishObjId.ToString());
                    total_score += fish_score;
                }
            }

            // 服务端广播创建特效
            int current_die_fish_id = (int)mMapParam["DieFishObjId"];//触发这个效果的鱼 

            mScene.getProtocol().s2allcCreateClientEffect(
                et_player_rpcid, bullet_rate, mPos, current_die_fish_id,
                mEffectId, mEffectName, (int)mEffectType, mDelayTime,
                custom_param_list);

            mReturnValue = new List<object>();
            ReturnValue.Add("EffectAOE");
            ReturnValue.Add(total_score);
        }

        //-------------------------------------------------------------------------
        List<CLogicFish> getFishByPositionAndRadius(EbVector3 position, float radius)
        {
            List<CLogicFish> fish_list = new List<CLogicFish>();
            List<CLogicFish> scene_fish_list = mScene.getLevel().getAllFish();
            foreach (var it in scene_fish_list)
            {
                if (position.getDistance(it.Position) > radius) continue;
                fish_list.Add(it);
            }

            return fish_list;
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

    public class LogicEffectAOEFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "EffectAOE";
        }

        //-------------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectAOE();
            return effect;
        }
    }
}

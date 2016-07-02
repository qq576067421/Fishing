using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EffectLighting : ClientEffectComponent
    {
        //-------------------------------------------------------------------------
        private List<StillSprite> mListLight = null;
        private EbVector3 mSourcePosition = EbVector3.Zero;
        float mFunDelayTime = 0;
        float mTimeCounter = 0;
        List<CRenderFish> fish_list;

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            mTimeCounter += elapsed_tm;

            if (mTimeCounter >= mFunDelayTime)
            {
                signDestroy();
            }
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();
            mListLight = new List<StillSprite>();
            create(mSourcePosition);
            mFunDelayTime = 1.5f;

        }

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            base.create(param);
            mSourcePosition = (EbVector3)mMapParam["SourcePosition"];
            TbDataEffectRadiationLighting effect_data = EbDataMgr.Instance.getData<TbDataEffectRadiationLighting>(mEffectId);
            fish_list = mScene.getLevel().getListFishById(effect_data.NormalFish.Id);
            int current_rate = (int)mMapParam["Rate"];
            uint et_player_rpcid = (uint)mMapParam["PlayerId"];

            // 销毁一起被连死的鱼
            List<string> custom_param_list = (List<string>)mMapParam["EffectCustomParam"];
            int effect_fish_vibid = int.Parse(custom_param_list[0]);
            if (effect_fish_vibid != -1)
            {
                List<CRenderFish> fishs = mScene.getLevel().getListFishById(effect_fish_vibid);
                foreach (var f in fishs)
                {
                    f.dieWithRate(et_player_rpcid, current_rate);
                }
            }
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            if (mHasDestroy) return;
            mHasDestroy = true;

            end();
        }

        //-------------------------------------------------------------------------
        void end()
        {
            if (mListLight == null) return;//"YellowLighting"

            foreach (var m in mListLight)
            {
                mScene.getRenderObjectPool().freeStillSprite(m);
            }
            mListLight.Clear();
            mListLight = null;
        }

        //-------------------------------------------------------------------------
        void create(EbVector3 cur_pos)
        {
            string animation_name = "";

            float scale_x = 1f;
            float scale_y = 1f;
            float width_x = 10f;
            float width_y = 60f;

            if (mScene.getLevel().getRandoNumber(0, 100) < 50)
            {
                animation_name = "lightening-yellow";
                scale_y = width_y / 132f;
                width_x = 360f;
            }
            else
            {
                animation_name = "lightning-green";
                scale_y = width_y / 85f;
                width_x = 420f;
            }
            foreach (var n in fish_list)
            {
                float distance = n.Position.getDistance(cur_pos);
                if (distance > 1100)
                    continue;
                EbVector3 middle_pos = EbVector3.lerp(n.Position, cur_pos, 0.5f);
                StillSprite still_sprite = mScene.getRenderObjectPool().newStillSprite();
                still_sprite.setPosition(middle_pos);
                still_sprite.setLayer(mScene.getLayerAlloter().getLayer(_eLevelLayer.FishSwim));

                still_sprite.setDirection(CLogicUtility.getAngle(CLogicUtility.getVector2ByRotateAngle(cur_pos - n.Position, 90)));
                still_sprite.setScale(n.Position.getDistance(cur_pos) / width_x, scale_y);

                still_sprite.playAnimation(animation_name);

                mListLight.Add(still_sprite);

#if UNITY_EDITOR
                still_sprite.gameObject.name = "TkSpriteEffectLighting_" + animation_name;
#endif
            }
        }
    }

    public class EffectLightingFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "Lighting";
        }

        //-------------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Server2Client;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectLighting();
            return effect;
        }
    }
}

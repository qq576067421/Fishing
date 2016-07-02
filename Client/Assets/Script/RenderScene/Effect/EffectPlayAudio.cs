using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class EffectPlayAudio : ClientEffectComponent
    {
        //-------------------------------------------------------------------------
        private float mLastTime = 0.0f;
        float mFunDelayTime = 0;
        float mTimeCounter = 0;

        //-------------------------------------------------------------------------
        public EffectPlayAudio()
        {
        }

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            base.create(param);
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();
            TbDataEffectPlayAudio effect_data = EbDataMgr.Instance.getData<TbDataEffectPlayAudio>(mEffectId);
            if (effect_data.AudioName == string.Empty || effect_data == null)
            {
                mLastTime = 0.5f;
                return;
            }
            string[] strs = effect_data.AudioName.Split(',');
            int count = strs.Length;
            int index = UnityEngine.Random.Range(0, count);
            string audio_name = strs[index];
            if (mScene == null) { Debug.LogWarning("error"); }

            if (effect_data.AudioType == TbDataEffectPlayAudio.AudioTypeEnum.EFFECT)
            {
                mScene.getSoundMgr().play(audio_name, _eSoundLayer.LayerNormal);

            }
            else if (effect_data.AudioType == TbDataEffectPlayAudio.AudioTypeEnum.DIALOG)
            {
                mScene.getSoundMgr().play(audio_name, _eSoundLayer.LayerIgnore);
            }
            else if (effect_data.AudioType == TbDataEffectPlayAudio.AudioTypeEnum.BACKGROUND)
            {
                mScene.getSoundMgr().play(audio_name, _eSoundLayer.Background);
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

    public class EffectPlayAudioFactory : IEffectFactory
    {
        //-------------------------------------------------------------------------
        public string getEffectName()
        {
            return "PlayAudio";
        }

        //---------------------------------------------------------------------
        public EffectTypeEnum getEffectType()
        {
            return EffectTypeEnum.Client;
        }

        //-------------------------------------------------------------------------
        public CEffect createEffect(Dictionary<string, object> param)
        {
            CEffect effect = new EffectPlayAudio();
            return effect;
        }
    }
}

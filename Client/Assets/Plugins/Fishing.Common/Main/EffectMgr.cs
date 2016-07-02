using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public static class EffectSys
    {
        //-------------------------------------------------------------------------
        static Dictionary<string, Effect> mMapEffect = new Dictionary<string, Effect>();

        //-------------------------------------------------------------------------
        public static bool CheckParam { get; set; }

        //-------------------------------------------------------------------------
        public static void regEffect(Effect effect)
        {
            mMapEffect[effect.GetType().Name] = effect;
        }

        //-------------------------------------------------------------------------
        public static Effect getEffect(string effect_name)
        {
            Effect effect = null;
            mMapEffect.TryGetValue(effect_name, out effect);
            return effect;
        }

        //-------------------------------------------------------------------------
        public static bool belongEffect<T>(EffectData effect_data) where T : Effect
        {
            TbDataEffect data_effect = EbDataMgr.Instance.getData<TbDataEffect>(effect_data.EffectId);
            if (data_effect == null) return false;

            if (typeof(T).Name == data_effect.ScriptName) return true;

            return false;
        }
    }

    public class EffectMgr
    {
        //-------------------------------------------------------------------------
        Queue<EffectContext> QueEffectContext { get; set; }

        //-------------------------------------------------------------------------
        public EffectMgr()
        {
            QueEffectContext = new Queue<EffectContext>();
        }

        //-------------------------------------------------------------------------
        public object doEffect(EffectContext effect_context, string effect_name, string[] predefine_param, string[] effect_param)
        {
            Effect effect = EffectSys.getEffect(effect_name);
            if (effect == null)
            {
                //EbLog.Error("EffectSys.doEffect() Error, can't found effect_name=" + effect_name);
                return null;
            }

            object result = effect.excute(this, effect_context, predefine_param, effect_param);

            _freeEffectContext(effect_context);

            return result;
        }

        //-------------------------------------------------------------------------
        public EffectContext genEffectContext()
        {
            if (QueEffectContext.Count > 0) return QueEffectContext.Dequeue();
            else return new EffectContext();
        }

        //-------------------------------------------------------------------------
        void _freeEffectContext(EffectContext effect_context)
        {
            effect_context.clear();
            QueEffectContext.Enqueue(effect_context);
        }
    }
}

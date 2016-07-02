using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CEffectMgr
    {
        //---------------------------------------------------------------------
        List<CombineCEffect> mCEffects = new List<CombineCEffect>();
        List<CombineCEffect> mAddCEffects = new List<CombineCEffect>();
        EffectComponentFactory mFactory = new EffectComponentFactory();
        List<CEffect> mSingleEffect = new List<CEffect>();

        //---------------------------------------------------------------------
        public CEffectMgr()
        {
        }

        //---------------------------------------------------------------------
        public void regEffectFactory(IEffectFactory effect_factory)
        {
            mFactory.regEffectFactory(effect_factory);
        }

        //---------------------------------------------------------------------
        public List<List<object>> createCombineEffect(int vib_component_id, Dictionary<string, object> param, EffectTypeEnum effect_generate_type)
        {
            CombineCEffect effect = new CombineCEffect(mFactory);
            effect.create(vib_component_id, param, effect_generate_type);
            mAddCEffects.Add(effect);
            return effect.ReturnValue;
        }

        //---------------------------------------------------------------------
        public List<object> createEffect(int effect_id, string effect_name, int effect_type, float delay_time, Dictionary<string, object> param, EffectTypeEnum effect_generate_type)
        {
            CEffect effect = mFactory.buildByEffectElementData(effect_id, effect_name, effect_type, delay_time, param, effect_generate_type);
            if (effect == null) return null;
            mSingleEffect.Add(effect);
            return effect.ReturnValue;
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            foreach (var it in mAddCEffects)
            {
                mCEffects.Add(it);
            }
            mAddCEffects.Clear();

            // 更新组合效果
            List<CombineCEffect> destroy_list = new List<CombineCEffect>();
            foreach (var it in mCEffects)
            {
                it.update(elapsed_tm);
                if (it.canDestroy())
                {
                    destroy_list.Add(it);
                }
            }
            foreach (var it in destroy_list)
            {
                mCEffects.Remove(it);
                it.destroy();
            }

            // 更新单个效果
            List<CEffect> effect_destroy_list = new List<CEffect>();
            foreach (var it in mSingleEffect)
            {
                if (!it.HasStart) it.start();
                it.update(elapsed_tm);
                if (it.canDestroy())
                {
                    effect_destroy_list.Add(it);
                }
            }
            foreach (var it in effect_destroy_list)
            {
                mSingleEffect.Remove(it);
                it.destroy();
            }
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            foreach (var it in mCEffects)
            {
                it.destroy();
            }
            mCEffects.Clear();
        }
    }

    public class CombineCEffect
    {
        //---------------------------------------------------------------------
        Dictionary<int, CEffect> mComponents = new Dictionary<int, CEffect>();
        List<ComponentUpdate> mComponentFuncs = new List<ComponentUpdate>();
        EffectComponentFactory mFactory = null;
        public List<List<object>> ReturnValue { get { return mReturnValue; } }
        List<List<object>> mReturnValue = new List<List<object>>();

        //---------------------------------------------------------------------
        public CombineCEffect(EffectComponentFactory factory)
        {
            mFactory = factory;
        }

        //---------------------------------------------------------------------
        public void create(int vib_component_id, Dictionary<string, object> param, EffectTypeEnum effect_type)
        {
            if (vib_component_id < 0) return;

            List<TbDataEffectCompose.EffectElementStruct> data_list = EbDataMgr.Instance.getData<TbDataEffectCompose>(vib_component_id).EffectElements;
            if (data_list == null || data_list.Count <= 0) return;

            int index = 0;
            CEffect pre_component = null;

            foreach (var it in data_list)
            {
                if (0 != it.EffectId && null != it.EffectName)
                {
                    CEffect effect = mFactory.buildByEffectElementData(it, param, effect_type);

                    if (effect == null) continue;

                    mComponents.Add(index, effect);
                    ComponentUpdate component_update = new ComponentUpdate(effect, pre_component);
                    mComponentFuncs.Add(component_update);

                    pre_component = effect;
                    index++;
                }
            }

            excute();
        }

        //---------------------------------------------------------------------
        // 服务器端运行excute直接执行所有的效果组件
        void excute()
        {
            foreach (var it in mComponents)
            {
                if (it.Value.ReturnValue != null)
                    mReturnValue.Add(it.Value.ReturnValue);
            }
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            foreach (var it in mComponentFuncs)
            {
                if (it != null)
                {
                    it.update(elapsed_tm);
                }
            }
        }

        //---------------------------------------------------------------------
        public bool canDestroy()
        {
            foreach (var it in mComponents)
            {
                if (!it.Value.canDestroy())
                {
                    return false;
                }
            }
            return true;
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            foreach (var it in mComponents)
            {
                it.Value.destroy();
            }
            mComponents.Clear();
            mComponentFuncs.Clear();
            mReturnValue.Clear();
        }
    }

    public class ComponentUpdate
    {
        //---------------------------------------------------------------------
        float mDelayTime = 0;
        bool mHasDestroy = false;
        CEffect mComponet = null;
        CEffect mPreComponent = null;

        //---------------------------------------------------------------------
        public ComponentUpdate(CEffect componet, CEffect pre_component)
        {
            mComponet = componet;
            mPreComponent = pre_component;
        }

        //---------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            if (mHasDestroy || mComponet == null) return;

            if (!mHasDestroy && mComponet.canDestroy())
            {
                mComponet.destroy();
                mHasDestroy = true;
                return;
            }

            mDelayTime += elapsed_tm;

            if (mComponet.EffectType == TbDataEffectCompose.EffectElementStruct.EffectTypeEnum.EffectParallel)
            {
                updateComponet(mComponet, elapsed_tm);
            }
            else if (mComponet.EffectType == TbDataEffectCompose.EffectElementStruct.EffectTypeEnum.EffectOrder
                && (mPreComponent == null || mPreComponent.canDestroy()))
            {
                updateComponet(mComponet, elapsed_tm);
            }
            else if (mComponet.EffectType == TbDataEffectCompose.EffectElementStruct.EffectTypeEnum.EffectDelay && (mDelayTime > mComponet.DelayTime))
            {
                updateComponet(mComponet, elapsed_tm);
            }
            else if (mComponet.EffectType == TbDataEffectCompose.EffectElementStruct.EffectTypeEnum.EffectAttach && mPreComponent != null && mPreComponent.canDestroy())
            {
                mComponet.destroy();
                mHasDestroy = true;
                return;
            }
            else if (mComponet.EffectType == TbDataEffectCompose.EffectElementStruct.EffectTypeEnum.EffectAttach && mPreComponent != null && mPreComponent.HasStart)
            {
                updateComponet(mComponet, elapsed_tm);
            }
            else if (mComponet.EffectType == TbDataEffectCompose.EffectElementStruct.EffectTypeEnum.EffectAttach && mPreComponent == null)
            {
                updateComponet(mComponet, elapsed_tm);
            }
        }

        //---------------------------------------------------------------------
        void updateComponet(CEffect component, float elapsed_tm)
        {
            if (!component.HasStart)
            {
                component.start();
            }
            component.update(elapsed_tm);
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
        }
    }

    public abstract class CEffect
    {
        //---------------------------------------------------------------------
        public List<object> ReturnValue { get { return mReturnValue; } }
        protected List<object> mReturnValue = null;
        protected void setReturnValue(List<object> return_value)
        {
            mReturnValue = return_value;
        }

        public TbDataEffectCompose.EffectElementStruct.EffectTypeEnum EffectType { get { return mEffectType; } }
        public float DelayTime { get { return mDelayTime; } }

        public bool HasStart { get { return mHasStart; } }
        protected bool mHasStart = false;
        protected Dictionary<string, object> mMapParam = null;//效果的参数
        bool mSignDestroy = false;
        protected string mName = "";
        public string Name { get { return mName; } }
        public void setName(string name)
        {
            mName = name;
        }
        protected bool mHasDestroy = false;

        protected int mEffectId = 0;
        protected string mEffectName = "";
        protected float mDelayTime = 0;
        protected TbDataEffectCompose.EffectElementStruct.EffectTypeEnum mEffectType = TbDataEffectCompose.EffectElementStruct.EffectTypeEnum.EffectParallel;

        //---------------------------------------------------------------------
        public abstract void create(Dictionary<string, object> param);

        //---------------------------------------------------------------------
        public void setEffectElementData(int effect_id, string effect_name, TbDataEffectCompose.EffectElementStruct.EffectTypeEnum effect_type, float delay_time)
        {
            mEffectId = effect_id;
            mEffectName = effect_name;
            mEffectType = effect_type;
            mDelayTime = delay_time;
        }

        //---------------------------------------------------------------------
        public virtual void start()
        {
            // 如果是服务器端直接设置返回值
            mHasStart = true;
            return;
        }

        //---------------------------------------------------------------------
        public abstract void update(float elapsed_tm);

        //---------------------------------------------------------------------
        public bool canDestroy() { return mSignDestroy; }

        //---------------------------------------------------------------------
        public abstract void destroy();

        //---------------------------------------------------------------------
        protected virtual void signDestroy()
        {
            mSignDestroy = true;
        }
    }

    public class EffectComponentFactory
    {
        //---------------------------------------------------------------------
        Dictionary<string, IEffectFactory> mDicEffectFactory = new Dictionary<string, IEffectFactory>();

        //---------------------------------------------------------------------
        public void regEffectFactory(IEffectFactory effect_factory)
        {
            if (!mDicEffectFactory.ContainsKey(effect_factory.getEffectName()))
            {
                mDicEffectFactory[effect_factory.getEffectName()] = effect_factory;
            }
        }

        //---------------------------------------------------------------------
        public CEffect buildByEffectElementData(TbDataEffectCompose.EffectElementStruct data, Dictionary<string, object> param, EffectTypeEnum effect_type)
        {
            string effect_name = data.EffectName.EffectName;
            if (mDicEffectFactory.ContainsKey(effect_name))
            {
                if (mDicEffectFactory[effect_name].getEffectType() != effect_type) return null;
                CEffect effect_component = mDicEffectFactory[effect_name].createEffect(param);
                effect_component.setEffectElementData(data.EffectId, effect_name, (TbDataEffectCompose.EffectElementStruct.EffectTypeEnum)data.EffectType, data.EffectDelayTime * 0.01f);
                effect_component.create(param);
                effect_component.setName(effect_name);
                return effect_component;
            }
            return null;
        }

        //---------------------------------------------------------------------
        public CEffect buildByEffectElementData(int effect_id, string effect_name, int effect_type, float delay_time, Dictionary<string, object> param, EffectTypeEnum effect_generate_type)
        {
            if (mDicEffectFactory.ContainsKey(effect_name))
            {
                if (mDicEffectFactory[effect_name].getEffectType() != effect_generate_type) return null;
                CEffect effect_component = mDicEffectFactory[effect_name].createEffect(param);
                effect_component.setEffectElementData(effect_id, effect_name, (TbDataEffectCompose.EffectElementStruct.EffectTypeEnum)effect_type, delay_time);
                effect_component.create(param);
                effect_component.setName(effect_name);
                return effect_component;
            }
            return null;
        }
    }

    public interface IEffectFactory
    {
        //---------------------------------------------------------------------
        string getEffectName();

        //---------------------------------------------------------------------
        EffectTypeEnum getEffectType();

        //---------------------------------------------------------------------
        CEffect createEffect(Dictionary<string, object> param);
    }

    public enum EffectTypeEnum
    {
        Server,
        Client,
        Server2Client
    }
}

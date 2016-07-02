using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public enum EffectCaller
    {
        None = 0,
        UseItem,// 使用道具
        EquipActive,// 装备激活
        StatusStart,// 状态开始
        StatusStop,// 状态结束
        StatusUpdate,// 状态更新
        SkillObj,// 技能对象
        SceneProduce,// 场景产出
        ProduceDistributorMonsterDropItem,// 产出分配器，怪物掉落
    }

    public class EffectData
    {
        //-------------------------------------------------------------------------
        public Effect Effect { get; private set; }
        public int EffectId { get; set; }
        public string[] ListParam { get; set; }

        //-------------------------------------------------------------------------
        public EffectData()
        {
        }

        //-------------------------------------------------------------------------
        public EffectData(string effect_data)
        {
        }

        //-------------------------------------------------------------------------
        public string save()
        {
            return "";
        }
    }

    public class EffectContext
    {
        //-------------------------------------------------------------------------
        public EffectCaller caller;
        public Entity EtActor;
        public Entity EtTarget;
        public Item item;
        public Dictionary<string, IProp> map_prop;
        public IComponent co_scene;

        //-------------------------------------------------------------------------
        public EffectContext()
        {
            clear();
        }

        //-------------------------------------------------------------------------
        public void clear()
        {
        }
    }

    public abstract class Effect
    {
        //-------------------------------------------------------------------------
        public abstract object excute(EffectMgr effect_mgr, EffectContext effect_context, string[] predefine_param, string[] effect_param);
    }
}

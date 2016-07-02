using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectCompose : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        //-------------------------------------------------------------------------
        public struct EffectElementStruct
        {
            public enum EffectTypeEnum
            {
                Default = -1,
                EffectParallel = 0,
                EffectOrder = 1,
                EffectDelay = 2,
                EffectAttach = 3
            }
            public TbDataEffectName EffectName;
            public int EffectId;
            public EffectTypeEnum EffectType;//并发执行效果，先后执行，延时执行
            public int EffectDelayTime;//乘以100以保持2位精度
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public string Note { get; private set; }
        public DataState State { get; private set; }

        public List<EffectElementStruct> EffectElements { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            Note = prop_set.getPropString("T_Note").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();

            EffectElements = new List<EffectElementStruct>();
            for (int i = 1; i <= 6; ++i)
            {
                string strElements = prop_set.getPropString("T_EffectElement" + i.ToString()).get();
                string[] ArrayElements = strElements.Split(';');
                EffectElementStruct effectElementStruct = new EffectElementStruct();
                effectElementStruct.EffectName = EbDataMgr.Instance.getData<TbDataEffectName>(int.Parse(ArrayElements[0]));
                effectElementStruct.EffectId = int.Parse(ArrayElements[1]);
                effectElementStruct.EffectType = string.IsNullOrEmpty(ArrayElements[2]) ? EffectElementStruct.EffectTypeEnum.Default : (EffectElementStruct.EffectTypeEnum)int.Parse(ArrayElements[2]);
                effectElementStruct.EffectDelayTime = int.Parse(ArrayElements[3]);
                EffectElements.Add(effectElementStruct);
            }
        }
    }
}

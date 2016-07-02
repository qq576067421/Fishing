using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectPlayAudio : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum AudioLoopTypeEnum
        {
            Default = -1,
            ONCE = 0,
            LOOP = 1
        }
        public enum AudioTypeEnum
        {
            Default = -1,
            EFFECT = 0,
            DIALOG = 1,
            BACKGROUND = 2
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public string AudioName;
        public AudioLoopTypeEnum LoopType;
        public AudioTypeEnum AudioType;

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            AudioName = prop_set.getPropString("T_AudioName").get();
            LoopType = (AudioLoopTypeEnum)prop_set.getPropInt("I_LoopType").get();
            AudioType = (AudioTypeEnum)prop_set.getPropInt("I_AudioType").get();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataBackgroundSprite : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public string AnimationName { get; private set; }
        public int SpritePixelHeight { get; private set; }
        public int SpriteLogicHeight { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            AnimationName = prop_set.getPropString("T_AnimationName").get();
            SpritePixelHeight = prop_set.getPropInt("I_SpritePixelHeight").get();
            SpriteLogicHeight = prop_set.getPropInt("I_SpriteLogicHeight").get();
        }
    }
}

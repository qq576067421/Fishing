using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataSinglePreLevelProp : EbData
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
        public string PropName { get; private set; } // 名称
        public string PropDesn { get; private set; } // 描述
        public string PropDisableTexName { get; private set; }  // 禁用
        public string PropAvailableTexName { get; private set; }   // 使用

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            PropName = prop_set.getPropString("T_PropName").get();
            PropDesn = prop_set.getPropString("T_PropDesn").get();
            PropDisableTexName = prop_set.getPropString("T_PropDisableTexName").get();
            PropAvailableTexName = prop_set.getPropString("T_PropAvailableTexName").get();
        }
    }
}

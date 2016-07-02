using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataSpecialLevel : EbData
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
        public int PreLevelNo { get; private set; }        // 前一关No
        public int FirstLevelNo { get; private set; }      // 大关卡第一关
        public int SecondLevelNo { get; private set; }     // 大关卡第二关
        public int ThirdLevelNo { get; private set; }      // 大关卡第三关

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            PreLevelNo = prop_set.getPropInt("I_PreLevelNo").get();
            FirstLevelNo = prop_set.getPropInt("I_FirstLevelNo").get();
            SecondLevelNo = prop_set.getPropInt("I_SecondLevelNo").get();
            ThirdLevelNo = prop_set.getPropInt("I_ThirdLevelNo").get();
        }
    }
}

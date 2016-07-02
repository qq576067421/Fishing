using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectFlySword : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum SpreadTypeEnum
        {
            Default = -1,
            Cycle = 0,
            Random = 1
        }
        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public string SourceName;
        public int SwordCount;
        public int LastTime;
        public int SwordSpeed;
        public SpreadTypeEnum SpreadType;

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            SourceName = prop_set.getPropString("T_SourceName").get();
            SwordCount = prop_set.getPropInt("I_SwordCount").get();
            LastTime = prop_set.getPropInt("I_LastTime").get();
            SwordSpeed = prop_set.getPropInt("I_SwordSpeed").get();
            var prop_spread = prop_set.getPropInt("I_SpreadType");
            SpreadType = prop_spread == null ? SpreadTypeEnum.Default : (SpreadTypeEnum)prop_spread.get();
        }
    }
}

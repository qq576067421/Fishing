using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataPlayerHead : EbData
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
        public int PlayerHeadNum { get; private set; }//推荐头像编号
        public string PlayerHeadName { get; private set; }//头像名字

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            PlayerHeadNum = prop_set.getPropInt("I_PlayerHeadNum").get();
            PlayerHeadName = prop_set.getPropString("T_PlayerHeadName").get();
        }
    }
}

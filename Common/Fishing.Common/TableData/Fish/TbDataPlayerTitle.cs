using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataPlayerTitle : EbData
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
        public int PlayerTitleId { get; private set; }//称号ID
        public int PlayerGold { get; private set; }//最少金币值
        public string PlayerTitle { get; private set; }//玩家称号

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            PlayerTitleId = prop_set.getPropInt("I_PlayerTitleId").get();
            PlayerGold = prop_set.getPropInt("I_PlayerGold").get();
            PlayerTitle = prop_set.getPropString("T_PlayerTitle").get();
        }
    }
}

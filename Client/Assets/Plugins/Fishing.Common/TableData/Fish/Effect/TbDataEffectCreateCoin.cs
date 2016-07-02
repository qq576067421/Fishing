using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataEffectCreateCoin : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum CoinTypeEnum
        {
            Default = -1,
            GOLD = 0,
            BLUE = 1
        }
        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public int CoinCount;
        public CoinTypeEnum CoinType;
        public int CoinScore;

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            CoinCount = prop_set.getPropInt("I_CoinCount").get();
            var prop_coin = prop_set.getPropInt("I_CoinType");
            CoinType = prop_coin == null ? CoinTypeEnum.Default : (CoinTypeEnum)prop_coin.get();
            CoinScore = prop_set.getPropInt("I_CoinScore").get();
        }
    }
}

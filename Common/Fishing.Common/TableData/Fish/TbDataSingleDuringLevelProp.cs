using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataSingleDuringLevelProp : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum _eIsDisposable
        {
            Default = -1,
            DisposableNo = 0,
            DisposableYes = 1
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public string PropName { get; private set; } // 名称
        public string PropDesn { get; private set; } // 描述
        public string PropDisableTexName { get; private set; }  // 禁用
        public string PropAvailableTexName { get; private set; }   // 使用
        public string PropPurchaseTexName { get; private set; }   // 购买
        public _eIsDisposable IsDisposable { get; private set; }    // 是否一次性

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
            PropPurchaseTexName = prop_set.getPropString("T_PropPurchaseTexName").get();
            var prop_isdisposable = prop_set.getPropInt("I_IsDisposable");
            IsDisposable = prop_isdisposable == null ? _eIsDisposable.Default : (_eIsDisposable)prop_isdisposable.get();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataFishEvenFive : EbData
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
        public List<TbDataFish.FishIdAndScaleStruct> ListFishIdAndScale { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            ListFishIdAndScale = new List<TbDataFish.FishIdAndScaleStruct>();
            for (int i = 1; i <= 5; ++i)
            {
                string strFishIdAndScales = prop_set.getPropString("T_FishIdAndScale" + i.ToString()).get();
                string[] arrayStrFishIdAndScales = strFishIdAndScales.Split(';');
                TbDataFish.FishIdAndScaleStruct fishIdAndScaleStruct = new TbDataFish.FishIdAndScaleStruct();
                fishIdAndScaleStruct.FishId = int.Parse(arrayStrFishIdAndScales[0]);
                fishIdAndScaleStruct.Scale = int.Parse(arrayStrFishIdAndScales[1]);
                ListFishIdAndScale.Add(fishIdAndScaleStruct);
            }
        }
    }
}

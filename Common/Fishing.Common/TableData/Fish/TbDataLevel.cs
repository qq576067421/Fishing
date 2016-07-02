using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataLevel : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        public struct FishOutStruct
        {
            public int BeginTime;
            public int EndTime;
            public int ShoalAmountMin;
            public int ShoalAmountMax;
            public TbDataShoal Shoal;
            public TbDataRoute Route;
        }
        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }

        public int Duration { get; private set; }// 单位是秒

        public List<FishOutStruct> FishOut { get; private set; }
        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            Duration = prop_set.getPropInt("I_Duration").get();

            FishOut = new List<FishOutStruct>();
            for (int i = 0; i < 100; ++i)
            {
                string strFishOut = prop_set.getPropString("T_FishOut" + i.ToString()).get();
                string[] arrayFishOut = strFishOut.Split(';');
                FishOutStruct fishOutStruct = new FishOutStruct();
                fishOutStruct.BeginTime = int.Parse(arrayFishOut[0]);
                fishOutStruct.EndTime = int.Parse(arrayFishOut[1]);
                fishOutStruct.ShoalAmountMin = int.Parse(arrayFishOut[2]);
                fishOutStruct.ShoalAmountMax = int.Parse(arrayFishOut[3]);
                fishOutStruct.Shoal = EbDataMgr.Instance.getData<TbDataShoal>(int.Parse(arrayFishOut[4] + 1));
                fishOutStruct.Route = EbDataMgr.Instance.getData<TbDataRoute>(int.Parse(arrayFishOut[5]));
                FishOut.Add(fishOutStruct);
            }
        }
    }
}

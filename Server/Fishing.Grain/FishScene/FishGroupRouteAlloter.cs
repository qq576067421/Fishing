using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class FishGroupRouteAlloter
    {
        //-------------------------------------------------------------------------
        Dictionary<int, TbDataFish> mMapFishData = new Dictionary<int, TbDataFish>();
        int mTotalWeight = 1;
        int[] mArrayFishIndex = null;
        System.Random mRandom = new System.Random(unchecked((int)System.DateTime.Now.Ticks));

        //-------------------------------------------------------------------------
        public FishGroupRouteAlloter()
        {
            mMapFishData.Clear();
            var map_data = EbDataMgr.Instance.getMapData<TbDataFish>();
            foreach (var i in map_data)
            {
                mMapFishData[i.Key] = (TbDataFish)i.Value;
            }

            foreach (var it in mMapFishData)
            {
                mTotalWeight += it.Value.OutFishWeight;
            }

            mArrayFishIndex = new int[mTotalWeight - 1];

            int index = 0;
            foreach (var it in mMapFishData)
            {
                int count = 0;
                while (count < it.Value.OutFishWeight)
                {
                    mArrayFishIndex[index++] = it.Key;
                    count++;
                }
            }
        }

        //-------------------------------------------------------------------------
        public int getRandomRoute()
        {
            return mArrayFishIndex[mRandom.Next(0, mTotalWeight - 1)];
        }
    }
}

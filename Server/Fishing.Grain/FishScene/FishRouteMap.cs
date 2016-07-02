using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class FishRouteMap
    {
        //-------------------------------------------------------------------------
        Dictionary<TbDataFish.FishRouteCategory, List<int>> mDicFishRouteCategory = new Dictionary<TbDataFish.FishRouteCategory, List<int>>();

        //-------------------------------------------------------------------------
        public FishRouteMap()
        {
            // 初始化鱼和路径映射表。
            mDicFishRouteCategory.Add(TbDataFish.FishRouteCategory.Default, new List<int>());
            mDicFishRouteCategory.Add(TbDataFish.FishRouteCategory.Small, new List<int>());
            mDicFishRouteCategory.Add(TbDataFish.FishRouteCategory.Medium, new List<int>());
            mDicFishRouteCategory.Add(TbDataFish.FishRouteCategory.Big, new List<int>());
            var mapData = EbDataMgr.Instance.getMapData<TbDataRoute>();
            Dictionary<int, TbDataRoute> mapDataRoute = new Dictionary<int, TbDataRoute>();
            foreach (var i in mapData)
            {
                mapDataRoute[i.Key] = (TbDataRoute)i.Value;
            }

            foreach (var it in mapDataRoute)
            {
                if (it.Value.routeCategory == TbDataRoute.RouteCategory.Small)
                {
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Small].Add(it.Value.Id);
                }
                else if (it.Value.routeCategory == TbDataRoute.RouteCategory.Medium)
                {
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Medium].Add(it.Value.Id);
                }
                else if (it.Value.routeCategory == TbDataRoute.RouteCategory.Big)
                {
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Big].Add(it.Value.Id);
                }
                else if (it.Value.routeCategory == TbDataRoute.RouteCategory.SmallAndMedium)
                {
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Small].Add(it.Value.Id);
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Medium].Add(it.Value.Id);
                }
                else if (it.Value.routeCategory == TbDataRoute.RouteCategory.MediumAndBig)
                {
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Medium].Add(it.Value.Id);
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Big].Add(it.Value.Id);
                }
                else if (it.Value.routeCategory == TbDataRoute.RouteCategory.All)
                {
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Small].Add(it.Value.Id);
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Medium].Add(it.Value.Id);
                    mDicFishRouteCategory[TbDataFish.FishRouteCategory.Big].Add(it.Value.Id);
                }
            }
        }

        //-------------------------------------------------------------------------
        public List<int> getGroupFishId(TbDataFish.FishRouteCategory category)
        {
            return mDicFishRouteCategory[category];
        }
    }
}

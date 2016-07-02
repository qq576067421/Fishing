using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public interface IRoute
    {
        bool IsEndRoute { get; }
        EbVector3 Position { get; }
        EbVector3 Direction { get; }
        float OverDistance { get; }

        void next(float elapsed_tm, float speed);
        void reset(float total_distance);
    }

    public static class RouteHelper
    {
        //-----------------------------------------------------------------------------
        public static IRoute buildLineRoute(int route_vib_id, float delay_time = 0)
        {
            TbDataRoute route_data = EbDataMgr.Instance.getData<TbDataRoute>(route_vib_id);

            List<EbVector3> points_list = route_data.ListPoints;
            if (0 < points_list.Count)
            {
                RouteLine route = new RouteLine();
                route.create(delay_time, points_list);
                return route;
            }
            else
            {
                EbLog.Error("build line Route error::" + route_vib_id);
                return null;
            }
        }

        //-----------------------------------------------------------------------------
        public static IRoute buildLineRoute(EbVector3 start_position, EbVector3 direction, float distance)
        {
            return buildLineRoute(start_position, start_position + direction.normalized * distance);
        }

        //-----------------------------------------------------------------------------
        public static IRoute buildLineRoute(EbVector3 start_position, EbVector3 end_position)
        {
            List<EbVector3> points_list = new List<EbVector3>();
            points_list.Add(start_position);
            points_list.Add(end_position);

            RouteLine route = new RouteLine();
            route.create(0, points_list);
            return route;
        }
    }
}

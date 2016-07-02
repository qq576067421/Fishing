using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class RouteObjectMgr
    {
        //---------------------------------------------------------------------
        Dictionary<string, DynamicSystem> mRouteDic = new Dictionary<string, DynamicSystem>();

        //---------------------------------------------------------------------
        public RouteObjectMgr(List<RouteJsonPacket> json_packet_list)
        {
            foreach (var it in json_packet_list)
            {
                parseJson(it);
            }
        }

        //---------------------------------------------------------------------
        void parseJson(RouteJsonPacket json)
        {
            try
            {
                RouteObject route_object = BaseJsonSerializer.deserialize<RouteObject>(json.JsonString);
                DynamicSystem system = new DynamicSystem(new EbVector3(route_object.initPos.x, route_object.initPos.y, 0), getSpeed(route_object.initVelocity));
                foreach (var it in route_object.accs)
                {
                    system.add(it.start_time, getSpeed(it.acc), it.duration);
                }

                mRouteDic.Add(json.FileName, system);
            }
            catch (Exception e)
            {
                EbLog.Note(e.ToString());
            }
        }

        //---------------------------------------------------------------------
        EbVector3 getSpeed(RouteObject.Velocity velocity)
        {
            EbVector3 speed = CLogicUtility.getDirection(velocity.direction);
            return speed * velocity.speed;
        }

        //---------------------------------------------------------------------
        public DynamicSystem getDynamicSystem(int id, float delay_time)
        {
            string key = id.ToString();
            if (mRouteDic.ContainsKey(key))
            {
                DynamicSystem system = mRouteDic[key].clone();
                system.setDelayTime(delay_time);
                return system;
            }

            return null;
        }
    }

    public class RouteJsonPacket
    {
        //---------------------------------------------------------------------
        string mFileName = string.Empty;
        string mJsonString = string.Empty;
        public string FileName { get { return mFileName; } }
        public string JsonString { get { return mJsonString; } }

        //---------------------------------------------------------------------
        public RouteJsonPacket(string file_name, string json_string)
        {
            mFileName = file_name;
            mJsonString = json_string;
        }
    }
}


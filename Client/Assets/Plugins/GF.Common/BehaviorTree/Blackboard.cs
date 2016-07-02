using System;
using System.Collections.Generic;

namespace GF.Common
{
    public class Blackboard
    {
        //---------------------------------------------------------------------
        Dictionary<string, object> mMapData = new Dictionary<string, object>();

        //---------------------------------------------------------------------
        public void setData(string key, object data)
        {
            mMapData[key] = data;
        }

        //---------------------------------------------------------------------
        public object getData(string key)
        {
            object data;
            mMapData.TryGetValue(key, out data);
            return data;
        }

        //---------------------------------------------------------------------
        public bool hasData(string key)
        {
            return mMapData.ContainsKey(key);
        }
    }
}

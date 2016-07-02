using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataRoute : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum RouteType
        {
            Default = -1,
            LINE = 0,
            BEZIER = 1
        }

        public enum RouteCategory
        {
            Default = -1,
            Small = 0,
            Medium = 1,
            Big = 2,
            SmallAndMedium = 3,
            MediumAndBig = 4,
            All = 5
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public List<EbVector3> ListPoints { get; private set; }
        public RouteType Type { get; private set; }
        public RouteCategory routeCategory { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            string strPoints = prop_set.getPropString("T_Points").get();
            if (!string.IsNullOrEmpty(strPoints))
            {
                ListPoints = new List<EbVector3>();
                string[] arrayPoints = strPoints.Split(';');
                bool is_x = true;
                float x = 0;
                foreach (var it in arrayPoints)
                {
                    if (is_x)
                    {
                        x = float.Parse(it);
                        is_x = false;
                    }
                    else
                    {
                        ListPoints.Add(new EbVector3(x, float.Parse(it), 0));
                        is_x = true;
                    }
                }
            }
            var prop_type = prop_set.getPropInt("I_Type");
            Type = prop_type == null ? RouteType.Default : (RouteType)prop_type.get();
            var prop_routecategory = prop_set.getPropInt("I_RouteCategory");
            routeCategory = prop_routecategory == null ? RouteCategory.Default : (RouteCategory)prop_routecategory.get();
        }
    }
}

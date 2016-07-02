using System;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public static class ViVectorStatic
    {
        public static Vector3 convert(this EbVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector3 logic2pixel(this EbVector3 v)
        {
            EbVector3 pixel_pos = CCoordinate.logic2toolkitPos(v);
            return pixel_pos.convert();
        }
    }
}

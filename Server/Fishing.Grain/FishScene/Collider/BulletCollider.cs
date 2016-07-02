using System;
using GF.Common;

namespace Ps
{
    public class BulletCollider : TagCollider
    {
        //---------------------------------------------------------------------
        public BulletCollider(float center_x, float center_y, float width, float height)
            : base(1, center_x, center_y, width, height)
        { }
    }
}

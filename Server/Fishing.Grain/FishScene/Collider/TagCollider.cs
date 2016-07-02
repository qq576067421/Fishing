using System;
using GF.Common;

namespace Ps
{
    public class TagCollider : CBoxCollider, IPoint<RectArea>
    {
        //---------------------------------------------------------------------
        public delegate void onCollisionDelegate(TagCollider other);
        public onCollisionDelegate onCollision;
        public int Tag { get { return mTag; } }
        int mTag;

        //---------------------------------------------------------------------
        public TagCollider(int tag, float center_x, float center_y, float width, float height)
            : base(center_x, center_y, width, height)
        {
            mTag = tag;
        }

        //---------------------------------------------------------------------
        public bool isInArea(RectArea area)
        {
            return
                area.hasPoint(mQuadrantPoints.TL.x + mPosition.x, mQuadrantPoints.TL.y + mPosition.y) ||
                area.hasPoint(mQuadrantPoints.TR.x + mPosition.x, mQuadrantPoints.TR.y + mPosition.y) ||
                area.hasPoint(mQuadrantPoints.BL.x + mPosition.x, mQuadrantPoints.BL.y + mPosition.y) ||
                area.hasPoint(mQuadrantPoints.BR.x + mPosition.x, mQuadrantPoints.BR.y + mPosition.y);
        }
    }
}

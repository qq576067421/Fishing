using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class CBoxCollider
    {
        //---------------------------------------------------------------------
        protected struct QuadrantPoints
        {
            public EbVector3 TL;
            public EbVector3 TR;
            public EbVector3 BL;
            public EbVector3 BR;
        }

        //---------------------------------------------------------------------
        protected QuadrantPoints mQuadrantPoints;
        protected float mAngle = 0;
        protected EbVector3 mPosition;

        //---------------------------------------------------------------------
        public CBoxCollider(float center_x, float center_y, float width, float height)
        {
            float half_width = width * 0.5f;
            float half_height = height * 0.5f;

            mQuadrantPoints.TL.x = center_x - half_width;
            mQuadrantPoints.TL.y = center_y + half_height;

            mQuadrantPoints.TR.x = center_x + half_width;
            mQuadrantPoints.TR.y = center_y + half_height;

            mQuadrantPoints.BL.x = center_x - half_width;
            mQuadrantPoints.BL.y = center_y - half_height;

            mQuadrantPoints.BR.x = center_x + half_width;
            mQuadrantPoints.BR.y = center_y - half_height;
        }

        //---------------------------------------------------------------------
        public void setPosition(EbVector3 position)
        {
            mPosition = position;
        }

        //---------------------------------------------------------------------
        public void setDirection(float angle)
        {
            mAngle = angle;
        }

        //---------------------------------------------------------------------
        public bool isIn(CBoxCollider other)
        {
            return
                other.isInBox(mQuadrantPoints.TL) ||
                other.isInBox(mQuadrantPoints.TR) ||
                other.isInBox(mQuadrantPoints.BL) ||
                other.isInBox(mQuadrantPoints.BR) ||
                isInBox(other.mQuadrantPoints.TL + other.mPosition) ||
                isInBox(other.mQuadrantPoints.TR + other.mPosition) ||
                isInBox(other.mQuadrantPoints.BL + other.mPosition) ||
                isInBox(other.mQuadrantPoints.BR + other.mPosition);
        }

        //---------------------------------------------------------------------
        EbVector3 trans2local(EbVector3 point)
        {
            EbVector3 local = point - mPosition;
            local = CLogicUtility.getVector2ByRotateAngle(local, -mAngle);
            return local;
        }

        //---------------------------------------------------------------------
        public bool isInBox(EbVector3 point)
        {
            EbVector3 local = trans2local(point);
            return
                local.x >= mQuadrantPoints.TL.x &&
                local.x <= mQuadrantPoints.TR.x &&
                local.y <= mQuadrantPoints.TL.y &&
                local.y >= mQuadrantPoints.BL.y;
        }
    }
}

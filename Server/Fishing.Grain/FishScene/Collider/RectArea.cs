using System;
using GF.Common;

namespace Ps
{
    public class RectArea : IArea
    {
        //---------------------------------------------------------------------
        EbVector3 mOrigin;
        EbVector3 mSize;

        //---------------------------------------------------------------------
        public RectArea(EbVector3 origin, EbVector3 size)
        {
            mOrigin = origin;
            mSize = size;
        }

        //---------------------------------------------------------------------
        public IArea getArea(_eQuadTreeLeaf leaf)
        {
            float half_size_x = mSize.x * 0.5f;
            float half_size_y = mSize.y * 0.5f;
            EbVector3 half_size = mSize * 0.5f;
            switch (leaf)
            {
                case _eQuadTreeLeaf.UR:
                    return new RectArea(mOrigin + new EbVector3(half_size_x, half_size_y, 0), half_size);
                case _eQuadTreeLeaf.UL:
                    return new RectArea(mOrigin + new EbVector3(0, half_size_y, 0), half_size);
                case _eQuadTreeLeaf.LL:
                    return new RectArea(mOrigin, half_size);
                default://LR
                    return new RectArea(mOrigin + new EbVector3(half_size_x, 0, 0), half_size);
            }
        }

        //---------------------------------------------------------------------
        public bool hasPoint(float x, float y)
        {
            return
                x >= mOrigin.x &&
                x <= mOrigin.x + mSize.x &&
                y >= mOrigin.y &&
                y <= mOrigin.y + mSize.y;
        }
    }
}

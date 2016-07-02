using System;
using GF.Common;

namespace Ps
{
    public class FishCollider : TagCollider
    {
        //---------------------------------------------------------------------
        public CLogicFish LogicFish { get { return mLogicFish; } }
        CLogicFish mLogicFish = null;

        //---------------------------------------------------------------------
        public FishCollider(float center_x, float center_y, float width, float height, CLogicFish fish)
            : base(0, center_x, center_y, width, height)
        {
            mLogicFish = fish;
        }
    }
}

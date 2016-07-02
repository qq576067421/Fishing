using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class RouteParaCurve : IRoute
    {
        //-----------------------------------------------------------------------------
        public bool IsEndRoute { get { return mIsEndRoute; } }
        public EbVector3 Position { get { return mPosition; } }
        public EbVector3 Direction { get { return mDirection; } }
        public float OverDistance { get { return mOverDistance; } }
        bool mIsEndRoute = false;
        EbVector3 mPosition;
        EbVector3 mDirection;
        float mOverDistance = 0;
        float mParaParamA = 0;
        float mParaParamB = 0;
        EbVector3 mInitPosition;
        float mInitAngle = 0;
        float mTime = 0;
        float mDuration = 0;

        //-----------------------------------------------------------------------------
        public void create(EbVector3 init_position, float init_angle, float width, float height)
        {
            mInitPosition = init_position;
            mInitAngle = init_angle;
            mDirection = CLogicUtility.getDirection(mInitAngle);
            mDuration = width;

            mParaParamB = 4 * height / width;
            mParaParamA = -mParaParamB / width;
        }

        //-----------------------------------------------------------------------------
        public void next(float elapsed_tm, float speed)
        {
            if (mIsEndRoute) return;

            mTime += elapsed_tm;

            if (mTime >= mDuration)
            {
                mPosition = mInitPosition;
                mIsEndRoute = true;
                return;
            }

            float delta_y = _getCurveValue(mTime);
            mPosition = mInitPosition + CLogicUtility.getVector2ByRotateAngle(new EbVector3(0, delta_y, 0), mInitAngle);
        }

        //-----------------------------------------------------------------------------
        public void reset(float total_distance)
        {
            throw new NotImplementedException();
        }

        //-------------------------------------------------------------------------
        float _getCurveValue(float t)
        {
            return mParaParamA * t * t + mParaParamB * t;
        }
    }
}

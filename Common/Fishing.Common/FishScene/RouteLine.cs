using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class RouteLine : IRoute
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

        List<EbVector3> mPoints = new List<EbVector3>();
        List<float> mGapList = new List<float>();

        int mAtWhichPoint = 0;
        float mCurrentT = 0;
        bool mIsTurning = false;
        EbVector3 mTargetDirection;
        float mAngleTurning = 30;
        float mTurningMaxTime = 0.3f;
        float mDelayTime = -1;

        //-----------------------------------------------------------------------------
        public void create(float delay_time, List<EbVector3> points)
        {


            for (int i = 0; i < points.Count; i++)
            {
                mPoints.Add(points[i]);
                if (i >= points.Count - 1) continue;
                mGapList.Add(points[i].getDistance(points[i + 1]));
            }

            mDirection = (mPoints[mAtWhichPoint + 1] - mPoints[mAtWhichPoint]).normalized;

            next(0, 100);

            mDelayTime = delay_time;
        }

        //-----------------------------------------------------------------------------
        public void next(float elapsed_tm, float speed)
        {
            if (mDelayTime > 0)
            {
                mDelayTime -= elapsed_tm;
                return;
            }
            if (mIsEndRoute || speed == 0) return;

            EbVector3 current_point = EbVector3.lerp(mPoints[mAtWhichPoint], mPoints[mAtWhichPoint + 1], mCurrentT);
            float max_distance = current_point.getDistance(mPoints[mAtWhichPoint + 1]);
            float need_distance = elapsed_tm * speed;
            if (need_distance < max_distance)
            {
                mCurrentT = (mGapList[mAtWhichPoint] - max_distance + need_distance) / mGapList[mAtWhichPoint];
                mPosition = EbVector3.lerp(mPoints[mAtWhichPoint], mPoints[mAtWhichPoint + 1], mCurrentT);
                _calculateDirection(elapsed_tm);
            }
            else
            {
                mAtWhichPoint += 1;
                mCurrentT = 0;
                if (mAtWhichPoint >= mPoints.Count - 1)
                {
                    mPosition = mPoints[mPoints.Count - 1];
                    mIsEndRoute = true;
                }
                else
                {
                    _initTurning();
                    next((need_distance - max_distance) / speed, speed);
                }
            }
        }

        //-----------------------------------------------------------------------------
        void _initTurning()
        {
            mIsTurning = true;
            mTargetDirection = (mPoints[mAtWhichPoint + 1] - mPoints[mAtWhichPoint]).normalized;
            float included_angle = _abs(CLogicUtility.getAngle(mTargetDirection - mDirection));
            mAngleTurning = included_angle / mTurningMaxTime;
        }

        //-----------------------------------------------------------------------------
        void _calculateDirection(float elapsed_tm)
        {
            if (mIsTurning)
            {
                float included_angle = _abs(CLogicUtility.getAngle(mTargetDirection - mDirection));
                float t = mAngleTurning * elapsed_tm / included_angle;

                mDirection = EbVector3.lerp(mDirection, mTargetDirection, t);

                if (t >= 1) mIsTurning = false;
            }
        }

        //-----------------------------------------------------------------------------
        float _abs(float v)
        {
            if (v < 0) return -v;
            return v;
        }

        //-----------------------------------------------------------------------------
        public void reset(float total_distance)
        {
            throw new NotImplementedException();
        }
    }
}

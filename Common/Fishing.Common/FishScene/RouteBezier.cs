using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class RouteBezier : IRoute
    {
        //---------------------------------------------------------------------
        List<EbVector3> mPoints = new List<EbVector3>();
        MultiBeziers mMultiBeziers = new MultiBeziers();

        //---------------------------------------------------------------------
        List<EbVector3> getMiddle(List<EbVector3> points)
        {
            List<EbVector3> middle = new List<EbVector3>();
            for (int i = 0; i < points.Count - 1; ++i)
            {
                middle.Add(EbVector3.lerp(points[i], points[i], 0.5f));
            }
            return middle;
        }

        //-----------------------------------------------------------------------------
        public void create(List<EbVector3> points)
        {
            foreach (var it in points)
            {
                mPoints.Add(it);
            }

            List<EbVector3> middle_of_origin_points = getMiddle(mPoints);
            List<EbVector3> middle_of_middle_points = getMiddle(middle_of_origin_points);

            List<EbVector3> translation_of_middle_points = new List<EbVector3>();

            for (int i = 0; i < middle_of_middle_points.Count; ++i)
            {
                translation_of_middle_points.Add(mPoints[i + 1] - middle_of_middle_points[i]);
            }

            for (int i = 0; i < middle_of_origin_points.Count - 3; ++i)
            {
                mMultiBeziers.addBezier(
                    new Bezier(
                        mPoints[i + 1],
                        middle_of_middle_points[i + 1] + translation_of_middle_points[i],
                        middle_of_middle_points[i + 2] + translation_of_middle_points[i],
                        mPoints[i + 2]));
            }
        }

        public bool IsEndRoute
        {
            get { return mMultiBeziers.IsEndRoute; }
        }

        public EbVector3 Position
        {
            get { return mMultiBeziers.Position; }
        }

        public EbVector3 Direction
        {
            get { return EbVector3.Zero; }
        }

        public float OverDistance
        {
            get { throw new NotImplementedException(); }
        }

        public void next(float elapsed_tm, float speed)
        {
            mMultiBeziers.next(elapsed_tm, speed);
        }

        public void reset(float total_distance)
        {
            throw new NotImplementedException();
        }
    }

    class MultiBeziers
    {
        public bool IsEndRoute
        {
            get { return mIsEndRoute; }
        }

        public EbVector3 Position
        {
            get { return mPosition; }
        }

        public EbVector3 Direction
        {
            get { throw new NotImplementedException(); }
        }

        List<Bezier> mBeziers = new List<Bezier>();
        int index = 0;
        float t = 0;

        EbVector3 mPosition;
        bool mIsEndRoute = false;
        float mTotalLen = 0;

        public void addBezier(Bezier bezier)
        {
            mBeziers.Add(bezier);
        }

        public void init()
        {
            foreach (var it in mBeziers)
            {
                mTotalLen += it.Length;
            }

        }

        public void next(float elapsed_tm, float speed)
        {
            float total_t = mTotalLen / speed;
            float current_t = mBeziers[index].Length / speed;
            float rest = speed * (current_t - t);
            if (rest < elapsed_tm * speed)
            {
                if (index >= mBeziers.Count - 1)
                {
                    mIsEndRoute = true;
                    mPosition = mBeziers[index].getPoint(1);
                    return;
                }
                float next_tm = elapsed_tm - rest / speed;
                if (next_tm < 0) next_tm = 0;
                next(next_tm, speed);
            }
            else
            {
                t = t + rest / speed;
                if (t > 1) t = 1;
                mPosition = mBeziers[index].getPoint(t);
            }
        }
    }

    class Bezier
    {
        EbVector3 mPoint0;
        EbVector3 mPoint1;
        EbVector3 mPoint2;
        EbVector3 mPoint3;

        public float Length { get { return mLength; } }
        float mLength;

        public Bezier(EbVector3 p0, EbVector3 p1, EbVector3 p2, EbVector3 p3)
        {
            mPoint0 = p0;
            mPoint1 = p1;
            mPoint2 = p2;
            mPoint3 = p3;

            mLength = mPoint0.getDistance(mPoint1) +
                mPoint1.getDistance(mPoint2) +
                mPoint2.getDistance(mPoint3);
        }

        public EbVector3 getPoint(float t)
        {//t[0,1]
            return mPoint0 * powerOf3(1 - t) +
                mPoint1 * 3 * t * powerOf2(1 - t) +
                mPoint2 * 3 * powerOf2(t) * (1 - t) +
                mPoint3 * powerOf3(t);
        }

        float powerOf3(float n)
        {
            return n * n * n;
        }

        float powerOf2(float n)
        {
            return n * n;
        }
    }
}

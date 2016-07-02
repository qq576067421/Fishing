using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class CLogicUtility
    {
        //---------------------------------------------------------------------
        // 角度转弧度
        public static float ConvertDegreesToRadians(float degrees)
        {
            float radians = ((float)Math.PI / 180) * degrees;
            return radians;
        }

        //---------------------------------------------------------------------
        // 弧度转角度
        public static float ConvertRadiansToDegrees(float radians)
        {
            float degrees = (180 / (float)Math.PI) * radians;
            return degrees;
        }

        //---------------------------------------------------------------------
        // 根据方向向量把对应角度的算出来,垂直向上为0度，向左转动为负，向右为正
        public static float getAngle(EbVector3 a)
        {
            float angle = MathHelper.Angle(a, EbVector3.UnitY);

            EbVector3 va = new EbVector3(a.x, a.y, 0);
            EbVector3 vb = new EbVector3(EbVector3.UnitY.x, EbVector3.UnitY.y, 0);
            EbVector3 vr = EbVector3.cross(va, vb);

            if (vr.z < 0)
            {
                angle = -angle;
            }

            return ConvertRadiansToDegrees(angle);
        }

        //---------------------------------------------------------------------
        public static float getRightAngle(float angle)
        {
            return getAngle(getVector2ByRotateAngle(getDirection(angle), 90));
        }

        //---------------------------------------------------------------------
        public static EbVector3 getVector2ByRotateAngle(EbVector3 v, float angle)
        {
            angle = -angle;

            float cos = (float)Math.Cos(ConvertDegreesToRadians(angle));
            float sin = (float)Math.Sin(ConvertDegreesToRadians(angle));

            float x = v.x * cos - v.y * sin;
            float y = v.x * sin + v.y * cos;

            return new EbVector3(x, y, 0);
        }

        //---------------------------------------------------------------------
        // 根据角度把对应的方向向量算出来
        public static EbVector3 getDirection(float angle)
        {
            return getVector2ByRotateAngle(EbVector3.UnitY, angle).normalized;
        }

        //---------------------------------------------------------------------
        // 获取当前路径点
        // 参数：起点（米），速度（米/秒），时间（秒）
        public static EbVector3 getCurrentPos(EbVector3 start_pos, float angle, float speed, float time_span)
        {
            return start_pos + getDirection(angle) * speed * time_span;
        }
    }

    public class CAngle
    {
        //---------------------------------------------------------------------
        public float Value { get { return mAngle; } }
        float mAngle = 0f;

        //---------------------------------------------------------------------
        public CAngle()
        {
        }

        //---------------------------------------------------------------------
        public CAngle(float angle)
        {
            mAngle = regularAngle(angle);
        }

        //---------------------------------------------------------------------
        float regularAngle(float angle)
        {
            float regular_angle = angle % 360;

            if (regular_angle <= -180) regular_angle = -180 - regular_angle;
            if (regular_angle > 180) regular_angle = 180 - regular_angle;

            return regular_angle;
        }

        //---------------------------------------------------------------------
        public static CAngle operator +(CAngle left, CAngle right)
        {
            return left + right.mAngle;
        }

        //---------------------------------------------------------------------
        public static CAngle operator +(CAngle left, float angle)
        {
            return new CAngle(left.mAngle + angle);
        }

        //---------------------------------------------------------------------
        public CDirection toDirection()
        {
            return new CDirection(mAngle);
        }
    }

    public class CDirection
    {
        //---------------------------------------------------------------------
        EbVector3 mDirection;

        //---------------------------------------------------------------------
        public CDirection(float angle)
        {
            mDirection = CLogicUtility.getDirection(angle);
        }

        //---------------------------------------------------------------------
        public CDirection(EbVector3 direction)
        {
            mDirection = direction.normalized;
        }

        //---------------------------------------------------------------------
        public static CDirection operator +(CDirection left_direction, CDirection right_direction)
        {
            return new CDirection(1);
        }

        //---------------------------------------------------------------------
        public static bool operator ==(CDirection left_direction, CDirection right_direction)
        {
            return left_direction.mDirection == right_direction.mDirection;
        }

        //---------------------------------------------------------------------
        public static bool operator !=(CDirection left_direction, CDirection right_direction)
        {
            return left_direction.mDirection != right_direction.mDirection;
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator *(CDirection direction, float distance)//返回位移
        {
            return direction.mDirection * distance;
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator *(float distance, CDirection direction)//返回位移
        {
            return direction.mDirection * distance;
        }

        //---------------------------------------------------------------------
        public static CDirection slerp(CDirection left_direction, CDirection right_direction, float t)
        {
            return new CDirection(EbVector3.lerp(left_direction.mDirection, right_direction.mDirection, t));
        }

        //---------------------------------------------------------------------
        public static float angle(CDirection left_direction, CDirection right_direction)
        {
            float left = CLogicUtility.getAngle(left_direction.mDirection);
            float right = CLogicUtility.getAngle(right_direction.mDirection);
            return _abs(left - right);
        }

        //-----------------------------------------------------------------------------
        static float _abs(float v)
        {
            if (v < 0) return -v;
            return v;
        }

        //---------------------------------------------------------------------
        public CAngle toAngle()
        {
            return new CAngle(CLogicUtility.getAngle(mDirection));
        }
    }
}

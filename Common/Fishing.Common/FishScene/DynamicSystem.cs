using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class DynamicSystem
    {
        //---------------------------------------------------------------------
        public EbVector3 mPosition;
        public EbVector3 mSpeed;
        Dictionary<float, Acceleration> mAcceleration = new Dictionary<float, Acceleration>();
        bool mIsEnd = false;
        public bool IsEnd { get { return mIsEnd; } }
        float mDelayTime = 0;
        public bool IsDelay { get { return mIsDelay; } }
        bool mIsDelay = true;

        //---------------------------------------------------------------------
        class Acceleration
        {
            public EbVector3 acceleration = EbVector3.Zero;
            public float duration = 0;
            public bool isCal = false;
            public EbVector3 acc = EbVector3.Zero;
            public EbVector3 t = EbVector3.Zero;
            public float m;

            public Acceleration clone()
            {
                Acceleration acc = new Acceleration();
                acc.acceleration = acceleration;
                acc.duration = duration;
                return acc;
            }
        }

        //---------------------------------------------------------------------
        public DynamicSystem(EbVector3 init_pos, EbVector3 init_speed)
        {
            mPosition = init_pos;
            mSpeed = init_speed;
        }

        //---------------------------------------------------------------------
        public void add(float time_x, EbVector3 acce, float duration)
        {
            if (duration <= 0) return;

            if (mAcceleration.ContainsKey(time_x))
            {
                EbLog.Note("DynamicSystem::add::error.");
                return;
            }

            Acceleration acceleration = new Acceleration();
            acceleration.acceleration = acce;
            acceleration.duration = duration;
            mAcceleration.Add(time_x, acceleration);
        }

        //---------------------------------------------------------------------
        public void setDelayTime(float delay_time)
        {
            mDelayTime = delay_time;
        }

        //---------------------------------------------------------------------
        public EbVector3 getAcce(float time_x, EbVector3 current)
        {
            time_x = time_x - mDelayTime;
            if (time_x > 0) mIsDelay = false;
            Acceleration acceleration = null;
            float start_time = 0;

            foreach (var it in mAcceleration)
            {
                if (time_x > it.Key)
                {
                    acceleration = it.Value;
                    start_time = it.Key;
                }
                else
                {
                    mIsEnd = false;
                    break;
                }
            }

            if (acceleration == null) return EbVector3.Zero;

            if (time_x > acceleration.duration + start_time)
            {
                return EbVector3.Zero;
            }

            if (acceleration.isCal) return calAcc(acceleration, time_x - start_time);

            acceleration.acc = (acceleration.acceleration - current) / acceleration.duration;
            acceleration.isCal = true;

            acceleration.t = acceleration.acc * EbMath.PI / 2;
            acceleration.m = EbMath.PI / acceleration.duration;

            return calAcc(acceleration, 0);
        }

        //---------------------------------------------------------------------
        EbVector3 calAcc(Acceleration acceleration, float time)
        {
            return acceleration.t * EbMath.Sin(acceleration.m * time);
        }

        //---------------------------------------------------------------------
        public DynamicSystem clone()
        {
            DynamicSystem system = new DynamicSystem(mPosition, mSpeed);
            foreach (var it in mAcceleration)
            {
                system.mAcceleration.Add(it.Key, it.Value.clone());
            }
            return system;
        }
    }
}

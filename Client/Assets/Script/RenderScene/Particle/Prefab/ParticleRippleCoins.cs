using UnityEngine;
using System.Collections.Generic;
using Ps;

public class ParticleRippleCoins : ParticleCoins
{
    List<Coin> mCoins = new List<Coin>();
    float mRippleDuration = 0.2f;
    float mMoveSpeed = 500;
    float mMoveDuration = 0.5f;
    float mPauseDuration = 0.5f;
    bool mIsLine = false;
    RippleState mRippleState = RippleState.Ripple;
    float mSprintDuration = 1.3f;
    float mSpringEachDelay = 0.01f;

    protected override void create()
    {
        base.create();
        mMoveDuration = Vector3.Distance(mInitPosition, mTargetPosition) / mMoveSpeed;

        if (Random.Range(0, 90) < 45)
        {
            rippleInit();
        }
        else
        {
            lineInit();
        }

        if (mCoins.Count <= 1)
        {
            mRippleState = RippleState.Move;
        }
    }

    void rippleInit()
    {
        float init_angle = Random.Range(0, 90);
        float each_angle = 360f / (float)mCoinsCount;

        float radiusMax = 80;
        float radiusMin = 50;
        int count = mCoinsCount;

        while (count > 0)
        {
            float angle_current = init_angle + each_angle * (count - 1) * Mathf.Deg2Rad;
            mCoins.Add(
                newCoin(mInitPosition, mInitPosition + Random.Range(radiusMin, radiusMax) * 
                new Vector3(Mathf.Cos(angle_current), Mathf.Sin(angle_current), 0), mTargetPosition, mLayer, 0));
            count = count - 1;
        }

        mIsLine = false;
    }

    void lineInit()
    {
        float gap = 50f;
        float left_distance = -gap * (float)mCoinsCount / 2f;

        int count = mCoinsCount;
        while (count > 0)
        {
            mCoins.Add(
                newCoin(mInitPosition, mInitPosition + new Vector3(left_distance + count * gap, 0, 0), 
                mTargetPosition, mLayer + count * 0.005f, mSpringEachDelay * count));
            count = count - 1;
        }

        mIsLine = true;
    }

    public override void destroy()
    {
        foreach (var it in mCoins)
        {
            if (it.Particle == null) continue;
            Destroy(it.Particle);
        }
        mCoins.Clear();
    }

    public void Update()
    {
        switch (mRippleState)
        {
            case RippleState.Ripple:
                ripple();
                mRippleDuration -= Time.deltaTime;
                if (mRippleDuration < 0)
                {
                    if (mIsLine)
                    {
                        mRippleState = RippleState.Spring;
                    }
                    else
                    {
                        mRippleState = RippleState.Pause;
                    }
                }
                break;
            case RippleState.Pause:
                mPauseDuration -= Time.deltaTime;
                if (mPauseDuration < 0)
                {
                    mRippleState = RippleState.Move;
                }
                break;
            case RippleState.Move:
                move();
                mMoveDuration -= Time.deltaTime;
                if (mMoveDuration < 0)
                {
                    mRippleState = RippleState.End;
                }
                break;
            case RippleState.Spring:
                spring();
                mSprintDuration -= Time.deltaTime;
                if (mSprintDuration < 0)
                {
                    mRippleState = RippleState.Move;
                }
                break;
            default:
                SignFinished();
                break;
        }
    }


    void ripple()
    {
        foreach (var it in mCoins)
        {
            if (it.Particle == null) continue;
            it.ripple(Time.deltaTime);
        }
    }

    void move()
    {
        foreach (var it in mCoins)
        {
            if (it.Particle == null) continue;
            it.move(Time.deltaTime);
        }
    }

    void spring()
    {
        foreach (var it in mCoins)
        {
            if (it.Particle == null) continue;
            it.spring(Time.deltaTime);
        }
    }

    Coin newCoin(Vector3 init_pos, Vector3 ripple_target_pos, Vector3 move_target_pos, float layer, float spring_delay)
    {
        Coin coin = new Coin();

        coin.RippleTargetPostion = ripple_target_pos;
        coin.OriginRippleTargetPostion = ripple_target_pos;
        coin.InitPostion = init_pos;
        coin.MoveTargetPostion = move_target_pos;
        coin.totalRippleSeconds = mRippleDuration;
        coin.totalMoveSeconds = mMoveDuration;
        coin.mSpringTimeCounter.totalSeconds = mSprintDuration;
        coin.layer = layer;
        coin.mSpringEachDelay = spring_delay;
        coin.Particle = ParticleLoader.Load(getPrefabName(), init_pos);

        return coin;
    }

    protected virtual string getPrefabName()
    {
        return "Game/Particle/EXyinbi";
    }

    class Coin
    {
        public struct TimeCounter
        {
            public float elapsedSeconds;
            public float totalSeconds;
        }
        public Vector3 MoveTargetPostion;
        public Vector3 RippleTargetPostion;
        public Vector3 OriginRippleTargetPostion;
        public Vector3 InitPostion;
        public GameObject Particle;
        float elapsedSeconds = 0;
        float elapsedMoveSeconds = 0;
        public float totalRippleSeconds = 2;
        public float totalMoveSeconds = 2;
        public float layer = 0;
        int mSpringCount = 3;
        float mMaxHeight = 110f;
        float mMaxTime = 0.4f;
        float mOffset = 0;
        float mOffsetSpeed = 0;
        public float mSpringEachDelay;
        bool isFirst = true;

        public TimeCounter mSpringTimeCounter;

        public Coin()
        {
            mSpringTimeCounter.elapsedSeconds = 0;
            mSpringTimeCounter.totalSeconds = 3f;
        }

        public void ripple(float elapsed_tm)
        {
            if (Particle == null) return;
            elapsedSeconds += elapsed_tm;

            setParticlePosition(Vector3.Lerp(InitPostion, RippleTargetPostion, elapsedSeconds / totalRippleSeconds));
        }

        public void move(float elapsed_tm)
        {
            if (Particle == null) return;
            elapsedMoveSeconds += elapsed_tm;

            setParticlePosition(Vector3.Lerp(RippleTargetPostion, MoveTargetPostion, elapsedMoveSeconds / totalMoveSeconds));
        }

        public void spring(float elapsed_tm)
        {
            if (Particle == null) return;
            //if (mSpringEachDelay > 0)
            //{
            //    mSpringEachDelay -= elapsed_tm;
            //    return;
            //}

            mSpringTimeCounter.elapsedSeconds += elapsed_tm;

            if (mSpringCount < 1) return;
            if (mSpringTimeCounter.elapsedSeconds >= mMaxTime)
            {
                mSpringTimeCounter.elapsedSeconds = 0;
                mSpringCount--;
                if (isFirst)
                {
                    reduceHeight(3f / 4f);
                    isFirst = false;
                }
                else {
                    reduceHeight(3f / 4f);
                }
                return;
            }

            mOffset += mOffsetSpeed * elapsed_tm;

            RippleTargetPostion = new Vector3(OriginRippleTargetPostion.x + mOffset, OriginRippleTargetPostion.y + curve(mMaxHeight, mMaxTime, mSpringTimeCounter.elapsedSeconds), 0);
            setParticlePosition(RippleTargetPostion);
        }

        void reduceHeight(float scale)
        {
            mMaxHeight *= scale;
            //Debug.LogWarning(mMaxHeight);
            //mMaxTime *= Mathf.Sqrt(scale);
            //mMaxTime *= Mathf.Sqrt(scale);
        }

        float curve(float max_height, float max_time, float t)
        {
            float b = 4 * max_height / max_time;
            float a = -b / max_time;

            return a * t * t + b * t;
        }

        void setParticlePosition(Vector3 pos)
        {
            pos.z = layer;
            Particle.transform.position = pos;
        }
    }

    enum RippleState
    {
        Ripple,
        Pause,
        Move,
        Spring,
        End
    }
}
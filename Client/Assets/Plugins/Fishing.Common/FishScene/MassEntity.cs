using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class MassEntity
    {
        //-------------------------------------------------------------------------
        public bool IsInScene { get { return mIsInScene; } }// 点是否在场景内，不是则在场景外
        public bool IsOutScreen { get { return mEnteredScene && !mIsInScene; } }// 点从场景外进入场景并且又出去了
        public bool IsEndRoute { get { return mIsEndRoute; } }// 沿路径模式运行是否到达路径的终点
        bool mIsInScene = false;
        bool mEnteredScene = false;
        bool mIsEndRoute = false;
        float mSceneBoardSize = 200f;
        public EbVector3 Position { get { return mPosition; } }
        public float Angle { get { return mAngle; } }
        EbVector3 mPosition;// 位置
        float mAngle = 0;
        float mSpeed = 10f;
        float mAngleSpeed = 0;
        IRoute mIRoute = null;

        DynamicSystem mDynamicSystem;
        float mTotalElapsedTime = 0;
        EbVector3 mViSpeed;

        //-------------------------------------------------------------------------
        public MassEntity()
        {
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            if (mDynamicSystem != null)
            {
                mTotalElapsedTime += elapsed_tm;
                EbVector3 acc = mDynamicSystem.getAcce(mTotalElapsedTime, mViSpeed);
                if (mDynamicSystem.IsDelay) { }
                else
                {
                    mViSpeed = mViSpeed + acc * elapsed_tm;
                    mPosition = mPosition + mViSpeed * elapsed_tm;
                    mAngle = CLogicUtility.getAngle(mViSpeed);

                    if (mDynamicSystem.IsEnd)
                    {
                        mIsEndRoute = true;
                    }
                }
            }
            else
                if (mIRoute != null)
            {
                mIRoute.next(elapsed_tm, mSpeed);
                mPosition = mIRoute.Position;
                mAngle = CLogicUtility.getAngle(mIRoute.Direction);

                if (mIRoute.IsEndRoute) { mIsEndRoute = true; }
            }
            else
            {
                //mAngle += mAngleSpeed * elapsed_tm;

                float newAngle = mAngle + mAngleSpeed * elapsed_tm;

                if (mAngleSpeed > 0)
                {
                    if (mAngle <= 180 && newAngle > 180)
                    {
                        mAngle = newAngle - 360;
                    }
                    else
                    {
                        mAngle = newAngle;
                    }
                }
                else
                {
                    //if (mAngle <= -180) mAngle = mAngle - 180;
                    if (mAngle > -180 && newAngle <= 180)
                    {
                        mAngle = newAngle + 360;
                    }
                    else
                    {
                        mAngle = newAngle;
                    }
                }

                //mAngle = regularAngle(mAngle);

                mPosition += CLogicUtility.getDirection(mAngle) * mSpeed * elapsed_tm;
            }

            _updateState();
        }

        //-------------------------------------------------------------------------
        public void setPosition(EbVector3 position)
        {
            mPosition = position;
        }

        //-------------------------------------------------------------------------
        public void setRoute(IRoute route)
        {
            mIRoute = route;
        }

        public void setDynamicSystem(DynamicSystem system)
        {
            mDynamicSystem = system;

            mViSpeed = mDynamicSystem.mSpeed;
            mPosition = mDynamicSystem.mPosition;
        }

        //-------------------------------------------------------------------------
        // 设置速度大小，不改变方向
        public void setSpeed(float speed)
        {
            mSpeed = speed;
        }

        //---------------------------------------------------------------------
        public void setAngleSpeed(float angle_speed)
        {
            mAngleSpeed = angle_speed;
        }

        //-------------------------------------------------------------------------
        public void setDirection(float angle)
        {
            mAngle = angle;
        }

        //---------------------------------------------------------------------
        bool _isInScene(EbVector3 position, float border_size = 0)
        {
            return (position.x >= -(CCoordinate.LogicSceneLength / 2 + border_size)
                && position.x <= (CCoordinate.LogicSceneLength / 2 + border_size)
                && position.y >= -(CCoordinate.LogicSceneWidth / 2 + border_size)
                && position.y <= CCoordinate.LogicSceneWidth / 2 + border_size);
        }

        //---------------------------------------------------------------------
        void _updateState()
        {
            mIsInScene = _isInScene(mPosition, mSceneBoardSize);
            if (!mEnteredScene) { mEnteredScene = mIsInScene; }
        }
    }
}

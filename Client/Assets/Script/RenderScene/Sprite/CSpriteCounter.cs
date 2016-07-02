using System;
using System.Collections.Generic;
using GF.Common;
using UnityEngine;
using Ps;

public class CSpriteCounter
{
    //-------------------------------------------------------------------------
    struct _tCounterScruct
    {
        public EbVector3 OriginPosition;
        public float UpAngle;
        public float RightAngle;
        public float MovingSpeed;
        public float GapBetweenHeaps;
        public EbVector3 RightDistance;
    }

    //-------------------------------------------------------------------------
    public struct _tScoreCountScruct
    {
        public int Number;
        public int Score;
    }

    enum _eCounterState
    {
        None,
        Adding,
        Moving
    }

    //-------------------------------------------------------------------------
    CRenderScene mScene = null;
    _eCounterState mCounterState = _eCounterState.None;
    _tCounterScruct mCounterScruct;
    Stack<_tScoreCountScruct> mScoreStack = new Stack<_tScoreCountScruct>();
    CSpriteHeapGroup mCSpriteHeapGroup = null;
    MassEntity mMassEntity = null;
    ScoreCounterMap mScoreCounterMap = new ScoreCounterMap();
    EbVector3 mOldPositon;

    //-------------------------------------------------------------------------
    public CSpriteCounter(CRenderScene scene, EbVector3 origin_position, float up_angle)
    {
        mScene = scene;

        mCounterScruct.MovingSpeed = 100f;
        mCounterScruct.OriginPosition = origin_position;
        mCounterScruct.UpAngle = up_angle;
        mCounterScruct.GapBetweenHeaps = mScene.getRenderConfigure().ChipHSpace;
        mCounterScruct.RightAngle = CLogicUtility.getRightAngle(up_angle);
        mCounterScruct.RightDistance = CLogicUtility.getDirection(mCounterScruct.RightAngle) * mCounterScruct.GapBetweenHeaps;
        mCounterState = _eCounterState.Adding;
        mCSpriteHeapGroup = new CSpriteHeapGroup(4);

        mScoreCounterMap.create(mScene.getRenderConfigure().ScoreGap);
    }

    //-------------------------------------------------------------------------
    void initMove(int move_step)
    {
        if (move_step <= 0) return;
        mMassEntity = new MassEntity();
        mMassEntity.setSpeed(mCounterScruct.MovingSpeed);
        mMassEntity.setRoute(RouteHelper.buildLineRoute(EbVector3.Zero, mCounterScruct.RightDistance * move_step));
        mCounterState = _eCounterState.Moving;
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        switch (mCounterState)
        {
            case _eCounterState.None:
                break;
            case _eCounterState.Adding:
                _addHeap();
                updateHeapState(elapsed_tm);
                break;
            case _eCounterState.Moving:
                updateHeapState(elapsed_tm);
                moveHeaps(elapsed_tm);
                break;
        }
    }

    //-------------------------------------------------------------------------
    void _addHeap()
    {
        if (!canAddHeap()) return;
        mCSpriteHeapGroup.add(mCounterScruct.OriginPosition, mCounterScruct.RightDistance, mScene, mScoreStack.Pop(), mCounterScruct.UpAngle);
    }

    //-------------------------------------------------------------------------
    bool canAddHeap()
    {
        return mScoreStack.Count > 0;
    }

    //-------------------------------------------------------------------------
    void updateHeapState(float elapsed_tm)
    {
        if (mCSpriteHeapGroup.update(elapsed_tm))
        {
            initMove(1);
        }
    }

    //-------------------------------------------------------------------------
    void moveHeaps(float elapsed_tm)
    {
        if (mMassEntity == null) return;

        mOldPositon = mMassEntity.Position;
        mMassEntity.update(elapsed_tm);

        mCSpriteHeapGroup.translate(mMassEntity.Position - mOldPositon);

        if (mMassEntity.IsEndRoute)
        {
            mCSpriteHeapGroup.swapHeaps();
            mCounterState = _eCounterState.Adding;
            mMassEntity = null;
        }
    }

    //-------------------------------------------------------------------------
    public void addChip(int score)
    {
        _tScoreCountScruct score_count;
        score_count.Score = score;
        score_count.Number = score2numberOfChip(score);
        mScoreStack.Push(score_count);
    }

    //-------------------------------------------------------------------------
    int score2numberOfChip(int score)
    {
        return mScoreCounterMap.getNumberByScore(score);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mCSpriteHeapGroup.destroy();
    }
}
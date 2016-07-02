using System;
using System.Collections.Generic;
using GF.Common;
using Ps;

public class CSpriteHeapGroup
{
    //-------------------------------------------------------------------------
    CSpriteHeap[] mCSpriteHeaps = null;
    CSpriteBgNumber.BgColorEnum mBgColor = CSpriteBgNumber.BgColorEnum.Red;

    //-------------------------------------------------------------------------
    public CSpriteHeapGroup(int count)
    {
        mCSpriteHeaps = new CSpriteHeap[count];
    }

    public void add(EbVector3 origin, EbVector3 gap, CRenderScene scene, CSpriteCounter._tScoreCountScruct score_count, float up_angle)
    {
        int index_null = mCSpriteHeaps.Length;

        for (int i = mCSpriteHeaps.Length - 1; i >= 0; --i)
        {
            if (mCSpriteHeaps[i] == null)
            {
                index_null = i;
            }
            else
            {
                break;
            }
        }

        if (index_null >= mCSpriteHeaps.Length)
        {
            return;
        }

        EbVector3 position = origin;

        position -= gap * index_null;

        mCSpriteHeaps[index_null] = new CSpriteHeap(scene, score_count.Number, score_count.Score, position, up_angle, getColor());
    }

    CSpriteBgNumber.BgColorEnum getColor()
    {
        CSpriteBgNumber.BgColorEnum color = mBgColor;
        if (mBgColor == CSpriteBgNumber.BgColorEnum.Red)
        {
            mBgColor = CSpriteBgNumber.BgColorEnum.Green;
        }
        else
        {
            mBgColor = CSpriteBgNumber.BgColorEnum.Red;
        }
        return color;
    }

    //-------------------------------------------------------------------------
    public bool update(float elapsed_tm)
    {
        for (int i = mCSpriteHeaps.Length - 1; i >= 0; --i)
        {
            if (mCSpriteHeaps[i] == null) continue;
            mCSpriteHeaps[i].update(elapsed_tm);
        }

        if (mCSpriteHeaps[0] == null) return false;

        if (mCSpriteHeaps[0].EndOfLife)
        {
            mCSpriteHeaps[0].destroy();
            mCSpriteHeaps[0] = null;
            return true;
        }
        return false;
    }

    //-------------------------------------------------------------------------
    public void translate(EbVector3 position)
    {
        foreach (var it in mCSpriteHeaps)
        {
            if (it == null) continue;
            it.translate(position);
        }
    }

    public void swapHeaps()
    {
        if (mCSpriteHeaps[0] != null)
        {
            mCSpriteHeaps[0].destroy();
            mCSpriteHeaps[0] = null;
        }

        for (int i = 1; i < mCSpriteHeaps.Length; ++i)
        {
            mCSpriteHeaps[i - 1] = mCSpriteHeaps[i];
        }

        mCSpriteHeaps[mCSpriteHeaps.Length - 1] = null;
    }

    public void destroy()
    {
        foreach (var it in mCSpriteHeaps)
        {
            if (it == null) continue;
            it.destroy();
        }
    }
}
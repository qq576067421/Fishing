using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GF.Common;
using Ps;

public class CSpriteNumber
{
    //-------------------------------------------------------------------------
    public enum _eNumberSize
    {
        Nomal,
        Small1,
        Small2,
        Small3,
        Big1,
        Big2,
        Big3,
        BaseGap
    }

    //-------------------------------------------------------------------------
    public enum _eNumberAlign
    {
        Left,
        Center,
        Right
    }

    //-------------------------------------------------------------------------
    struct _tNumberInfo
    {
        public EbVector3 Position;
        public float UpAngle;
        public float DigitGap;
        public float DigitScale;
        public int DigitCount;
        public string Tag;
        public float Layer;
        public _eNumberAlign Align;
    }

    //-----------------------------------------------------------------------------
    CRenderScene mScene;
    CSpriteNumberFactory mFactory = null;
    List<StillSprite> mListDigit = new List<StillSprite>();
    _tNumberInfo mNumberInfo;

    //-------------------------------------------------------------------------
    public CSpriteNumber(CRenderScene render_scene, CSpriteNumberFactory factory)
    {
        mScene = render_scene;
        mFactory = factory;
    }

    //-------------------------------------------------------------------------
    public void create(int number, EbVector3 postion, float up_angle, _eNumberSize size, _eNumberAlign align = _eNumberAlign.Center)
    {
        mNumberInfo.Position = postion;
        mNumberInfo.UpAngle = up_angle;
        mNumberInfo.DigitGap = _calculateDigitGap(size);
        mNumberInfo.Align = align;
        mNumberInfo.DigitScale = mFactory.getScale(size);
        mNumberInfo.DigitCount = _calculateDigitCount(number);
        mNumberInfo.Tag = "Untagged";

        setNumber(number);
    }

    //-------------------------------------------------------------------------
    public void create(int number, float number_picture_length, EbVector3 postion, float up_angle, _eNumberAlign align = _eNumberAlign.Center)
    {
        mNumberInfo.DigitCount = _calculateDigitCount(number);

        float digit_width = number_picture_length / mNumberInfo.DigitCount;
        float picture_width = mFactory.getScale(_eNumberSize.BaseGap) / mFactory.getScale(_eNumberSize.Nomal);
        float scale = digit_width / picture_width;

        mNumberInfo.Position = postion;
        mNumberInfo.UpAngle = up_angle;
        mNumberInfo.DigitGap = _calculateDigitGap(scale);
        mNumberInfo.Align = align;
        mNumberInfo.DigitScale = scale;
        mNumberInfo.Tag = "Untagged";

        setNumber(number);
    }

    //-------------------------------------------------------------------------
    public void setNumber(int number)
    {
        _destroyNumber();
        _newNumber(number);
        setLayer(mNumberInfo.Layer);
        setFontSize();
        setTag(mNumberInfo.Tag);
        _updateDigitsLayout();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        _destroyNumber();
    }

    //-------------------------------------------------------------------------
    void _destroyNumber()
    {
        foreach (var it in mListDigit)
        {
            mFactory.freeDigitSprite(it);
        }
        mListDigit.Clear();
    }

    //-------------------------------------------------------------------------
    void _newNumber(int number)
    {
        mNumberInfo.DigitCount = _calculateDigitCount(number);
        int new_number = number;
        int digit = 0;
        if (number > 0)
        {
            while (new_number != 0)
            {
                digit = new_number % 10;
                new_number = (new_number - digit) / 10;
                mListDigit.Add(mFactory.newDigitSprite(digit));
            }
        }
        else
        {
            mNumberInfo.DigitCount = 1;
            mListDigit.Add(mFactory.newDigitSprite(0));
        }
    }

    //-------------------------------------------------------------------------
    void setFontSize()
    {
        foreach (var it in mListDigit)
        {
            it.setScale(mNumberInfo.DigitScale);
        }
    }

    //-------------------------------------------------------------------------
    public void setPosition(EbVector3 position, float angle)
    {
        mNumberInfo.Position = position;
        mNumberInfo.UpAngle = angle;
        _updateDigitsLayout();
    }

    //-------------------------------------------------------------------------
    public void setLayer(float layer)
    {
        mNumberInfo.Layer = layer;
        foreach (var it in mListDigit)
        {
            it.setLayer(mNumberInfo.Layer);
        }
    }

    //-------------------------------------------------------------------------
    public void setColor(UnityEngine.Color color)
    {
        foreach (var it in mListDigit)
        {
            it.setColor(color);
        }
    }

    //-------------------------------------------------------------------------
    public void setAlpha(float alpha)
    {
        foreach (var it in mListDigit)
        {
            it.setAlpha(alpha);
        }
    }

    //-------------------------------------------------------------------------
    public void setTag(string tag)
    {
        mNumberInfo.Tag = tag;
        foreach (var it in mListDigit)
        {
            it.setTag(mNumberInfo.Tag);
        }
    }

    //-------------------------------------------------------------------------
    public void setTrigger(bool isTrigger, float size = 1)
    {
        foreach (var it in mListDigit)
        {
            it.setTrigger(isTrigger, size);
        }
    }

    //-------------------------------------------------------------------------
    bool _isOdd(int number)
    {
        return number % 2 == 1;
    }

    //-------------------------------------------------------------------------
    void _updateDigitsLayout()
    {
        float offset = 0f;

        switch (mNumberInfo.Align)
        {
            case _eNumberAlign.Left:
                offset = 0.5f;
                break;
            case _eNumberAlign.Right:
                offset = mNumberInfo.DigitCount + 0.5f;
                break;
            default:
                if (_isOdd(mNumberInfo.DigitCount))
                {
                    offset = (int)(((float)mNumberInfo.DigitCount - 1f) / 2f);
                }
                else
                {
                    offset = mNumberInfo.DigitCount / 2 - 0.5f;
                }
                break;
        }

        for (int i = mNumberInfo.DigitCount - 1; i >= 0; i--)
        {
            mListDigit[i].setPosition(mNumberInfo.Position + CLogicUtility.getDirection(mNumberInfo.UpAngle + 90) * (mNumberInfo.DigitCount - 1 - i - offset) * mNumberInfo.DigitGap);
            mListDigit[i].setDirection(mNumberInfo.UpAngle);
        }
    }

    //-------------------------------------------------------------------------
    float _calculateDigitGap(_eNumberSize size)
    {
        return _calculateDigitGap(mFactory.getScale(size));
    }

    //-------------------------------------------------------------------------
    float _calculateDigitGap(float digit_scale)
    {
        return mFactory.getScale(_eNumberSize.BaseGap) * digit_scale / mFactory.getScale(_eNumberSize.Nomal);
    }

    //-------------------------------------------------------------------------
    int _calculateDigitCount(int number)
    {
        return number.ToString().Length;
    }
}
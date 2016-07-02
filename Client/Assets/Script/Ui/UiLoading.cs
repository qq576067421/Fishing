using UnityEngine;
using System;
using System.Collections;
using FairyGUI;

public class UiLoading : UiBase
{
    //-------------------------------------------------------------------------
    public bool IsFinished { get; private set; }
    public Action OnFinished { get; set; }

    //-------------------------------------------------------------------------
    public override void create()
    {
        OnFinished = null;
        IsFinished = false;

        Timers.inst.Add(0.01f, 0, _playProgress);
    }

    //-------------------------------------------------------------------------
    public override void destroy()
    {
        Timers.inst.Remove(_playProgress);
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
    }

    //-------------------------------------------------------------------------
    public void setTip(string tip)
    {

    }

    //-------------------------------------------------------------------------
    void _playProgress(object param)
    {
        GProgressBar obj = ComUi.GetChild("ProgressBar") as GProgressBar;
        if (obj != null)
        {
            if (obj.value <= obj.max)
            {
                obj.value += 2;
            }
            else
            {
                Timers.inst.Remove(_playProgress);

                IsFinished = true;
                if (OnFinished != null)
                {
                    OnFinished();
                }
            }
        }
    }
}

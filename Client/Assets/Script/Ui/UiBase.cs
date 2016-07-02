using UnityEngine;
using System.Collections;
using FairyGUI;
using GF.Common;

public abstract class UiBase
{
    //-------------------------------------------------------------------------
    public UiMgr UiMgr { get; set; }
    public GameObject GoUi { get; set; }
    public GComponent ComUi { get; set; }
    public EntityMgr EntityMgr { get { return UiMgr.EntityMgr; } }

    //-------------------------------------------------------------------------
    public abstract void create();

    //-------------------------------------------------------------------------
    public abstract void destroy();

    //-------------------------------------------------------------------------
    public abstract void update(float elapsed_tm);

    //-------------------------------------------------------------------------
    public T genEvent<T>() where T : EntityEvent, new()
    {
        return UiMgr.genEvent<T>();
    }
}

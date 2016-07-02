using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using GF.Common;

public class UiMgr
{
    //-------------------------------------------------------------------------
    public EntityMgr EntityMgr { get; private set; }
    static public UiMgr Instance { get { return mUiMgr; } }

    //-------------------------------------------------------------------------
    static UiMgr mUiMgr;
    Dictionary<string, List<UiBase>> MapAllUi = new Dictionary<string, List<UiBase>>();

    //-------------------------------------------------------------------------
    public void create(EntityMgr entity_mgr)
    {
        mUiMgr = this;
        EntityMgr = entity_mgr;

        UIConfig.defaultFont = "Microsoft YaHei";

        UIPackage.AddPackage("Ui/Common");
        UIPackage.AddPackage("Ui/Main");
        UIPackage.AddPackage("Ui/Loading");
        UIPackage.AddPackage("Ui/Login");

        UIConfig.buttonSound = (AudioClip)UIPackage.GetItemAsset("Common", "Close");

        //UIConfig.verticalScrollBar = UIPackage.GetItemURL("Common", "ScrollBar_VT");
        //UIConfig.horizontalScrollBar = UIPackage.GetItemURL("Basics", "ScrollBar_HZ");
        //UIConfig.popupMenu = UIPackage.GetItemURL("Basics", "PopupMenu");
        //GRoot.inst.SetContentScaleFactor(1136, 640);

        Stage.inst.onKeyDown.Add(_onKeyDown);
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        foreach (var i in MapAllUi)
        {
            if (i.Value == null) continue;
            foreach (var j in i.Value)
            {
                j.update(elapsed_tm);
            }
        }
    }

    //-------------------------------------------------------------------------
    public T createUi<T>(string package_name, string component_name,
        FitScreen fit_screen = FitScreen.FitSize) where T : UiBase, new()
    {
        var go = new GameObject();
        go.name = component_name;
        go.layer = 5;// "UI"

        var ui_panel = go.AddComponent<UIPanel>();
        ui_panel.packageName = package_name;
        ui_panel.componentName = component_name;
        ui_panel.sortingOrder = 1;
        ui_panel.fitScreen = fit_screen;
        ui_panel.ApplyModifiedProperties(true, true);

        var ui_contentscale = go.AddComponent<UIContentScaler>();
        ui_contentscale.designResolutionX = 1280;
        ui_contentscale.designResolutionY = 800;
        ui_contentscale.screenMatchMode = UIContentScaler.ScreenMatchMode.MatchWidthOrHeight;
        ui_contentscale.scaleMode = UIContentScaler.ScaleMode.ScaleWithScreenSize;
        ui_contentscale.ApplyChange();

        var com = ui_panel.ui;

        T ui = new T();
        ui.UiMgr = this;
        ui.GoUi = go;
        ui.ComUi = com;

        ui.create();

        string ui_name = typeof(T).Name;
        List<UiBase> list_ui = null;
        MapAllUi.TryGetValue(ui_name, out list_ui);
        if (list_ui == null)
        {
            list_ui = new List<UiBase>();
            MapAllUi[ui_name] = list_ui;
        }
        list_ui.Add(ui);

        return ui;
    }

    //-------------------------------------------------------------------------
    public void destroyFirstUi<T>() where T : UiBase
    {
        string ui_name = typeof(T).Name;
        List<UiBase> list_ui = null;
        MapAllUi.TryGetValue(ui_name, out list_ui);
        if (list_ui != null && list_ui.Count > 0)
        {
            T ui = (T)list_ui[0];
            ui.destroy();
            GameObject.Destroy(ui.GoUi);
            list_ui.Remove(ui);
        }
    }

    //-------------------------------------------------------------------------
    public T getFirstUi<T>() where T : UiBase
    {
        string ui_name = typeof(T).Name;
        List<UiBase> list_ui = null;
        MapAllUi.TryGetValue(ui_name, out list_ui);
        if (list_ui != null && list_ui.Count > 0)
        {
            T ui = (T)list_ui[0];
            return ui;
        }

        return default(T);
    }

    //---------------------------------------------------------------------
    public T genEvent<T>() where T : EntityEvent, new()
    {
        var publisher = EntityMgr.getDefaultEventPublisher();
        return publisher.genEvent<T>();
    }

    //-------------------------------------------------------------------------
    void _onKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}

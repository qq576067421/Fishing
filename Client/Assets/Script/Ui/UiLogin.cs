using UnityEngine;
using System;
using System.Collections;
using FairyGUI;

public class UiLogin : UiBase
{
    //-------------------------------------------------------------------------
    public bool IsFinished { get; private set; }
    public Action OnFinished { get; set; }

    //-------------------------------------------------------------------------
    public override void create()
    {
        var dlg_login = ComUi.GetChild("DlgLogin").asCom;
        var btn_login = dlg_login.GetChild("BtnLogin").asButton;
        btn_login.onClick.Add(_onClickBtnLogin);
    }

    //-------------------------------------------------------------------------
    public override void destroy()
    {
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
    }

    //-------------------------------------------------------------------------
    void _onClickBtnLogin()
    {
        var ev = genEvent<EvUiLoginClickBtnLogin>();
        ev.acc = "test1000";
        ev.pwd = "123456";
        ev.send(null);
    }
}

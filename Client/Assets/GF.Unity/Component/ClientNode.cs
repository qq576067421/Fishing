using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class ClientNode<TDef> : Component<TDef> where TDef : DefNode, new()
{
    //-------------------------------------------------------------------------
    public ClientSuperSocket<DefSuperSocket> CoSuperSocket { get; private set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        EbLog.Note("ClientNode.init()");

        var settings = EcEngine.Instance.Settings;

        if (settings.EnableCoSuperSocket)
        {
            var et = EntityMgr.createEntity<EtSuperSocket>(null, Entity);
            CoSuperSocket = et.getComponent<ClientSuperSocket<DefSuperSocket>>();
        }
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        EbLog.Note("ClientNode.release()");
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }
}

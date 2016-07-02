using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public class ClientSuperSocket<TDef> : Component<TDef> where TDef : DefSuperSocket, new()
{
    //-------------------------------------------------------------------------
    public override void init()
    {
        EbLog.Note("ClientSuperSocket.init()");
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        EbLog.Note("ClientSuperSocket.release()");
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

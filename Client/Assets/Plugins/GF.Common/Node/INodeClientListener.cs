using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public interface INodeClientListener
{
    //-------------------------------------------------------------------------
    void nodeClient2ServerSendMsg(CNode node, int msg_type, int msg_id, int msg_param, List<object> msg_paramlist);

    //-------------------------------------------------------------------------
    void nodeSelectExit(CNode node, bool all_exit, int exit_id);
}
using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public interface INodeServerListener
{
    //-------------------------------------------------------------------------
    void nodeSendMsg(int msg_id, List<object> list_param);

    //-------------------------------------------------------------------------
    void nodeServer2ClientSendMsg(CNode node, int msg_type, int msg_id, int msg_param, List<object> msg_paramlist);

    //-------------------------------------------------------------------------
    void nodeSaveNodeInfoList(List<_tNodeInfo> list_nodeinfo);

    //-------------------------------------------------------------------------
    void nodeUpdateNodeOpQue(Queue<_tNodeOp> que_nodeop);
}
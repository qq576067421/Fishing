using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

public interface INodeTrigger
{
    //-------------------------------------------------------------------------
    int Id { get; set; }
    int Param { get; set; }

    //-------------------------------------------------------------------------
    void setup(CNode node, int linked_from, int param);

    //-------------------------------------------------------------------------
    void reset();

    //-------------------------------------------------------------------------
    void update(float elasped_tm);

    //-------------------------------------------------------------------------
    bool handleMsg(int msg_id, List<object> vec_param);

    //-------------------------------------------------------------------------
    bool triggered();

    //-------------------------------------------------------------------------
    bool hasMsg(_tMsgInfo msg_info);
}

public interface INodeTriggerFactory
{
    //-------------------------------------------------------------------------
    _tMsgInfo getMsgInfo();

    //-------------------------------------------------------------------------
    int getId();

    //-------------------------------------------------------------------------
    INodeTrigger createTrigger(CNode node, int linked_from, int param);
}

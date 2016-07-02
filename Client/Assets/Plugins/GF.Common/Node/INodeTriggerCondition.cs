using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

public interface INodeTriggerCondition
{
    //-------------------------------------------------------------------------
    string getId();

    //-------------------------------------------------------------------------
    void setEntity(CNode node);

    //-------------------------------------------------------------------------
    bool excute(Group xml_group);
}

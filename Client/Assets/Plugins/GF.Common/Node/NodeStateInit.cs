using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CNodeStateInit : EbState
{
    //-------------------------------------------------------------------------
    CNode mNode;

    //-------------------------------------------------------------------------
    public CNodeStateInit(CNode node)
    {
        mNode = node;

        _defState("CNodeStateInit", "EbFsm", 0, true);

        _bindAction("evSetNextState", new EbAction(this.evSetNextSate));
    }

    //-------------------------------------------------------------------------
    public override void enter()
    {
        mNode.postEvent("evSetNextState");
    }

    //-------------------------------------------------------------------------
    public override void exit()
    {
    }

    //-------------------------------------------------------------------------
    public string evSetNextSate(IEbEvent ev)
    {
        // 根据NodeState跳转到对应state
        _eNodeState state = mNode.getNodeState();

        if (state == _eNodeState.Init || state == _eNodeState.Start)
        {
            return "CNodeStateStart";
        }
        else if (state == _eNodeState.Run)
        {
            return "CNodeStateRun";
        }
        else if (state == _eNodeState.Stop)
        {
            return "CNodeStateStop";
        }
        else if (state == _eNodeState.Release)
        {
            return "CNodeStateRelease";
        }

        return "CNodeStateStart";
    }
}
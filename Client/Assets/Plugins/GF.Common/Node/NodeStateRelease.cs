using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CNodeStateRelease : EbState
{
    //-------------------------------------------------------------------------
    CNode mNode;
    INodeServerScript mNodeServerScript;
    INodeClientScript mNodeClientScript;

    //-------------------------------------------------------------------------
    public CNodeStateRelease(CNode node)
    {
        mNode = node;

        _defState("CNodeStateRelease", "EbFsm", 0, false);
    }

    //-------------------------------------------------------------------------
    public override void enter()
    {
        mNodeServerScript = mNode._getNodeServerScript();
        mNodeClientScript = mNode._getNodeClientScript();
        mNode._setNodeState(_eNodeState.Release);

        // 销毁cur node
        int node_id = mNode.getNodeId();
        int next_node_id = mNode._getNextNodeId();
        CNode parent_node = mNode.getParentNode();

        // 生成NodeOp
        if (!mNode.getNodeSys().isClient())
        {
            mNode.getNodeMgr()._opDestroyNode(node_id);
        }

        // 创建NextNode
        if (!mNode.getNodeSys().isClient())
        {
            bool exit_all = mNode._isSelectAllExits();
            if (exit_all)
            {
                var groups = mNode.getDefXml().GetGroupArray("Exit");
                foreach (int i in groups.Keys)
                {
                    Property exit_successor_id = groups[i].GetValue("Successor");
                    if (exit_successor_id != null)
                    {
                        int e_id = int.Parse(exit_successor_id.Value);
                        if (e_id > 0)
                        {
                            mNode.getNodeMgr()._opCreateNode(e_id, _eNodeState.Init, mNode.getNodeId());
                        }
                    }
                }
            }
            else
            {
                if (parent_node != null && next_node_id == parent_node.getNodeId())
                {
                    // 不创建NextNode，通知父Node出口
                    int parent_exit_id = 0;
                    var groups = mNode.getDefXml().GetGroupArray("Exit");
                    int exit_id = mNode.getExitId();
                    foreach (int i in groups.Keys)
                    {
                        if (i == exit_id)
                        {
                            Property prop_parent_exit_id = groups[i].GetValue("ParentExit");
                            parent_exit_id = int.Parse(prop_parent_exit_id.Value);
                            break;
                        }
                    }
                    //EbLog.Note("parent_exit_id=" + parent_exit_id);
                    parent_node._notifyExit(parent_exit_id);
                }
                else if (next_node_id > 0)
                {
                    mNode.getNodeMgr()._opCreateNode(next_node_id, _eNodeState.Init, mNode.getNodeId());
                }
                else
                {
                    EbLog.Note("Not Create Next Node. next_node_id=" + next_node_id);
                }
            }

            if (parent_node != null)
            {
                parent_node._removeChildNode(mNode.getNodeId());
            }
        }
    }

    //-------------------------------------------------------------------------
    public override void exit()
    {
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CNodeStateRun : EbState
{
    //-------------------------------------------------------------------------
    CNode mNode;
    INodeServerScript mNodeServerScript;
    INodeClientScript mNodeClientScript;

    //-------------------------------------------------------------------------
    public CNodeStateRun(CNode node)
    {
        mNode = node;

        _defState("CNodeStateRun", "EbFsm", 0, false);

        _bindAction("main_update", new EbAction(this.evMainUpdate));
        _bindAction("main_sendmsg", new EbAction(this.evMainSendMsg));
        _bindAction("evSetStopState", new EbAction(this.evSetStopState));
    }

    //-------------------------------------------------------------------------
    public override void enter()
    {
        if (mNode.getNodeMgr().EnableLog)
        {
            StringBuilder sb = new StringBuilder(512);
            sb.Append("EtPlayer et_guid=");
            sb.Append(mNode.getNodeMgr().EtPlayer.Guid);
            sb.Append(" NodeType=");
            sb.Append(mNode.getNodeType());
            sb.Append(" NodeId=");
            sb.Append(mNode.getNodeId());
            sb.Append(" Run");
            EbLog.Note(sb.ToString());
        }

        mNodeServerScript = mNode._getNodeServerScript();
        mNodeClientScript = mNode._getNodeClientScript();
        mNode._setNodeState(_eNodeState.Run);

        // 生成NodeOp
        if (!mNode.getNodeSys().isClient())
        {
            mNode.getNodeMgr()._opEnterState(mNode.getNodeId(), mNode.getNodeState());
        }

        // 决策是否需要创建子Node
        {
            CNodeMgr node_mgr = mNode.getNodeMgr();
            int node_id = mNode.getNodeId();
            EventDef node_def = mNode.getDefXml();
            if (mNode.hasChildNodeDef() && mNode._getChildNodeCount() == 0)
            {
                bool ok = false;

                var group = node_def.GetGroup("LinkedToChild");
                if (group != null)
                {
                    ok = true;

                    Property p_entity_def_uid = group.GetValue("UID");
                    int this_child_entity_id = int.Parse(p_entity_def_uid.Value);
                    if (this_child_entity_id != 0)
                    {
                        node_mgr._opCreateNode(this_child_entity_id, _eNodeState.Init, node_id);
                    }
                }

                if (!ok)
                {
                    var groups = node_def.GetGroupArray("LinkedToChild");
                    if (groups != null)
                    {
                        foreach (int i in groups.Keys)
                        {
                            Property p_entity_def_uid = groups[i].GetValue("UID");
                            int this_child_entity_id = int.Parse(p_entity_def_uid.Value);
                            if (this_child_entity_id != 0)
                            {
                                node_mgr._opCreateNode(this_child_entity_id, _eNodeState.Init, node_id);
                            }
                        }
                    }
                }
            }
        }

        // 广播NodeRun消息
        if (!mNode.getNodeSys().isClient())
        {
            List<object> list_param = new List<object>();
            list_param.Add(mNode.getNodeType());
            list_param.Add(mNode.getNodeId());
            mNode._getNodeServerListener().nodeSendMsg((int)_eNodeMsg.NodeEnterRun, list_param);
        }

        // 执行脚本函数
        if (mNodeServerScript != null)
        {
            mNodeServerScript.onEnterRunState(mNode);
        }
        else if (mNodeClientScript != null)
        {
            mNodeClientScript.onEnterRunState(mNode);
        }
    }

    //-------------------------------------------------------------------------
    public override void exit()
    {
    }

    //-------------------------------------------------------------------------
    // event: 每帧更新
    public string evMainUpdate(IEbEvent ev)
    {
        EbEvent1<float> evt = ev as EbEvent1<float>;
        float elapsed_tm = evt.param1;

        // 执行脚本函数
        if (mNodeServerScript != null)
        {
            mNodeServerScript.onUpdate(mNode, elapsed_tm);
        }
        else if (mNodeClientScript != null)
        {
            mNodeClientScript.onUpdate(mNode, elapsed_tm);
        }

        return "";
    }

    //-------------------------------------------------------------------------
    // event: 响应消息
    public string evMainSendMsg(IEbEvent ev)
    {
        EbEvent2<int, List<object>> evt = ev as EbEvent2<int, List<object>>;
        int msg_id = evt.param1;
        List<object> msg_paramlist = evt.param2;

        if (mNodeServerScript != null)
        {
            mNodeServerScript.onServerMsg(mNode, 0, msg_id, 0, msg_paramlist);
        }
        else if (mNodeClientScript != null)
        {
            mNodeClientScript.onClientMsg(msg_id, msg_paramlist);
        }

        return "";
    }

    //-------------------------------------------------------------------------
    // event: 设置进入停止状态
    public string evSetStopState(IEbEvent ev)
    {
        EbEvent1<List<_tNodeParamPair>> evt = ev as EbEvent1<List<_tNodeParamPair>>;
        List<_tNodeParamPair> list_param = evt.param1;

        Dictionary<byte, object> map_param = mNode.getMapParam();
        if (list_param != null)
        {
            foreach (var i in list_param)
            {
                map_param[i.k] = i.v;
            }
        }

        return "CNodeStateStop";
    }
}

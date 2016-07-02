using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CNodeStateStop : EbState
{
    //-------------------------------------------------------------------------
    CNode mNode;
    INodeServerScript mNodeServerScript;
    INodeClientScript mNodeClientScript;
    Dictionary<int, List<Group>> mMapEffectGroup = new Dictionary<int, List<Group>>();

    //-------------------------------------------------------------------------
    public CNodeStateStop(CNode node)
    {
        mNode = node;

        _defState("CNodeStateStop", "EbFsm", 0, false);

        _bindAction("evSetNextState", new EbAction(this.evSetNextSate));
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
            sb.Append(" Stop");
            EbLog.Note(sb.ToString());
        }

        mNodeServerScript = mNode._getNodeServerScript();
        mNodeClientScript = mNode._getNodeClientScript();
        mNode._setNodeState(_eNodeState.Stop);

        // 生成NodeOp
        if (!mNode.getNodeSys().isClient())
        {
            List<_tNodeParamPair> list_param = new List<_tNodeParamPair>();
            {
                _tNodeParamPair pp;
                pp.k = (byte)_eNodeParam.AllExit;
                pp.v = mNode._isSelectAllExits();
                list_param.Add(pp);
            }

            {
                _tNodeParamPair pp;
                pp.k = (byte)_eNodeParam.ExitId;
                pp.v = mNode.getExitId();
                list_param.Add(pp);
            }

            mNode.getNodeMgr()._opEnterState(mNode.getNodeId(), mNode.getNodeState(), list_param);
        }

        // 执行脚本函数
        if (mNodeServerScript != null)
        {
            mNodeServerScript.onEnterStopState(mNode);
        }
        else if (mNodeClientScript != null)
        {
            mNodeClientScript.onEnterStopState(mNode);
        }

        // 广播BeforeEffect消息
        //mpEntity.sendMessage((int)MsgType.Entity, 2, mpEntity.getNodeId(), null);

        // 执行EffectXml中定义的效果
        _parseEffectXml();
        _doEffect();

        // 执行脚本函数
        if (mNodeServerScript != null)
        {
            mNodeServerScript.onDoEffect(mNode, mNode.getExitId());
        }
        else if (mNodeClientScript != null)
        {
            mNodeClientScript.onDoEffect(mNode, mNode.getExitId());
        }

        mNode.postEvent("evSetNextState");
    }

    //-------------------------------------------------------------------------
    public override void exit()
    {
        // 广播NodeStop消息
        if (!mNode.getNodeSys().isClient())
        {
            List<object> list_param = new List<object>();
            list_param.Add(mNode.getNodeType());
            list_param.Add(mNode.getNodeId());
            list_param.Add(mNode.getExitId());
            mNode._getNodeServerListener().nodeSendMsg((int)_eNodeMsg.NodeLeaveStop, list_param);
        }
    }

    //-------------------------------------------------------------------------
    public string evSetNextSate(IEbEvent ev)
    {
        return "CNodeStateRelease";
    }

    //-------------------------------------------------------------------------
    // 解析EffectXml
    private void _parseEffectXml()
    {
        EventDef entity_def = mNode.getDefXml();
        var effect_groups = entity_def.GetGroupArray("Effect");
        if (effect_groups != null)
        {
            foreach (int i in effect_groups.Keys)
            {
                Group grp = effect_groups[i];

                Property p = grp.GetValue("BoundToExit");
                int exit_code = int.Parse(p.Value);

                if (mMapEffectGroup.ContainsKey(exit_code))
                {
                    mMapEffectGroup[exit_code].Add(grp);
                }
                else
                {
                    List<Group> l = new List<Group>();
                    l.Add(grp);
                    mMapEffectGroup[exit_code] = l;
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    // 执行EffectXml中定义的所有效果
    private void _doEffect()
    {
        if (mNode._isSelectAllExits())
        {
            foreach (var effect_grp in mMapEffectGroup)
            {
                List<Group> grp_list = effect_grp.Value;
                foreach (var grp in grp_list)
                {
                    foreach (var g in grp.Groups)
                    {
                        INodeEffect entity_effect = mNode.getNodeSys().getNodeEffect(g.GetName());
                        if (entity_effect != null)
                        {
                            entity_effect.setEntity(mNode);
                            entity_effect.excute(g);
                        }
                    }
                }
            }
        }
        else
        {
            int exit_id = mNode.getExitId();

            if (mMapEffectGroup.ContainsKey(exit_id))
            {
                List<Group> grp_list = mMapEffectGroup[exit_id];
                foreach (var grp in grp_list)
                {
                    foreach (var g in grp.Groups)
                    {
                        INodeEffect entity_effect = mNode.getNodeSys().getNodeEffect(g.GetName());
                        if (entity_effect != null)
                        {
                            entity_effect.setEntity(mNode);
                            entity_effect.excute(g);
                        }
                    }
                }
            }
            else
            {
            }
        }
    }
}
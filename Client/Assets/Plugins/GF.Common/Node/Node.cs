using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using EventDataXML;
using GF.Common;

public class CNode : EbFsm, IDisposable
{
    //-------------------------------------------------------------------------
    int mNodeId = -1;
    _eNodeState mNodeState = _eNodeState.Init;
    Dictionary<byte, object> mMapParam = new Dictionary<byte, object>();
    INodeServerListener mNodeServerListener;
    INodeClientListener mNodeClientListener;
    INodeServerScript mNodeServerScript;
    INodeClientScript mNodeClientScript;
    CNodeSys mNodeSys;
    CNodeMgr mNodeMgr;
    CNode mParentNode;
    EventDef mNodeDef;
    Dictionary<int, CNode> mMapAllChild = new Dictionary<int, CNode>();
    Dictionary<string, object> mMapCacheData = new Dictionary<string, object>();
    List<int> mListExitId = new List<int>();
    List<INodeTrigger> mTriggerList = new List<INodeTrigger>();
    string mNodeType = "";

    //-------------------------------------------------------------------------
    ~CNode()
    {
        this.Dispose(false);
    }

    //-------------------------------------------------------------------------
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    //-------------------------------------------------------------------------
    protected virtual void Dispose(bool disposing)
    {
        destroy();
    }

    //-------------------------------------------------------------------------
    public void create(int node_id, _eNodeState state, List<_tNodeParamPair> list_param,
        INodeServerListener server_listener, INodeClientListener client_listener, CNodeSys node_sys, CNodeMgr node_mgr)
    {
        mNodeServerListener = server_listener;
        mNodeClientListener = client_listener;
        mNodeSys = node_sys;
        mNodeMgr = node_mgr;

        // 初始化NodeInfo
        mNodeId = node_id;
        mNodeState = state;
        if (list_param != null)
        {
            foreach (var i in list_param)
            {
                if (i.k == (byte)_eNodeParam.PreNodeId || i.k == (byte)_eNodeParam.ExitId)
                {
                    mMapParam[i.k] = int.Parse(i.v.ToString());
                }
                else
                {
                    mMapParam[i.k] = i.v;
                }
            }
        }

        // 加载DefXml数据
        _loadDefXml();

        // 设置该Node的父Node
        string str_parent_node_id = getDefXml().GetValue("Parent").Value;
        if (str_parent_node_id != string.Empty && str_parent_node_id != "0")
        {
            int parent_node_id = Convert.ToInt32(str_parent_node_id);
            CNode node_parent = getNodeMgr().findNode(parent_node_id);
            if (node_parent != null)
            {
                mParentNode = node_parent;
                node_parent._addChildNode(this);
            }
        }

        // 从xml中解析所有trigger
        _parseTriggerXml();

        // 初始化脚本
        if (mNodeSys.isClient())
        {
            INodeClientScriptFactory factory = mNodeSys.getNodeClientScriptFactory(getNodeType());
            if (factory != null)
            {
                mNodeClientScript = factory.createScript(this);
            }
        }
        else
        {
            INodeServerScriptFactory factory = mNodeSys.getNodeServerScriptFactory(getNodeType());
            if (factory != null)
            {
                mNodeServerScript = factory.createScript(this, getEtPlayer());
            }
        }

        // 创建Fsm
        addState(new CNodeStateInit(this));
        addState(new CNodeStateStart(this));
        addState(new CNodeStateRun(this));
        addState(new CNodeStateStop(this));
        addState(new CNodeStateRelease(this));
        setupFsm();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        // 销毁Fsm
        destroyFsm();
    }

    //-------------------------------------------------------------------------
    // Client专用接口
    public void clientSelectExit(bool all_exit, int exit_id)
    {
        if (mNodeClientListener != null)
        {
            mNodeClientListener.nodeSelectExit(this, all_exit, exit_id);
        }
    }

    //-------------------------------------------------------------------------
    // Server专用接口，选择出口退出
    public void serverSelectExit(int exit_id)
    {
        if (!mNodeSys.hasChildNodeDef(mNodeId))
        {
            mMapParam[(byte)_eNodeParam.AllExit] = false;
            mMapParam[(byte)_eNodeParam.ExitId] = exit_id;

            List<_tNodeParamPair> list_param = new List<_tNodeParamPair>();
            postEvent("evSetStopState", list_param);
        }
    }

    //-------------------------------------------------------------------------
    // Server专用接口，选择所有出口退出（只能用于父Node）
    public void serverSelectAllExits()
    {
        if (!mNodeSys.hasChildNodeDef(mNodeId))
        {
            mMapParam[(byte)_eNodeParam.AllExit] = true;
            mMapParam[(byte)_eNodeParam.ExitId] = (int)0;

            List<_tNodeParamPair> list_param = new List<_tNodeParamPair>();
            postEvent("evSetStopState", list_param);
        }
    }

    //-------------------------------------------------------------------------
    // 获取出口id
    public int getExitId()
    {
        bool all_exit = false;
        int exit_id = -1;

        if (mMapParam.ContainsKey((byte)_eNodeParam.AllExit))
        {
            all_exit = Convert.ToBoolean(mMapParam[(byte)_eNodeParam.AllExit]);
        }

        if (mMapParam.ContainsKey((byte)_eNodeParam.ExitId))
        {
            exit_id = Convert.ToInt32(mMapParam[(byte)_eNodeParam.ExitId]);
        }

        if (all_exit) return 999;
        else return exit_id;
    }

    //-------------------------------------------------------------------------
    public Dictionary<byte, object> getMapParam()
    {
        return mMapParam;
    }

    //-------------------------------------------------------------------------
    public CNodeMgr getNodeMgr()
    {
        return mNodeMgr;
    }

    //-------------------------------------------------------------------------
    public CNodeSys getNodeSys()
    {
        return mNodeSys;
    }

    //-------------------------------------------------------------------------
    public string getNodeType()
    {
        return mNodeType;
    }

    //-------------------------------------------------------------------------
    // 获取node_id
    public int getNodeId()
    {
        return mNodeId;
    }

    //-------------------------------------------------------------------------
    // 查询Node状态
    public _eNodeState getNodeState()
    {
        return mNodeState;
    }

    //-------------------------------------------------------------------------
    // 切换Node状态
    public void setNodeSate(_tNodeOp node_op)
    {
        _eNodeState next_state = (_eNodeState)node_op.state;
        if (next_state == _eNodeState.Run)
        {
            processEvent("evSetRunState", node_op.list_param);
        }
        else if (next_state == _eNodeState.Stop)
        {
            processEvent("evSetStopState", node_op.list_param);
        }
    }

    //-------------------------------------------------------------------------
    // 获取NodeDef
    public EventDef getDefXml()
    {
        return mNodeDef;
    }

    //-------------------------------------------------------------------------
    public void server2clientSendMsg(int msg_type, int msg_id, int msg_param, List<object> msg_paramlist)
    {
        mNodeServerListener.nodeServer2ClientSendMsg(this, msg_type, msg_id, msg_param, msg_paramlist);
    }

    //-------------------------------------------------------------------------
    public Entity getEtPlayer()
    {
        return mNodeMgr.getEtPlayer();
    }

    //-------------------------------------------------------------------------
    // 获取缓存数据
    public object getCacheData(string key)
    {
        if (mMapCacheData.ContainsKey(key))
        {
            return mMapCacheData[key];
        }
        else return null;
    }

    //-------------------------------------------------------------------------
    // 设置缓存数据
    public void setCacheData(string key, object value)
    {
        mMapCacheData[key] = value;
    }

    //-------------------------------------------------------------------------
    // 查询缓存数据
    public bool hasCacheData(string key)
    {
        return mMapCacheData.ContainsKey(key);
    }

    //-------------------------------------------------------------------------
    // 清空缓存数据
    public void clearCacheData()
    {
        mMapCacheData.Clear();
    }

    //-------------------------------------------------------------------------
    public int getDefXmlCount(string key)
    {
        var groups = mNodeDef.GetGroupArray(key);
        if (groups != null)
        {
            return groups.Count;
        }
        return 0;
    }

    //-------------------------------------------------------------------------
    public string getDefXmlValue(string key)
    {
        Property pEvtType = mNodeDef.GetValue(key);
        if (pEvtType != null)
        {
            return pEvtType.Value;
        }
        return "";
    }

    //-------------------------------------------------------------------------
    public string getDefXmlValue(int entity_id, string key)
    {
        EventDef entity_def = mNodeSys.getNodeDef(entity_id);
        Property evt_type = entity_def.GetValue(key);
        if (evt_type != null)
        {
            return evt_type.Value;
        }
        return "";
    }

    //-------------------------------------------------------------------------
    public int getDefXmlCount(int entity_id, string key)
    {
        EventDef entity_def = mNodeSys.getNodeDef(entity_id);
        var groups = entity_def.GetGroupArray(key);
        if (groups != null)
        {
            return groups.Count;
        }

        return 0;
    }

    //-------------------------------------------------------------------------
    // 是否包含子NodeDef
    public bool hasChildNodeDef()
    {
        return mNodeSys.hasChildNodeDef(getNodeId());
    }

    //-------------------------------------------------------------------------
    public List<INodeTrigger> getTriggerList()
    {
        return mTriggerList;
    }

    //-------------------------------------------------------------------------
    // 获取出口对应的Next Node Id
    public int _getNextNodeId()
    {
        var groups = mNodeDef.GetGroupArray("Exit");
        if (groups == null) return 0;

        int exit_id = getExitId();
        foreach (int i in groups.Keys)
        {
            if (i == exit_id)
            {
                Property exit_successor_id = groups[i].GetValue("Successor");
                if (exit_successor_id != null && exit_successor_id.Value != "0")
                {
                    return int.Parse(exit_successor_id.Value);
                }
                else if (mParentNode != null)
                {
                    return mParentNode.getNodeId();
                }
            }
        }

        return 0;
    }

    //-------------------------------------------------------------------------
    // 获取子Node数量
    public int _getChildNodeCount()
    {
        return mMapAllChild.Count;
    }

    //-------------------------------------------------------------------------
    public Dictionary<int, CNode> _getAllChildNode()
    {
        return mMapAllChild;
    }

    //-------------------------------------------------------------------------
    // 移除子Node
    public void _removeChildNode(int node_id)
    {
        mMapAllChild.Remove(node_id);
    }

    //-------------------------------------------------------------------------
    // 添加子Node
    public void _addChildNode(CNode node)
    {
        if (!mMapAllChild.ContainsKey(node.getNodeId()))
        {
            mMapAllChild.Add(node.getNodeId(), node);
        }
    }

    //-------------------------------------------------------------------------
    public CNode getParentNode()
    {
        return mParentNode;
    }

    //-------------------------------------------------------------------------
    public bool _isSelectAllExits()
    {
        bool all_exit = false;

        if (mMapParam.ContainsKey((byte)_eNodeParam.AllExit))
        {
            all_exit = (bool)mMapParam[(byte)_eNodeParam.AllExit];
        }

        return all_exit;
    }

    //-------------------------------------------------------------------------
    // 子Node通知父Node退出
    internal void _notifyExit(int exit_id)
    {
        if (mNodeSys.hasChildNodeDef(mNodeId))
        {
            mMapParam[(byte)_eNodeParam.AllExit] = false;
            mMapParam[(byte)_eNodeParam.ExitId] = exit_id;

            List<_tNodeParamPair> list_param = new List<_tNodeParamPair>();
            postEvent("evSetStopState", list_param);
        }
    }

    //-------------------------------------------------------------------------
    // 获取_tNodeInfo
    internal _tNodeInfo _getNodeInfo()
    {
        _tNodeInfo node_info = new _tNodeInfo();
        node_info.id = mNodeId;
        node_info.state = (byte)mNodeState;
        node_info.list_param = new List<_tNodeParamPair>();
        foreach (var k in mMapParam)
        {
            _tNodeParamPair pp;
            pp.k = k.Key;
            pp.v = k.Value;
            node_info.list_param.Add(pp);
        }

        if (mMapAllChild.Count > 0)
        {
            node_info.list_child = new List<_tNodeInfo>();

            foreach (var i in mMapAllChild)
            {
                node_info.list_child.Add(i.Value._getNodeInfo());
            }
        }
        else
        {
            node_info.list_child = null;
        }

        return node_info;
    }

    //-------------------------------------------------------------------------
    internal void _recvMsgFromRemote(int msg_type, int msg_id, int msg_param, List<object> msg_paramlist)
    {
        if (!mNodeMgr.isClient())
        {
            if (mNodeServerScript != null)
            {
                mNodeServerScript.onClient2ServerMsg(this, msg_type, msg_id, msg_param, msg_paramlist);
            }
        }
        else
        {
            if (mNodeClientScript != null)
            {
                mNodeClientScript.onServer2ClientMsg(this, msg_type, msg_id, msg_param, msg_paramlist);
            }
        }
    }

    //-------------------------------------------------------------------------
    internal INodeServerListener _getNodeServerListener()
    {
        return mNodeServerListener;
    }

    //-------------------------------------------------------------------------
    internal INodeServerScript _getNodeServerScript()
    {
        return mNodeServerScript;
    }

    //-------------------------------------------------------------------------
    internal INodeClientListener _getNodeClientListener()
    {
        return mNodeClientListener;
    }

    //-------------------------------------------------------------------------
    internal INodeClientScript _getNodeClientScript()
    {
        return mNodeClientScript;
    }

    //-------------------------------------------------------------------------
    internal void _setNodeState(_eNodeState state)
    {
        mNodeState = state;
    }

    //-------------------------------------------------------------------------
    internal void _loadDefXml()
    {
        mNodeDef = mNodeSys.getNodeDef(mNodeId);

        Property pEvtType = mNodeDef.GetValue("Type");
        mNodeType = pEvtType.Value.ToString();
    }

    //-------------------------------------------------------------------------
    // 解析TriggerXml
    internal void _parseTriggerXml()
    {
        EventDef entity_def = mNodeDef;
        var trigger_groups = entity_def.GetGroupArray("Trigger");
        if (trigger_groups != null)
        {
            foreach (int i in trigger_groups.Keys)
            {
                Group trg = trigger_groups[i];

                Property p1 = trg.GetValue("LinkedFrom");
                int linked_from = int.Parse(p1.Value);

                int eid = 0;
                if (linked_from == 0)
                {
                    eid = getParentNode().getNodeId();
                }
                else
                {
                }

                Property p2 = trg.GetValue("Type");
                int trg_id = int.Parse(p2.Value);

                int trg_param = 0;
                Property p3 = trg.GetValue("Id");
                if (p3.Value != "")
                {
                    trg_param = int.Parse(p3.Value);
                }

                INodeTriggerFactory factory = mNodeSys.getNodeTriggerFactory(trg_id);
                if (factory != null)
                {
                    INodeTrigger entity_trigger = factory.createTrigger(this, eid, trg_param);
                    mTriggerList.Add(entity_trigger);
                }
            }
        }
    }
}

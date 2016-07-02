using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CNodeTriggerMgr
{
    //-------------------------------------------------------------------------
    Dictionary<int, INodeTriggerFactory> mMapNodeTriggerFactory = new Dictionary<int, INodeTriggerFactory>();
    List<_tMsgInfo> mListMsgInfo = new List<_tMsgInfo>();

    //-------------------------------------------------------------------------
    public void create()
    {
        // 初始化触发器消息列表
        foreach (var i in mMapNodeTriggerFactory)
        {
            _tMsgInfo msg_info = i.Value.getMsgInfo();
            if (msg_info.msg_type != -1 && msg_info.msg_id != -1)
            {
                mListMsgInfo.Add(msg_info);
            }
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mMapNodeTriggerFactory.Clear();
        mListMsgInfo.Clear();
    }

    //-------------------------------------------------------------------------
    public void regNodeTriggerFactory(INodeTriggerFactory trigger_factory)
    {
        mMapNodeTriggerFactory[trigger_factory.getId()] = trigger_factory;
    }

    //-------------------------------------------------------------------------
    public INodeTriggerFactory getNodeTriggerFactory(int trigger_id)
    {
        if (mMapNodeTriggerFactory.ContainsKey(trigger_id))
        {
            return mMapNodeTriggerFactory[trigger_id];
        }
        else
        {
            return null;
        }
    }

    //-------------------------------------------------------------------------
    public Dictionary<int, INodeTriggerFactory> getMapTriggerFactory()
    {
        return mMapNodeTriggerFactory;
    }

    //-------------------------------------------------------------------------
    public bool isTriggerMsg(int msg_type, int msg_id)
    {
        foreach (var i in mListMsgInfo)
        {
            if (i.msg_type == msg_type && i.msg_id == msg_id)
            {
                return true;
            }
        }
        return false;
    }
}
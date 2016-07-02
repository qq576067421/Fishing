using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CMsgMgr
{
    //-------------------------------------------------------------------------
    public enum _eMsgType
    {
        Normal = 0,
        Trigger
    }

    public struct _tMsgInfo
    {
        public int msg_type;
        public int msg_id;
        public int msg_param;
        public List<object> msg_paramlist;
    }

    public struct _tMsgInfoEx
    {
        public _tMsgInfo msg_info;
        public _eMsgType msg_type;
        public int trigger_type;
        public int trigger_id;
    }

    public struct _tRegMsg
    {
        public _tMsgInfo msg_info;
        public bool is_three;
        public int entity_id;
    }

    public struct _tTriggerMsg
    {
        public int trigger_type;
        public int trigger_id;
        public bool is_two;
        public int entity_id;
    }

    //-------------------------------------------------------------------------
    CNodeSys mNodeSys;
    CNodeMgr mNodeMgr;
    List<_tRegMsg> mListRegMsg = new List<_tRegMsg>();// 存贮消息注册信息
    List<_tTriggerMsg> mListTriggerMsg = new List<_tTriggerMsg>();// 存贮自定义触发器消息注册信息

    //-------------------------------------------------------------------------
    public CMsgMgr(CNodeSys node_sys, CNodeMgr node_mgr)
    {
        mNodeSys = node_sys;
        mNodeMgr = node_mgr;
    }

    //-------------------------------------------------------------------------
    public void onMsg(int msg_type, int msg_id, int msg_param, List<object> msg_paramlist)
    {
        // 消息信息收集
        _tMsgInfoEx msg = new _tMsgInfoEx();
        msg.msg_info.msg_type = msg_type;
        msg.msg_info.msg_id = msg_id;
        msg.msg_info.msg_param = msg_param;
        msg.msg_info.msg_paramlist = msg_paramlist;
        msg.msg_type = _eMsgType.Normal;
        bool is_trigger_msg = mNodeSys._getTriggerMgr().isTriggerMsg(msg.msg_info.msg_type, msg.msg_info.msg_id);
        if (is_trigger_msg)
        {
            msg.msg_type = _eMsgType.Trigger;
            msg.trigger_type = msg.msg_info.msg_id;
            msg.trigger_id = msg.msg_info.msg_param;
        }

        // 预处理消息
        int result = _preHandleMsg(msg);

        // 根据预处理结果再次处理该消息
        switch (result)
        {
            case -1:// 被预处理了
                return;
            case 1:// 进入默认处理流程
                List<object> param_list = new List<object>();
                param_list.Add(msg.trigger_type);
                param_list.Add(msg.trigger_id);
                //mEntityMgr.sendMessage(20, 950, 0, param_list);
                return;
            case 0:// 进入原始处理流程
                _handleMsg(msg);
                break;
            default:
                break;
        }
    }

    //-------------------------------------------------------------------------
    // 注册消息
    public void regMessage(int msg_type, int msg_id, int entity_id)
    {
        _tRegMsg msg = new _tRegMsg();
        msg.msg_info.msg_type = msg_type;
        msg.msg_info.msg_id = msg_id;
        msg.is_three = false;
        msg.entity_id = entity_id;
        mListRegMsg.Add(msg);
    }

    //-------------------------------------------------------------------------
    // 注册消息
    public void regMessage(int msg_type, int msg_id, int msg_param, int entity_id)
    {
        _tRegMsg msg = new _tRegMsg();
        msg.msg_info.msg_type = msg_type;
        msg.msg_info.msg_id = msg_id;
        msg.msg_info.msg_param = msg_param;
        msg.is_three = true;
        msg.entity_id = entity_id;
        mListRegMsg.Add(msg);
    }

    //-------------------------------------------------------------------------
    // 注销消息
    public void unregMessage(int msg_type, int msg_id, int entity_id)
    {
        mListRegMsg.RemoveAll(msg =>
            {
                if (msg.entity_id == entity_id
                && msg.msg_info.msg_type == msg_type
                && msg.msg_info.msg_id == msg_id) return true;
                else return false;
            }
            );
    }

    //-------------------------------------------------------------------------
    // 注销消息
    public void unregMessage(int msg_type, int msg_id, int msg_param, int entity_id)
    {
        mListRegMsg.RemoveAll(msg =>
        {
            if (msg.entity_id == entity_id
                && msg.msg_info.msg_type == msg_type
                && msg.msg_info.msg_id == msg_id
                && msg.msg_info.msg_param == msg_param) return true;
            else return false;
        }
            );
    }

    //-------------------------------------------------------------------------
    // 注销消息
    public void unregMessage(int entity_id)
    {
        mListRegMsg.RemoveAll(msg =>
        {
            if (msg.entity_id == entity_id) return true;
            else return false;
        }
            );
    }

    //-------------------------------------------------------------------------
    // 是否注册了该消息
    public bool hasMessage(int msg_type, int msg_id, int msg_param, int entity_id)
    {
        foreach (var i in mListRegMsg)
        {
            if (!i.is_three)
            {
                if (i.msg_info.msg_type == msg_type && i.msg_info.msg_id == msg_id && i.entity_id == entity_id) return true;
            }
            else
            {
                if (i.msg_info.msg_type == msg_type && i.msg_info.msg_id == msg_id && i.msg_info.msg_param == msg_param && i.entity_id == entity_id) return true;
            }
        }

        return false;
    }

    //-------------------------------------------------------------------------
    public void regUserHandleTrigger(int entity_id, int trigger_type, int trigger_id)
    {
        _tTriggerMsg trigger_msg = new _tTriggerMsg();
        trigger_msg.trigger_type = trigger_type;
        trigger_msg.trigger_id = trigger_id;
        trigger_msg.is_two = true;
        trigger_msg.entity_id = entity_id;
        mListTriggerMsg.Add(trigger_msg);
    }

    //-------------------------------------------------------------------------
    public void regUserHandleTrigger(int entity_id, int trigger_type)
    {
        _tTriggerMsg trigger_msg = new _tTriggerMsg();
        trigger_msg.trigger_type = trigger_type;
        trigger_msg.is_two = false;
        trigger_msg.entity_id = entity_id;
        mListTriggerMsg.Add(trigger_msg);
    }

    //-------------------------------------------------------------------------
    public void unregUserHandleTrigger(int entity_id, int trigger_type, int trigger_id)
    {
        List<_tTriggerMsg> l = new List<_tTriggerMsg>(mListTriggerMsg);
        foreach (var i in l)
        {
            if (i.is_two && i.entity_id == entity_id && i.trigger_type == trigger_type && i.trigger_id == trigger_id)
            {
                mListTriggerMsg.Remove(i);
            }
        }
    }

    //-------------------------------------------------------------------------
    public void unregUserHandleTrigger(int entity_id, int trigger_type)
    {
        List<_tTriggerMsg> l = new List<_tTriggerMsg>(mListTriggerMsg);
        foreach (var i in l)
        {
            if (!i.is_two && i.entity_id == entity_id && i.trigger_type == trigger_type)
            {
                mListTriggerMsg.Remove(i);
            }
        }
    }

    //-------------------------------------------------------------------------
    public void unregUserHandleTrigger(int entity_id)
    {
        List<_tTriggerMsg> l = new List<_tTriggerMsg>(mListTriggerMsg);
        foreach (var i in l)
        {
            if (i.entity_id == entity_id)
            {
                mListTriggerMsg.Remove(i);
            }
        }
    }

    //-------------------------------------------------------------------------
    public int _isUserHandleTrigger(_tMsgInfoEx msg)
    {
        if (msg.msg_type != _eMsgType.Trigger) return -1;

        foreach (var i in mListTriggerMsg)
        {
            if (i.is_two)
            {
                if (i.trigger_type == msg.trigger_type && i.trigger_id == msg.trigger_id)
                {
                    return i.entity_id;
                }
            }
            else
            {
                if (i.trigger_type == msg.trigger_type)
                {
                    return i.entity_id;
                }
            }
        }
        return -1;
    }

    //-------------------------------------------------------------------------
    public int _preHandleMsg(_tMsgInfoEx msg)
    {
        int handle_entity_id = _isUserHandleTrigger(msg);
        if (handle_entity_id < 0) return 0;

        return 0;

        //CNode handle_entity = mEntityMgr._getEntity(handle_entity_id);
        //if (handle_entity == null) return 0;
        //List<object> param_list = new List<object>();
        //param_list.Add(msg.trigger_type);
        //param_list.Add(msg.trigger_id);
        //int handle_result = (int)handle_entity.sendPrivateMessage(20, 951, msg.msg_info.msg_param, param_list);
        //return handle_result;
    }

    //-------------------------------------------------------------------------
    public void _handleMsg(_tMsgInfoEx msg)
    {
        //if (msg.msg_type == _eMsgType.Trigger)
        //{
        //    // 将entity_list分成两类：处于start状态一类，处于其他状态一类
        //    List<CNode> entity_list = mEntityMgr._getCurrentNodeList();
        //    List<CNode> el = new List<CNode>(entity_list);
        //    List<CNode> startstate_entity_list = new List<CNode>();
        //    List<CNode> otherstate_entity_list = new List<CNode>();
        //    foreach (var i in el)
        //    {
        //        if (i.getNodeState() == _eNodeState.Start)
        //        {
        //            startstate_entity_list.Add(i);
        //        }
        //        else
        //        {
        //            otherstate_entity_list.Add(i);
        //        }
        //    }
        //    el.Clear();

        //    // 将startstate_entity_list随机洗牌
        //    _reshuffle<CNode>(startstate_entity_list);

        //    // 将startstate_entity_list按trigger权重由高到低排序
        //    //startstate_entity_list.Sort(
        //    //    delegate(CNode a, CNode b)
        //    //    {
        //    //        int aw = a.getTotalTriggerWeight();
        //    //        int bw = b.getTotalTriggerWeight();
        //    //        if (aw > bw)
        //    //        {
        //    //            return -1;
        //    //        }
        //    //        else if (aw == bw)
        //    //        {
        //    //            return 0;
        //    //        }
        //    //        else
        //    //        {
        //    //            return 1;
        //    //        }
        //    //    });

        //    //string w = "";
        //    //foreach (var i in startstate_entity_list)
        //    //{
        //    //    w += i.getTotalTriggerWeight().ToString();
        //    //    w += " ";
        //    //}
        //    //Debug.Log("权重列表为：" + w); 

        //    // 依次执行，跟踪执行结果
        //    bool success = false;
        //    foreach (var i in startstate_entity_list)
        //    {
        //        if (entity_list.Contains(i) && i._hasMessage(msg.msg_info.msg_type, msg.msg_info.msg_id, msg.msg_info.msg_param))
        //        {
        //            i.processEvent("main_sendmsg", msg.msg_info.msg_type, msg.msg_info.msg_id, msg.msg_info.msg_param, msg.msg_info.msg_paramlist);
        //            i.processEvent("main_update", 0.0f);

        //            if (i.getNodeState() != _eNodeState.Start)
        //            {
        //                // 状态切换成功
        //                success = true;
        //                break;
        //            }
        //        }
        //    }

        //    // 所有entity都没有执行成功，执行默认事件
        //    if (!success)
        //    {
        //        List<object> param_list = new List<object>();
        //        param_list.Add(msg.trigger_type);
        //        param_list.Add(msg.trigger_id);
        //        mEntityMgr.sendMessage(20, 950, 0, param_list);
        //    }

        //    // otherstate_entity_list直接广播执行
        //    foreach (var i in otherstate_entity_list)
        //    {
        //        if (entity_list.Contains(i) && i._hasMessage(msg.msg_info.msg_type, msg.msg_info.msg_id, msg.msg_info.msg_param))
        //        {
        //            i.processEvent("main_sendmsg", msg.msg_info.msg_type, msg.msg_info.msg_id, msg.msg_info.msg_param, msg.msg_info.msg_paramlist);
        //        }
        //    }
        //}
        //else
        //{
        //    // 非trigger_msg直接广播执行
        //    List<CNode> entity_list = mEntityMgr._getCurrentNodeList();
        //    List<CNode> l = new List<CNode>(entity_list);
        //    foreach (var i in l)
        //    {
        //        if (entity_list.Contains(i) && i._hasMessage(msg.msg_info.msg_type, msg.msg_info.msg_id, msg.msg_info.msg_param))
        //        {
        //            i.processEvent("main_sendmsg", msg.msg_info.msg_type, msg.msg_info.msg_id, msg.msg_info.msg_param, msg.msg_info.msg_paramlist);
        //        }
        //    }
        //}
    }

    //-------------------------------------------------------------------------
    // 洗牌算法
    public void _reshuffle<T>(List<T> listtemp)
    {
        System.Random ram = new System.Random();
        int current_index;
        T temp_value;
        for (int i = 0; i < listtemp.Count; i++)
        {
            current_index = ram.Next(0, listtemp.Count - i);
            temp_value = listtemp[current_index];
            listtemp[current_index] = listtemp[listtemp.Count - 1 - i];
            listtemp[listtemp.Count - 1 - i] = temp_value;
        }
    }
}
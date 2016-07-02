using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CNodeTriggerConditionMgr
{
    //-------------------------------------------------------------------------
    Dictionary<string, INodeTriggerCondition> mMapEntityTriggerCondition = new Dictionary<string, INodeTriggerCondition>();

    //-------------------------------------------------------------------------
    public void create()
    {
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mMapEntityTriggerCondition.Clear();
    }

    //-------------------------------------------------------------------------
    public void regEntityTriggerCondition(INodeTriggerCondition triggercondition)
    {
        mMapEntityTriggerCondition[triggercondition.getId()] = triggercondition;
    }

    //-------------------------------------------------------------------------
    public INodeTriggerCondition getEntityTriggerCondition(string triggercondition_id)
    {
        if (mMapEntityTriggerCondition.ContainsKey(triggercondition_id))
        {
            return mMapEntityTriggerCondition[triggercondition_id];
        }
        else
        {
            return null;
        }
    }
}
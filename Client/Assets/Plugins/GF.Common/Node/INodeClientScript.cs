using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

public abstract class INodeClientScript
{
    //-------------------------------------------------------------------------
    INodeClientScriptFactory mFactory;
    protected CNode mNode;

    //-------------------------------------------------------------------------
    public INodeClientScript(INodeClientScriptFactory factory, CNode entity)
    {
        mFactory = factory;
        mNode = entity;
    }

    //-------------------------------------------------------------------------
    public string getType()
    {
        return mFactory.getNodeType();
    }

    //-------------------------------------------------------------------------
    public abstract void onEnterStartState(CNode entity);

    //-------------------------------------------------------------------------
    public abstract void onEnterRunState(CNode entity);

    //-------------------------------------------------------------------------
    public abstract void onUpdate(CNode node, float elasped_tm);

    //-------------------------------------------------------------------------
    public abstract void onEnterStopState(CNode node);

    //-------------------------------------------------------------------------
    public abstract void onDoEffect(CNode node, int exit_id);

    //-------------------------------------------------------------------------
    public abstract void onServer2ClientMsg(CNode node, int msg_type, int msg_id, int msg_param, List<object> msg_paramlist);

    //-------------------------------------------------------------------------
    public abstract void onClientMsg(int msg_id, List<object> msg_paramlist);
}

public abstract class INodeClientScriptFactory
{
    //-------------------------------------------------------------------------
    public abstract string getNodeType();

    //-------------------------------------------------------------------------
    public abstract INodeClientScript createScript(CNode entity);

    //-------------------------------------------------------------------------
    public void destroyScript(INodeClientScript s)
    {
    }
}

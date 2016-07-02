using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

public abstract class INodeServerScript
{
    //-------------------------------------------------------------------------
    protected INodeServerScriptFactory mFactory;
    protected CNode mNode;
    protected Entity mEntityPlayer;

    //-------------------------------------------------------------------------
    public INodeServerScript(INodeServerScriptFactory factory, CNode node, Entity entity_player)
    {
        mFactory = factory;
        mNode = node;
        mEntityPlayer = entity_player;
    }

    //-------------------------------------------------------------------------
    public string getType()
    {
        return mFactory.getNodeType();
    }

    //-------------------------------------------------------------------------
    public abstract void onEnterStartState(CNode node);

    //-------------------------------------------------------------------------
    public abstract bool canExitStartState(CNode node);

    //-------------------------------------------------------------------------
    public abstract void onEnterRunState(CNode node);

    //-------------------------------------------------------------------------
    public abstract void onUpdate(CNode node, float elasped_tm);

    //-------------------------------------------------------------------------
    public abstract void onEnterStopState(CNode node);

    //-------------------------------------------------------------------------
    public abstract void onDoEffect(CNode node, int exit_id);

    //-------------------------------------------------------------------------
    public abstract void onClient2ServerMsg(CNode node, int msg_type, int msg_id, int msg_param, List<object> msg_paramlist);

    //-------------------------------------------------------------------------
    public abstract void onServerMsg(CNode node, int msg_type, int msg_id, int msg_param, List<object> msg_paramlist);
}

public abstract class INodeServerScriptFactory
{
    //-------------------------------------------------------------------------
    public abstract string getNodeType();

    //-------------------------------------------------------------------------
    public abstract INodeServerScript createScript(CNode node, Entity entity_player);

    //-------------------------------------------------------------------------
    void destroyScript(INodeServerScript s)
    {
    }
}
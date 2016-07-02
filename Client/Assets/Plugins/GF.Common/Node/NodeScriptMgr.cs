using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventDataXML;
using GF.Common;

class CNodeScriptMgr
{
    //-------------------------------------------------------------------------
    Dictionary<string, INodeServerScriptFactory> mMapNodeServerScriptFactory = new Dictionary<string, INodeServerScriptFactory>();
    Dictionary<string, INodeClientScriptFactory> mMapNodeClientScriptFactory = new Dictionary<string, INodeClientScriptFactory>();

    //-------------------------------------------------------------------------
    public void create(string script_path, string commonscript_path)
    {
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
    }

    //-------------------------------------------------------------------------
    public void regNodeServerScriptFactory(INodeServerScriptFactory factory)
    {
        string key = factory.getNodeType();
        string[] str_list = key.Split('_');
        mMapNodeServerScriptFactory[str_list[0]] = factory;
    }

    //-------------------------------------------------------------------------
    public void regNodeClientScriptFactory(INodeClientScriptFactory factory)
    {
        string key = factory.getNodeType();
        string[] str_list = key.Split('_');
        mMapNodeClientScriptFactory[str_list[0]] = factory;
    }

    //-------------------------------------------------------------------------
    public INodeServerScriptFactory getNodeServerScriptFactory(string node_type)
    {
        if (mMapNodeServerScriptFactory.ContainsKey(node_type))
        {
            return mMapNodeServerScriptFactory[node_type];
        }
        else return null;
    }

    //-------------------------------------------------------------------------
    public INodeClientScriptFactory getNodeClientScriptFactory(string node_type)
    {
        if (mMapNodeClientScriptFactory.ContainsKey(node_type))
        {
            return mMapNodeClientScriptFactory[node_type];
        }
        else return null;
    }
}
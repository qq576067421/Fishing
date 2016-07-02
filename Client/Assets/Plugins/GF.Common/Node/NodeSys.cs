using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EventDataXML;
using GF.Common;

public class CNodeSys : IDisposable
{
    //-------------------------------------------------------------------------
    string mNodeSysMediaPath = "";
    // key=玩家guid，每个玩家拥有一个CNodeMgr
    Dictionary<string, CNodeMgr> mMapNodeMgr = new Dictionary<string, CNodeMgr>();
    Dictionary<int, string> mMapNodeDefFile = new Dictionary<int, string>();
    CNodeScriptMgr mNodeScriptMgr = null;
    CNodeEffectMgr mNodeEffectMgr = null;
    CNodeTriggerMgr mNodeTriggerMgr = null;
    CNodeTriggerConditionMgr mNodeTriggerConditionMgr = null;
    CNodeConfig mNodeConfig = new CNodeConfig();
    bool mIsClient = false;
    bool mbTestMode = false;

    //-------------------------------------------------------------------------
    ~CNodeSys()
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
    public void create(string nodesys_media_path, bool is_client, bool test_mode)
    {
        mNodeSysMediaPath = nodesys_media_path;
        mIsClient = is_client;
        mbTestMode = test_mode;

        // 创建效果管理器
        if (mNodeEffectMgr == null)
        {
            mNodeEffectMgr = new CNodeEffectMgr();
        }

        // 创建触发器管理器
        if (mNodeTriggerMgr == null)
        {
            mNodeTriggerMgr = new CNodeTriggerMgr();
        }

        // 创建触发条件管理器
        if (mNodeTriggerConditionMgr == null)
        {
            mNodeTriggerConditionMgr = new CNodeTriggerConditionMgr();
        }

        // 创建脚本管理器
        if (mNodeScriptMgr == null)
        {
            mNodeScriptMgr = new CNodeScriptMgr();
            mNodeScriptMgr.create("", "");
        }

        // 创建
        _create();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var it in mMapNodeMgr)
        {
            it.Value.Dispose();
        }
        mMapNodeMgr.Clear();

        mMapNodeDefFile.Clear();

        if (mNodeScriptMgr != null)
        {
            mNodeScriptMgr.destroy();
            mNodeScriptMgr = null;
        }

        if (mNodeEffectMgr != null)
        {
            mNodeEffectMgr.destroy();
            mNodeEffectMgr = null;
        }

        if (mNodeTriggerMgr != null)
        {
            mNodeTriggerMgr.destroy();
            mNodeTriggerMgr = null;
        }

        if (mNodeTriggerConditionMgr != null)
        {
            mNodeTriggerConditionMgr.destroy();
            mNodeTriggerConditionMgr = null;
        }
    }

    //-------------------------------------------------------------------------
    public void regNodeEffect(INodeEffect effect)
    {
        mNodeEffectMgr.regNodeEffect(effect);
    }

    //-------------------------------------------------------------------------
    public INodeEffect getNodeEffect(string effect_id)
    {
        return mNodeEffectMgr.getNodeEffect(effect_id);
    }

    //-------------------------------------------------------------------------
    public void regNodeTriggerFactory(INodeTriggerFactory trigger_factory)
    {
        mNodeTriggerMgr.regNodeTriggerFactory(trigger_factory);
    }

    //-------------------------------------------------------------------------
    public INodeTriggerFactory getNodeTriggerFactory(int trigger_id)
    {
        return mNodeTriggerMgr.getNodeTriggerFactory(trigger_id);
    }

    //-------------------------------------------------------------------------
    public void regEntityTriggerCondition(INodeTriggerCondition trigger_condition)
    {
        mNodeTriggerConditionMgr.regEntityTriggerCondition(trigger_condition);
    }

    //-------------------------------------------------------------------------
    public INodeTriggerCondition getEntityTriggerCondition(string triggercondition_id)
    {
        return mNodeTriggerConditionMgr.getEntityTriggerCondition(triggercondition_id);
    }

    //-------------------------------------------------------------------------
    public void regNodeServerScriptFactory(INodeServerScriptFactory factory)
    {
        mNodeScriptMgr.regNodeServerScriptFactory(factory);
    }

    //-------------------------------------------------------------------------
    public INodeServerScriptFactory getNodeServerScriptFactory(string node_type)
    {
        return mNodeScriptMgr.getNodeServerScriptFactory(node_type);
    }

    //-------------------------------------------------------------------------
    public void regNodeClientScriptFactory(INodeClientScriptFactory factory)
    {
        mNodeScriptMgr.regNodeClientScriptFactory(factory);
    }

    //-------------------------------------------------------------------------
    public INodeClientScriptFactory getNodeClientScriptFactory(string node_type)
    {
        return mNodeScriptMgr.getNodeClientScriptFactory(node_type);
    }

    //-------------------------------------------------------------------------
    //public void update(float elapsed_tm)
    //{
    //    foreach (var it in mMapNodeMgr)
    //    {
    //        it.Value._update(elapsed_tm);
    //    }
    //}

    //-------------------------------------------------------------------------
    public CNodeMgr createNodeMgr(string player_guid, INodeServerListener server_listener,
        INodeClientListener client_listener, Entity et_player)
    {
        if (!mMapNodeMgr.ContainsKey(player_guid))
        {
            CNodeMgr entity_mgr = new CNodeMgr();
            entity_mgr._create(this, server_listener, client_listener, et_player);
            mMapNodeMgr.Add(player_guid, entity_mgr);

            return entity_mgr;
        }
        else
        {
            return mMapNodeMgr[player_guid];
        }
    }

    //-------------------------------------------------------------------------
    public void destroyNodeMgr(string player_guid)
    {
        if (mMapNodeMgr.ContainsKey(player_guid))
        {
            mMapNodeMgr[player_guid]._destroy();
            mMapNodeMgr.Remove(player_guid);
        }
    }

    //-------------------------------------------------------------------------
    // 获取EntityDef
    public EventDataXML.EventDef getNodeDef(int node_id)
    {
        if (!mMapNodeDefFile.ContainsKey(node_id))
        {
            EbLog.Error("CNodeSys.getNodeDef() 没有找到指定node_id=" + node_id.ToString() + "的Xml文件");
            return null;
        }
        else
        {
            //string filepath = mNodeSysMediaPath + "/XmlDef/";
            //string filename = filepath + mMapNodeDefFile[node_id];

            string file_name = mMapNodeDefFile[node_id];
            var node_def = EventDataXML.EventDef.LoadEventData(file_name);
            return node_def;
        }
    }

    //-------------------------------------------------------------------------
    public bool hasParentNodeDef(int node_id)
    {
        EventDataXML.EventDef entity_def = getNodeDef(node_id);

        if (null == entity_def)
        {
            EbLog.Error("CNodeSys._isChildEntity(), null == entity_def, node_id=" + node_id);
            return false;
        }

        string str_parent_entity_id = entity_def.GetValue("Parent").Value;
        if (str_parent_entity_id != string.Empty && str_parent_entity_id != "0")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //-------------------------------------------------------------------------
    public bool hasChildNodeDef(int node_id)
    {
        EventDataXML.EventDef entity_def = getNodeDef(node_id);

        if (null == entity_def)
        {
            EbLog.Error("CNodeSys._isChildEntity(), null == entity_def, node_id=" + node_id);
            return false;
        }

        bool has = false;
        bool ok = false;

        var group = entity_def.GetGroup("LinkedToChild");
        if (group != null)
        {
            ok = true;

            Property p_entity_def_uid = group.GetValue("UID");
            int this_child_entity_id = int.Parse(p_entity_def_uid.Value);
            if (this_child_entity_id != 0)
            {
                has = true;
            }
        }

        if (!ok)
        {
            var groups = entity_def.GetGroupArray("LinkedToChild");
            if (groups != null)
            {
                foreach (int i in groups.Keys)
                {
                    Property p_entity_def_uid = groups[i].GetValue("UID");
                    int this_child_entity_id = int.Parse(p_entity_def_uid.Value);
                    if (this_child_entity_id != 0)
                    {
                        has = true;
                    }
                }
            }
        }

        return has;
    }

    //-------------------------------------------------------------------------
    public bool isTestMode()
    {
        return mbTestMode;
    }

    //-------------------------------------------------------------------------
    public bool isClient()
    {
        return mIsClient;
    }

    //-------------------------------------------------------------------------
    internal CNodeConfig _getConfig()
    {
        return mNodeConfig;
    }

    //-------------------------------------------------------------------------
    internal CNodeTriggerMgr _getTriggerMgr()
    {
        return mNodeTriggerMgr;
    }

    //-------------------------------------------------------------------------
    private void _create()
    {
        // 初始化entity配置文件
        string filepath = mNodeSysMediaPath + "/Config/NodeConfig.xml";
        mNodeConfig.setup(filepath);

        // 初始化本地所有EntityDef文件
        mMapNodeDefFile.Clear();
        filepath = mNodeSysMediaPath + "/XmlDef/";
        string[] list_xml = Directory.GetFiles(filepath, "*.xml", System.IO.SearchOption.AllDirectories);

        foreach (string item in list_xml)
        {
            string s = item.Replace("\\", "/");
            int sep_index = s.LastIndexOf('/');
            string file_name = s.Substring(sep_index + 1);
            string[] tags = file_name.Split(new char[] { '_', '.' });
            if (tags[tags.Length - 1] == "meta") continue;
            if (tags.Length > 3)
            {
                int id = int.Parse(tags[1]);
                mMapNodeDefFile[id] = s;
            }
            else if (tags.Length == 3)
            {
            }
        }

        mNodeEffectMgr.create();
        mNodeTriggerMgr.create();
        mNodeTriggerConditionMgr.create();
    }
}

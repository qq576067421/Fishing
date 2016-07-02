using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EventDataXML
{
    [XmlRoot("Scenario")]
    public class data_Scenario
    {
        //---------------------------------------------------------------------
        [XmlIgnore]
        public int wbsid = 0;
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public int episode;//意义不明
        [XmlAttribute]
        public int StartEvent = 0;//泳道的启动事件
        [XmlElement("Event")]
        public List<ScenarioEvent> events = new List<ScenarioEvent>();
        //[XmlIgnore]
        //public string path;
        [XmlIgnore]
        public string Id;
        [XmlIgnore]
        public string filename;

        //---------------------------------------------------------------------
        static public bool isContain(string path, data_Scenario data)
        {
            if (path != data.filename)
                return false;
            return true;
        }

        //---------------------------------------------------------------------
        static public data_Scenario Init(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(data_Scenario));
                using (var stream = RemoteStream.RemoteStream.DefRemoteStrem.OpenRead(filename, true))
                //using(var stream = System.IO.File.OpenRead(filename))
                {
                    try
                    {
                        data_Scenario s = xmls.Deserialize(stream) as data_Scenario;
                        //p.RootPath = System.IO.Path.GetDirectoryName(filename);
                        s.filename = filename;
                        s.ParseData();
                        return s;
                    }
                    catch (Exception err)
                    {
                        return null;
                    }
                }
                //加载
            }
            else
            {
                return null;
            }
        }

        //---------------------------------------------------------------------
        public bool Save()
        {
            XmlSerializer xmls = new XmlSerializer(typeof(data_Scenario));
            using (var stream = System.IO.File.Create(filename))
            {
                try
                {
                    xmls.Serialize(stream, this);
                }
                catch (Exception err)
                {
                    return false;
                }
            }
            return true;
        }

        //---------------------------------------------------------------------
        // 处理所有的子节点，排序\组织父子关系
        public void ParseData()
        {
            events.Sort((a, b) =>
            {
                return a.seq - b.seq;
            });
            foreach (var node in events)
            {
                node.bstart = false;
                if (node.UID == this.StartEvent && this.StartEvent > 0)
                    node.bstart = true;
                node.parent = null;
                //node.parent = null;
                //node.path = RootPath + "\\" + node.text;
                ParseNode(node);
            }
        }

        //---------------------------------------------------------------------
        void ParseNode(ScenarioEvent node)
        {
            node.subevents.Sort((a, b) =>
            {
                return a.seq - b.seq;
            });

            foreach (var snode in node.subevents)
            {
                snode.parent = node;
                snode.bstart = false;
                if (snode.UID == node.StartSubEvent && node.StartSubEvent > 0)
                {
                    snode.bstart = true;
                }
                //node.parent = null;
                //node.path = RootPath + "\\" + node.text;
                ParseNode(snode);
            }
            node.exits.Sort((a, b) =>
            {
                if (a.Id == 0 && b.Id == 0) return 0;

                if (a.Id == 0) return int.MaxValue - b.Id;
                if (b.Id == 0) return a.Id - int.MaxValue;
                return a.Id - b.Id;
            });
            for (int i = 0; i < node.exits.Count; i++)
            {
                // node.exits[i].Id = i + 1;
            }
        }

        //---------------------------------------------------------------------
        public void UpdateWbs()
        {
            events.Sort((a, b) =>
            {
                return a.seq - b.seq;
            });
            int index = 1;
            foreach (var _event in events)
            {
                _event.seq = index;
                _event._wbs = wbsid.ToString() + "." + index.ToString();
                index++;
                _event.UpdateWbs();
            }
        }
    }
    public class ScenarioEventExitTrigger
    {
        //---------------------------------------------------------------------
        [XmlAttribute]
        public int TriggerType = 0;
        [XmlAttribute]
        public int TriggerID = 0;
        [XmlIgnore]
        public string tag = null;

        //---------------------------------------------------------------------
        public ScenarioEventExitTrigger Copy()
        {
            ScenarioEventExitTrigger p = new ScenarioEventExitTrigger();
            p.TriggerType = this.TriggerType;
            p.TriggerID = this.TriggerID;
            return p;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            if (tag == null) return TriggerType.ToString() + ":" + TriggerID.ToString();
            else return tag + ":" + TriggerID.ToString();
        }
    }
    public class ScenarioEventExit
    {
        //---------------------------------------------------------------------
        [XmlAttribute]
        public int Id;
        [XmlAttribute]
        public string text = "";
        [XmlElement("Trigger")]
        public List<ScenarioEventExitTrigger> Triggers = new List<ScenarioEventExitTrigger>();
        [XmlAttribute]
        public int Successor = 0;//指向事件
        [XmlAttribute]
        public int BoundToParentExit = 0;//指向上层事件出口
        //以上只能2选一
        //[XmlAttribute]
        //public int BindTag = 0;//绑定标志，事件可以指定根据不同的绑定标志作出不同反应

        //---------------------------------------------------------------------
        public ScenarioEventExit Copy()
        {
            ScenarioEventExit newi = new ScenarioEventExit();
            newi.Id = this.Id;
            newi.text = this.text;
            //newi.BindTag = this.BindTag;
            foreach (var trigger in this.Triggers)
            {
                newi.Triggers.Add(trigger.Copy());
            }
            //newi.TriggerType = this.TriggerType;
            newi.Successor = this.Successor;
            //newi.Scenario = this.Scenario;
            newi.BoundToParentExit = this.BoundToParentExit;
            return newi;
        }
    }

    public class ScenarioEvent
    {
        //---------------------------------------------------------------------
        public ScenarioEvent()
        {
            this.index = GetIndex();
            if (index == 8 || index == 20 || index == 28)
            {
                int s = 1;
            }
        }

        //---------------------------------------------------------------------
        public static int g_index = 0;
        public static int GetIndex()
        {
            g_index++;
            return g_index;
        }
        public int index;
        [XmlAttribute]
        public int seq;//显示顺序
        [XmlAttribute]
        public int Type;//事件类型，有啥用？意义不明，和typedef关系？
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public int UID;//事件唯一ID
        [XmlAttribute]
        public int StartSubEvent = 0;
        [XmlElement("Event")]
        public List<ScenarioEvent> subevents = new List<ScenarioEvent>();
        [XmlElement("Exit")]
        public List<ScenarioEventExit> exits = new List<ScenarioEventExit>();
        [XmlIgnore]
        public bool bstart = false;
        [XmlIgnore]
        public ScenarioEvent parent = null;
        [XmlIgnore]
        public string _wbs = "";

        //---------------------------------------------------------------------
        public void UpdateWbs()
        {
            subevents.Sort((a, b) =>
            {
                return a.seq - b.seq;
            });
            int index = 1;
            foreach (var _event in subevents)
            {
                _event.seq = index;
                _event._wbs = _wbs + "." + index.ToString();
                index++;
                _event.UpdateWbs();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EventDataXML
{
    public class Property
    {
        [XmlAttribute]
        public string Key = "";
        [XmlAttribute]
        public string Type = "";
        [XmlAttribute]
        public string Value = "";
        //[XmlAttribute]
        //public string Index = null;

        public override string ToString()
        {
            return "Key=" + Key + ";Type=" + Type + ";Value=" + Value;
        }

        public string GetName()
        {
            return Key;
        }
    }

    public class Group
    {
        public Group()
        {
        }

        public Group(string key)
        {
            var iss = key.Split(new char[] { '(', ')' });
            this.Key = iss[0];
            if (iss.Length >= 2)
            {
                this.Index = iss[1];
            }
        }

        [XmlAttribute]
        public string Key = "";
        [XmlAttribute]
        public string Index = null;

        public string GetName()
        {
            if (Index == null) return Key;
            if (Index == "") return Key;
            return Key + "(" + Index + ")";
        }

        [XmlElement("Property")]
        public List<Property> Propertys = new List<Property>();
        [XmlElement("Group")]
        public List<Group> Groups = new List<Group>();

        public override string ToString()
        {
            return "Group(" + (Propertys.Count + Groups.Count).ToString() + ")";
        }

        public Property GetValue(string key)
        {
            if (key.Contains("."))
            {
                var strs = key.Split('.');
                Queue<string> qstr = new Queue<string>();
                foreach (var substr in strs)
                {
                    qstr.Enqueue(substr);
                }
                return GetValue(qstr);
            }
            else
            {
                foreach (var sp in Propertys)
                {
                    if (sp.GetName() == key) return sp;
                }
                return null;
            }
        }

        public Property GetValue(Queue<string> keys)
        {
            var key = keys.Dequeue();
            if (keys.Count == 0)
            {
                return GetValue(key);
            }
            else
            {
                foreach (var g in Groups)
                {
                    if (g.GetName() == key)
                    {
                        return g.GetValue(keys);
                    }
                }
                return null;
            }
        }

        public Group GetGroup(string key)
        {
            if (key.Contains("."))
            {
                var strs = key.Split('.');
                Queue<string> qstr = new Queue<string>();
                foreach (var substr in strs)
                {
                    qstr.Enqueue(substr);
                }
                return GetGroup(qstr);
            }
            else
            {
                foreach (var sp in Groups)
                {
                    if (sp.GetName() == key) return sp;
                }
                return null;
            }
        }

        public Group GetGroup(Queue<string> keys)
        {
            var key = keys.Dequeue();
            if (keys.Count == 0)
            {
                return GetGroup(key);
            }
            else
            {
                foreach (var g in Groups)
                {
                    if (g.GetName() == key)
                    {
                        return g.GetGroup(keys);
                    }
                }
                return null;
            }
        }

        public Dictionary<int, Group> GetGroupArray(string key)
        {
            if (key.Contains("."))
            {
                var strs = key.Split('.');
                Queue<string> qstr = new Queue<string>();
                foreach (var substr in strs)
                {
                    qstr.Enqueue(substr);
                }
                return GetGroupArray(qstr);
            }
            else
            {
                Dictionary<int, Group> array = new Dictionary<int, Group>();
                foreach (var sp in Groups)
                {
                    if (sp.Key == key)
                    {
                        int i = int.Parse(sp.Index);
                        array.Add(i, sp);
                    }
                }
                //if (array.Count == 0) return null;
                return array;
            }
        }

        public Dictionary<int, Group> GetGroupArray(Queue<string> keys)
        {
            var key = keys.Dequeue();
            if (keys.Count == 0)
            {
                return GetGroupArray(key);
            }
            else
            {
                foreach (var g in Groups)
                {
                    if (g.GetName() == key)
                    {
                        return g.GetGroupArray(keys);
                    }
                }
                return null;
            }
        }

        public Property CreateProperty(Queue<string> keys)
        {
            string key = keys.Dequeue();
            if (keys.Count == 0)
            {
                Property p = new Property();
                p.Key = key;
                this.Propertys.Add(p);
                return p;
            }
            foreach (Group g in this.Groups)
            {
                if (g.GetName() == key)
                {
                    return g.CreateProperty(keys);
                }
            }
            {
                Group g = new Group(key);
                this.Groups.Add(g);
                return g.CreateProperty(keys);
            }
        }

        public Property CreateProperty(string key)
        {
            if (key.Contains("."))
            {
                var strs = key.Split('.');
                Queue<string> qstr = new Queue<string>();
                foreach (var substr in strs)
                {
                    qstr.Enqueue(substr);
                }
                return CreateProperty(qstr);
            }
            else
            {
                Property p = new Property();
                p.Key = key;
                this.Propertys.Add(p);
                return p;
            }
        }
    }

    public class EventDef : Group
    {
        [XmlAttribute]
        public int BaseTypeDef = 0;// 基础类型定义

        public EventDef()
        {
            Key = null;
        }

        //将文件xml反序列化读取
        public static EventDef LoadEventData(System.IO.Stream filestream)
        {
            System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(EventDef));
            return s.Deserialize(filestream) as EventDef;
        }

        //将文件XML序列化保存
        public void SaveEventData(System.IO.Stream filestream)
        {
            System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(EventDef));

            s.Serialize(filestream, this);
        }

        public static EventDef LoadEventData(string filename)
        {
            try
            {
                using (var fs = RemoteStream.RemoteStream.DefRemoteStrem.OpenRead(filename, true))
                //using (var fs = System.IO.File.OpenRead(filename))
                {
                    return LoadEventData(fs);
                }
            }
            catch (Exception e)
            {
                throw new Exception(filename, e);
            }
        }

        public void SaveEventData(string filename)
        {
            try
            {
                using (var fs = System.IO.File.Create(filename))
                {
                    SaveEventData(fs);
                }
            }
            catch
            {
            }
        }
    }

    public class QuickGroup
    {
        public QuickGroup()
        {
        }

        public QuickGroup(string key)
        {
            var iss = key.Split(new char[] { '(', ')' });
            this.Key = iss[0];
            if (iss.Length >= 2)
            {
                this.Index = iss[1];
            }
        }

        [XmlAttribute]
        public string Key = "";
        [XmlAttribute]
        public string Index = null;

        public string GetName()
        {
            if (Index == null)
                return Key;
            if (Index == "")
                return Key;
            return Key + "(" + Index + ")";
        }

        [XmlElement("Property")]
        public List<Property> Propertys = new List<Property>();

        public override string ToString()
        {
            return "Group(" + (Propertys.Count).ToString() + ")";
        }

        public Property GetValue(string key)
        {
            if (key.Contains("."))
            {
                var strs = key.Split('.');
                Queue<string> qstr = new Queue<string>();
                foreach (var substr in strs)
                {
                    qstr.Enqueue(substr);
                }
                return GetValue(qstr);
            }
            else
            {
                foreach (var sp in Propertys)
                {
                    if (sp.GetName() == key) return sp;
                }
                return null;
            }
        }

        public Property GetValue(Queue<string> keys)
        {
            var key = keys.Dequeue();
            if (keys.Count == 0)
            {
                return GetValue(key);
            }
            else
            {
                return null;
            }
        }
    }

    [XmlRoot("EventDef")]
    public class EventDefQuick : QuickGroup
    {
        [XmlAttribute]
        public int BaseTypeDef = 0;//基础类型定义
        public EventDefQuick()
        {
            Key = null;
        }

        public static EventDefQuick LoadEventData(System.IO.Stream filestream)
        {
            System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(EventDefQuick));
            return s.Deserialize(filestream) as EventDefQuick;
        }

        public void SaveEventData(System.IO.Stream filestream)
        {
            System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(EventDefQuick));
            s.Serialize(filestream, this);
        }

        public void SaveEventData(string filename)
        {
            try
            {
                using (var fs = System.IO.File.Create(filename))
                {
                    SaveEventData(fs);
                }
            }
            catch
            {
            }
        }
    }
}

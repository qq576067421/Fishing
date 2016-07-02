using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EventDataType
{
    public class Property
    {
        public Property Copy()
        {
            Property p = new Property();
            p.Key=this.Key;
            p.Label =this.Label;
            p.DataType =this.DataType;
            p.Display =this.Display;
            p.Visible =this.Visible;
            p.Value =this.Value;
            p.Validation = this.Validation;
            p.Tips =this.Tips;
            p.Range = this.Range;
            p.Optional =this.Optional;
            p.DSType =this.DSType;
            p.DSId = this.DSId;
            p.Control = this.Control;
            p.DSDisplayCol = this.DSDisplayCol;
            p.DSValueCol = this.DSValueCol;
            p.Popup =this.Popup;
            p.Parameters = this.Parameters;
            p.PreviewFolder = this.PreviewFolder;
            p.Editable = this.Editable;
            p.Lines = this.Lines;
            return p;
        }
        [XmlAttribute]
        public string Key = "";
        [XmlAttribute]
        public string Label = "";
        [XmlAttribute]
        public string DataType = "";
        [XmlAttribute]
        public string Display = null;
        [XmlAttribute]
        public string Visible = null;
        [XmlAttribute]
        public string Value = null;
        [XmlAttribute]
        public string Validation = null;
        [XmlAttribute]
        public string Tips = null;
        [XmlAttribute]
        public string Range = null;
        [XmlAttribute]
        public string Optional = null;
        [XmlAttribute]
        public string DSType = null;
        [XmlAttribute]
        public string DSId = null;
        [XmlAttribute]
        public string Control = null;
        [XmlAttribute]
        public string DSDisplayCol = null;
        [XmlAttribute]
        public string DSValueCol = null;
        [XmlAttribute]
        public string DSFilterCol = null;
        [XmlAttribute]
        public string Filter = null;
        [XmlAttribute]
        public string Popup = null;
        [XmlAttribute]
        public string PreviewFolder = null;
        [XmlAttribute]
        public string Parameters = null;
        [XmlAttribute]
        public string Editable = null;
        [XmlAttribute]
        public int Lines = 0;
    }

    public class Category
    {
        public void Copyto(Category t)
        {
            t.Key = this.Key;
            t.Label = this.Label;
            t.Display = this.Display;
            t.Count = this.Count;
            t.Rollup = this.Rollup;
            t.MinOccurs = this.MinOccurs;
            t.MaxOccurs = this.MaxOccurs;
            t.Editable = this.Editable;

            foreach (Category c in categorys)
            {
                t.categorys.Add(c.Copy());
            }
            foreach (Property p in propertys)
            {
                t.propertys.Add(p.Copy());
            }
        }
        public virtual Category Copy()
        {
            Category t = new Category();
            Copyto(t);
            return t;
        }
        [XmlAttribute]
        public string Key = "";
        [XmlAttribute]
        public string Label = "";
        [XmlAttribute]
        public string Display = null;
        [XmlAttribute]
        public string Count = null;
        [XmlAttribute]
        public string Rollup = null;
        [XmlAttribute]
        public string MinOccurs = null;
        [XmlAttribute]
        public string MaxOccurs = null;
        [XmlAttribute]
        public string Editable = null;
        [XmlAttribute]
        public int InsertBefore = 0;
        [XmlElement("Category")]
        public List<Category> categorys = new List<Category>();
        [XmlElement("Property")]
        public List<Property> propertys = new List<Property>();

        public Category AddSubCategory()
        {
            Category c= new Category();
            categorys.Add(c);
            return c;

        }
        public Property AddSubProperty()
        {
            Property p = new Property();
            propertys.Add(p);
            return p;
        }
    }

    public class __PropertyGrid:Category
    {
        public __PropertyGrid()
        {
            Key = null;
            Label = null;
        }

        public override Category Copy()
        {
            __PropertyGrid d = new __PropertyGrid();
            //d.Version = this.Version;
            //d.Name = this.Name;
            //d.ID = this.ID;
            Copyto(d);
            return d;
        }

    }

    public class EventTypeDef
    {
        [XmlElement("PropertyGrid")]
        public __PropertyGrid PropertyGrid=new __PropertyGrid();
        [XmlAttribute]
        public string Version = "0.0.1";
        [XmlAttribute]
        public string Name = "BaseEvent";
        [XmlAttribute]
        public int ID = 0;  
    }
}
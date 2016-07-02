using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GF.Common;

class CNodeConfig
{
    //-------------------------------------------------------------------------
    int mStartEntity = 0;

    //-------------------------------------------------------------------------
    public void setup(string xml_file)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(xml_file);

        XmlElement el_starnode = doc.DocumentElement["StartNode"];
        mStartEntity = int.Parse(el_starnode.GetAttribute("Id"));
    }

    //-------------------------------------------------------------------------
    public int getStartEntity()
    {
        return mStartEntity;
    }
}
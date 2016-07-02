using System;
using System.Collections.Generic;
using System.Text;

namespace GF.Common
{
    public abstract class EbData
    {
        //---------------------------------------------------------------------
        public int Id { get; internal set; }

        //---------------------------------------------------------------------
        public abstract void load(EbPropSet prop_set);
    }

    public class EbPropSet
    {
        //---------------------------------------------------------------------
        // key=prop_name
        Dictionary<string, IProp> mMapProp = new Dictionary<string, IProp>();

        //---------------------------------------------------------------------
        public int Id { get; internal set; }

        //---------------------------------------------------------------------
        public IProp getProp(string key)
        {
            IProp prop;
            mMapProp.TryGetValue(key, out prop);
            return prop;
        }

        //---------------------------------------------------------------------
        public Prop<int> getPropInt(string key)
        {
            IProp prop;
            mMapProp.TryGetValue(key, out prop);
            if (prop == null) return null;
            Prop<int> p = (Prop<int>)prop;
            return p;
        }

        //---------------------------------------------------------------------
        public Prop<float> getPropFloat(string key)
        {
            IProp prop;
            mMapProp.TryGetValue(key, out prop);
            if (prop == null) return null;
            Prop<float> p = (Prop<float>)prop;
            return p;
        }

        //---------------------------------------------------------------------
        public Prop<string> getPropString(string key)
        {
            IProp prop;
            mMapProp.TryGetValue(key, out prop);
            if (prop == null) return null;
            Prop<string> p = (Prop<string>)prop;
            return p;
        }

        //---------------------------------------------------------------------
        internal void _addProp(string key, IProp prop)
        {
            mMapProp[key] = prop;
        }
    }

    public class EbTable
    {
        //---------------------------------------------------------------------
        // key=prop_name
        Dictionary<string, PropDef> mMapPropDef = new Dictionary<string, PropDef>();
        Dictionary<int, EbPropSet> mMapPropSet = new Dictionary<int, EbPropSet>();

        //---------------------------------------------------------------------
        public string Name { get; internal set; }

        //---------------------------------------------------------------------
        public PropDef getPropDef(string key)
        {
            PropDef prop_def;
            mMapPropDef.TryGetValue(key, out prop_def);
            return prop_def;
        }

        //---------------------------------------------------------------------
        public EbPropSet getPropSet(int id)
        {
            EbPropSet prop_set;
            mMapPropSet.TryGetValue(id, out prop_set);
            if (prop_set == null)
            {
                EbLog.Error("EbTable.getPropSet() Error! not exist prop_set,id=" + id);
            }
            return prop_set;
        }

        //---------------------------------------------------------------------
        public Dictionary<int, EbPropSet> getAllPropSet()
        {
            return mMapPropSet;
        }

        //---------------------------------------------------------------------
        internal void _addPropDef(PropDef prop_def)
        {
            mMapPropDef[prop_def.getKey()] = prop_def;
        }

        //---------------------------------------------------------------------
        internal void _addPropSet(EbPropSet prop_set)
        {
            mMapPropSet[prop_set.Id] = prop_set;
        }
    }

    public class EbDb
    {
        //---------------------------------------------------------------------
        Dictionary<string, EbTable> mMapTable = new Dictionary<string, EbTable>();

        //---------------------------------------------------------------------
        internal EbTable _getTable(string table_name)
        {
            EbTable table;
            mMapTable.TryGetValue(table_name, out table);
            if (table == null)
            {
                EbLog.Error("EbDb.getTable() Error! not exist table,table_name=" + table_name);
            }
            return table;
        }

        //---------------------------------------------------------------------
        internal void _addTable(EbTable table)
        {
            mMapTable[table.Name] = table;
        }
    }
}

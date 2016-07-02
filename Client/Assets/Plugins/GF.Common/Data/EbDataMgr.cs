using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GF.Common
{
    public enum EbFieldType
    {
        None = 0,
        Int,
        Float,
        String
    }

    public class EbDataMgr
    {
        //---------------------------------------------------------------------
        static EbDataMgr mDataMgr;
        EbFileStream mFileStream = new EbFileStreamDefault();
        EbDb mDb = new EbDb();
        ISqlite mSqlite;
        Dictionary<string, Dictionary<int, EbData>> mMapData = new Dictionary<string, Dictionary<int, EbData>>();

        //---------------------------------------------------------------------
        static public EbDataMgr Instance
        {
            get { return mDataMgr; }
        }

        //---------------------------------------------------------------------
        public EbDataMgr()
        {
            mDataMgr = this;
        }

        //---------------------------------------------------------------------
        public void setup(string db_name, string db_filename)
        {
#if UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_DASHBOARD_WIDGET || UNITY_STANDALONE_LINUX || UNITY_WEBPLAYER
            mSqlite = new SqliteUnity(db_filename);
#else
            mSqlite = new SqliteWin(db_filename);
#endif
            if (!mSqlite.openDb())
            {
                EbLog.Note("EbDataMgr.setup() failed! Can not Open File! db_filename=" + db_filename);
                return;
            }

            try
            {
                // 加载所有Table数据
                HashSet<string> list_tablename = _loadAllTableName();
                foreach (var i in list_tablename)
                {
                    _loadTable(i);
                }

                mSqlite.closeDb();
            }
            catch (Exception e)
            {
                EbLog.Note(e.ToString());
            }
        }

        //---------------------------------------------------------------------
        public void loadData<T>(string table_name) where T : EbData, new()
        {
            string key = typeof(T).Name;
            Dictionary<int, EbData> map_data = new Dictionary<int, EbData>();
            mMapData[key] = map_data;

            EbTable table = getTable(table_name);
            Dictionary<int, EbPropSet> map_propset = table.getAllPropSet();
            foreach (var i in map_propset)
            {
                T data = new T();
                data.Id = i.Value.Id;
                data.load(i.Value);
                map_data[data.Id] = data;
            }
        }

        //---------------------------------------------------------------------
        public T getData<T>(int id) where T : EbData
        {
            string key = typeof(T).Name;
            Dictionary<int, EbData> map_data = null;

            mMapData.TryGetValue(key, out map_data);
            if (map_data == null)
            {
                EbLog.Error("EbDataMgr.getData() Error, can't found EbData Type=" + key);
                return default(T);
            }

            EbData data = null;
            map_data.TryGetValue(id, out data);
            if (data == null) return default(T);
            else return (T)data;
        }

        //---------------------------------------------------------------------
        public Dictionary<int, EbData> getMapData<T>() where T : EbData
        {
            string key = typeof(T).Name;
            Dictionary<int, EbData> map_data = null;

            mMapData.TryGetValue(key, out map_data);
            return map_data;
        }

        //---------------------------------------------------------------------
        public EbTable getTable(string table_name)
        {
            return mDb._getTable(table_name);
        }

        //---------------------------------------------------------------------
        // 获取Db中所有表名
        HashSet<string> _loadAllTableName()
        {
            HashSet<string> list_tablename = new HashSet<string>();
            string str_query = string.Format("SELECT * FROM {0};", "sqlite_master");
            list_tablename = mSqlite.getAllTableName(str_query);
            return list_tablename;
        }

        //---------------------------------------------------------------------
        void _loadTable(string table_name)
        {
            string str_query_select = string.Format("SELECT * FROM {0};", table_name);
            Dictionary<int, List<DataInfo>> map_data = mSqlite.getTableData(str_query_select);
            if (map_data.Count <= 0)
            {
                return;
            }

            EbTable table = new EbTable();
            table.Name = table_name;

            foreach (var i in map_data)
            {
                EbPropSet prop_set = new EbPropSet();
                int data_id = i.Key;
                List<DataInfo> list_data_info = i.Value;
                foreach (var data_info in list_data_info)
                {
                    object data_value = data_info.data_value;
                    string data_name = data_info.data_name;

                    switch (data_info.data_type)
                    {
                        case 1:
                            {
                                PropDef prop_def = table.getPropDef(data_name);
                                if (prop_def == null)
                                {
                                    PropDef d = new PropDef(data_name, typeof(int), false);
                                    table._addPropDef(d);
                                }
                                Prop<int> prop = new Prop<int>(null, prop_def, 0);
                                prop.set((int)data_value);
                                prop_set._addProp(data_name, prop);
                            }
                            break;
                        case 2:
                            {
                                PropDef prop_def = table.getPropDef(data_name);
                                if (prop_def == null)
                                {
                                    PropDef d = new PropDef(data_name, typeof(float), false);
                                    table._addPropDef(d);
                                }
                                Prop<float> prop = new Prop<float>(null, prop_def, 0f);
                                prop.set(((float)(double)data_value));
                                prop_set._addProp(data_name, prop);
                            }
                            break;
                        case 3:
                            {
                                PropDef prop_def = table.getPropDef(data_name);
                                if (prop_def == null)
                                {
                                    PropDef d = new PropDef(data_name, typeof(string), false);
                                    table._addPropDef(d);
                                }
                                Prop<string> prop = new Prop<string>(null, prop_def, "");
                                prop.set((string)data_value);
                                prop_set._addProp(data_name, prop);
                            }
                            break;
                    }
                }

                IProp prop_id = prop_set.getProp("Id");
                if (prop_id == null)
                {
                    EbLog.Error("EbDataMgr._loadTable() Error! Key=Id not exist, TableName=" + table_name);
                    continue;
                }
                Prop<int> p = (Prop<int>)prop_id;
                prop_set.Id = data_id;
                table._addPropSet(prop_set);
            }

            mDb._addTable(table);
        }
    }
}

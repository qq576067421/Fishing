using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GF.Common
{
    public class EbScriptMgr
    {
        //---------------------------------------------------------------------
        static EbScriptMgr mScriptMgr;
        // key=file_name，短文件名，不含扩展名，value=文件内容
        Dictionary<string, string> mMapFile = new Dictionary<string, string>();

        //-------------------------------------------------------------------------
        static public EbScriptMgr Instance
        {
            get { return mScriptMgr; }
        }

        //-------------------------------------------------------------------------
        public EbScriptMgr()
        {
            mScriptMgr = this;
        }

        //---------------------------------------------------------------------
        public void create(string dir_script)
        {
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
        }

        //---------------------------------------------------------------------
        public void setValue(string name, object v)
        {
        }

        //---------------------------------------------------------------------
        public void clearAllValue()
        {
        }

        //---------------------------------------------------------------------
        public object doFile(string file_name)
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EventDataXML
{
    public class DeleteHistory
    {
        public string path;
        public string path2;
        public string traceinfo;
    }

    public static class StackTrace_File
    {
        public static void Delete(string path)
        {
            StackTrace trace = new StackTrace();
            DeleteHistory delete = new DeleteHistory();
            delete.path = path;
            delete.path2 = "";
            delete.traceinfo = trace.ToString();
            historys.Add(delete);
            System.IO.File.Delete(path);
        }

        public static void Move(string srcpath, string destpath)
        {
            StackTrace trace = new StackTrace();
            DeleteHistory delete = new DeleteHistory();
            delete.path = srcpath;
            delete.path2 = destpath;
            delete.traceinfo = trace.ToString();
            historys.Add(delete);
            System.IO.File.Move(srcpath, destpath);
        }

        public static List<DeleteHistory> historys = new List<DeleteHistory>();
    }

    public static class StackTrace_Path
    {
        public static void Delete(string path)
        {
            StackTrace trace = new StackTrace();
            DeleteHistory delete = new DeleteHistory();
            delete.path = path;
            delete.path2 = "";
            delete.traceinfo = trace.ToString();
            historys.Add(delete);
            System.IO.Directory.Delete(path);
        }

        public static void Move(string srcpath, string destpath)
        {
            StackTrace trace = new StackTrace();
            DeleteHistory delete = new DeleteHistory();
            delete.path = srcpath;
            delete.path2 = destpath;
            delete.traceinfo = trace.ToString();
            historys.Add(delete);
            System.IO.Directory.Move(srcpath, destpath);
        }

        public static List<DeleteHistory> historys = new List<DeleteHistory>();
    }
}

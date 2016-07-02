using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct DataFile
{
    public string datafile_local_path;
    public string datafile_remote_url;
}

public class AutoPatcherDataFileList
{
    //-------------------------------------------------------------------------
    WWW mWWW;
    float mWWWTimeOut = 0f;
    Queue<DataFile> mQueDataFile;
    DataFile mCurrentDataFile;
    byte[] mBuffer = new byte[1024 * 1024];

    //-------------------------------------------------------------------------    
    public bool Finished { get { return mQueDataFile.Count == 0 && mWWW == null; } }
    public float CurrentLoadProgress { get { return (mWWW == null) ? 0f : mWWW.progress; } }
    public string CurrentDataFile { get; private set; }
    public int TotalCount { get; private set; }
    public int CurrentIndex { get; private set; }

    //-------------------------------------------------------------------------
    public AutoPatcherDataFileList()
    {
        mQueDataFile = new Queue<DataFile>();
        CurrentDataFile = "";
        TotalCount = 0;
        CurrentIndex = 0;
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mWWW == null && mQueDataFile.Count > 0)
        {
            mCurrentDataFile = mQueDataFile.Dequeue();
            mWWW = new WWW(mCurrentDataFile.datafile_remote_url);
            CurrentDataFile = Path.GetFileName(mCurrentDataFile.datafile_local_path);
            CurrentIndex = TotalCount - mQueDataFile.Count;
        }

        if (mWWW == null) return;

        if (mWWW.isDone)
        {
            string dir = Path.GetDirectoryName(mCurrentDataFile.datafile_local_path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (string.IsNullOrEmpty(mWWW.error))
            {
                using (MemoryStream ms = new MemoryStream(mWWW.bytes))
                {
                    using (FileStream fs = new FileStream(mCurrentDataFile.datafile_local_path, FileMode.Create))
                    {
                        ms.WriteTo(fs);
                    }
                }
            }
            else
            {
                EbLog.Error(mWWW.error);
            }

            mWWW = null;
        }
        else
        {
            // WWW有时会不下载，此处记录超时并重新下载
            if (mWWW.progress == 0)
            {
                mWWWTimeOut += elapsed_tm;
                if (mWWWTimeOut > 5f)
                {
                    mWWWTimeOut = 0f;
                    mWWW = new WWW(mCurrentDataFile.datafile_remote_url);
                }
            }
            else
            {
                mWWWTimeOut = 0f;
            }
        }
    }

    //-------------------------------------------------------------------------
    public void addFile4Download(string remote_url_prefix, Dictionary<string, string> map_newfile)
    {
        foreach (var i in map_newfile)
        {
            DataFile data_file;
            data_file.datafile_local_path = AutoPatcherPath.combinePersistentDataPath(i.Key);
            data_file.datafile_remote_url = remote_url_prefix + i.Key;

            mQueDataFile.Enqueue(data_file);
        }

        TotalCount = mQueDataFile.Count;
    }
}

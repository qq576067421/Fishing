using UnityEngine;
using System;

public class PathMgr
{
    //-------------------------------------------------------------------------
    string mMediaRootPath = "/Media/Fishing/";

    //-------------------------------------------------------------------------
    public string getNodeSysMediaPath()
    {
        return StreamingAssetsPathHelper.filePath(mMediaRootPath + "NodeSys");
    }

    //-------------------------------------------------------------------------
    public string getConfigMediaPath()
    {
        return StreamingAssetsPathHelper.filePath(mMediaRootPath + "Config");
    }

    //-------------------------------------------------------------------------
    public string getWWWPersistentDataPath()
    {
        string persistent_path =
#if UNITY_STANDALONE_WIN && UNITY_EDITOR
 "file:///" + Application.persistentDataPath + "/PC";
#elif UNITY_ANDROID&&UNITY_EDITOR
 "file:///" + Application.persistentDataPath + "/ANDROID";
#elif UNITY_IPHONE&&UNITY_EDITOR
"file:///" + Application.persistentDataPath+ "/IOS";
#elif UNITY_ANDROID
 "file:///" + Application.persistentDataPath;
#elif UNITY_IPHONE
"file:///" + Application.persistentDataPath;
#else
 string.Empty;
#endif
        return persistent_path;
    }

    //-------------------------------------------------------------------------
    public string combineWWWPersistentDataPath(string path)
    {
        return getWWWPersistentDataPath() + path;
    }

    //-------------------------------------------------------------------------
    public string getPersistentDataPath()
    {
        string persistent_path =
#if UNITY_STANDALONE_WIN && UNITY_EDITOR
 Application.persistentDataPath + "/PC";
#elif UNITY_ANDROID && UNITY_EDITOR
 Application.persistentDataPath + "/ANDROID";
#elif UNITY_IPHONE && UNITY_EDITOR
Application.persistentDataPath+ "/IOS";
#elif UNITY_ANDROID
 Application.persistentDataPath;
#elif UNITY_IPHONE
Application.persistentDataPath;
#else
 string.Empty;
#endif
        return persistent_path;
    }

    //-------------------------------------------------------------------------
    public string combinePersistentDataPath(string path)
    {
        return getPersistentDataPath() + path;
    }
}

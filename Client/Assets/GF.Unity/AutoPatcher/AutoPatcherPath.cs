using UnityEngine;
using System;

public class AutoPatcherPath
{
    //-------------------------------------------------------------------------
    public static string getWWWPersistentDataPath()
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
    public static string combineWWWPersistentDataPath(string path)
    {
        return getWWWPersistentDataPath() + path;
    }

    //-------------------------------------------------------------------------
    public static string getPersistentDataPath()
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
    public static string combinePersistentDataPath(string path)
    {
        return getPersistentDataPath() + path;
    }

    //-------------------------------------------------------------------------
    public static string wwwPath(string name)
    {
        if (string.IsNullOrEmpty(name)) { name = ""; }
        if (_isPathSeparator(name[0])) name = "/" + name;

        string filepath = "";

#if UNITY_EDITOR
        filepath = @"file://" + Application.streamingAssetsPath + name;
#elif UNITY_IPHONE
        filepath = @"file://" + Application.streamingAssetsPath + name;
#elif UNITY_ANDROID
        filepath = @"file://" + Application.persistentDataPath + "/assets" + name;
#endif

        return filepath;
    }

    //-------------------------------------------------------------------------
    public static string filePath(string file_name)
    {
        if (string.IsNullOrEmpty(file_name)) { file_name = ""; }
        if (_isPathSeparator(file_name[0])) file_name = "/" + file_name;

        string filepath = "";

#if UNITY_ANDROID && !UNITY_EDITOR
        filepath = Application.persistentDataPath + "/assets" + file_name;
#else
        filepath = Application.streamingAssetsPath + file_name;
#endif

        return filepath;
    }

    //-----------------------------------------------------------------------------
    static bool _isPathSeparator(char c)
    {
        return c != '/' && c != '\\';
    }
}

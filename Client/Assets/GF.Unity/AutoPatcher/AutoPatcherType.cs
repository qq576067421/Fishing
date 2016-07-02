using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
public enum AutoPatcherResult : short
{
    Success = 0,// 成功
    Failed = 10,// 失败
    Timeout = 20,// 超时
}

//-----------------------------------------------------------------------------
public enum AutoPatcherServerState
{
    Running = 0,// 运行中
    Maintenance,// 维护中
}

//-----------------------------------------------------------------------------
public class AutoPatcherConfig
{
    public string RemoteVersionInfoUrl { get; set; }// 远程VersionInfo Url
}

//-----------------------------------------------------------------------------
public class AutoPatcherStringDef
{
    public static string PlayerPrefsKeyLocalVersionInfo
    {
        get
        {
            string key = "LocalVersionInfo";
#if UNITY_ANDROID
            key+="_Android";
#elif UNITY_IPHONE
            key+="_iOS";
#else
            key += "_Pc";
#endif
            return key;
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

public class EditorGF : EditorWindow
{
    //-------------------------------------------------------------------------
    static MD5 mMD5;
    static string mBundleVersion;
    static string mDataVersion;
    static string mResourcesPath;
    static string mTargetPath;
    static bool mCopyOrDelete;
    static string mDataVersionKey = "DataVersionKey";
    static string mResourcesPathKey = "ResourcesPathKey";
    static string mTargetPathKey = "TargetPathKey";
    static string mNotPackAssetPath;
    string mPackInfoTextName = "PackInfo.txt";
    string mDataTargetPath;
    List<string> mDoNotPackFileExtention = new List<string> { ".meta" };
    const string mNotPackAsset = "NotPackAsset";

    //-------------------------------------------------------------------------
    [MenuItem("GF/AutoPatcher")]
    static void autoPatcher()
    {
        var ui_wnd = EditorWindow.GetWindow<UiWndAutoPatcher>("自动更新");
    }

    //-------------------------------------------------------------------------
    [MenuItem("GF/BuildAssetBundles ver5.3")]
    static void BuildAssetBundles()
    {
        string p = Path.Combine(Application.persistentDataPath, "../Dragon/");
        string path_root = Path.GetFullPath(p);

        Caching.CleanCache();

        List<string> list_file = new List<string>(100);
        string[] arr_file = Directory.GetFiles("Assets/NeedPackAsset/Actor/Prefab/");
        foreach (var i in arr_file)
        {
            if (!i.EndsWith(".prefab")) continue;
            list_file.Add(i);
        }

        string path_actor = path_root + "Android/Actor/";
        if (!Directory.Exists(path_actor))
        {
            Directory.CreateDirectory(path_actor);
        }

        foreach (var i in list_file)
        {
            var names = AssetDatabase.GetDependencies(i);

            AssetBundleBuild abb;
            abb.assetBundleName = Path.GetFileNameWithoutExtension(i) + ".assetbundle";
            abb.assetNames = new string[names.Length];
            abb.assetBundleVariant = "";
            int asset_index = 0;
            foreach (var j in names)
            {
                Debug.Log("Asset: " + j);
                if (j.EndsWith(".cs")) continue;
                abb.assetNames[asset_index++] = j;
            }

            AssetBundleBuild[] arr_abb = new AssetBundleBuild[1];
            arr_abb[0] = abb;
            BuildPipeline.BuildAssetBundles(
            path_actor,
            arr_abb,
            BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.Android);
        }
    }

    //-------------------------------------------------------------------------
    [MenuItem("GF/ChangeLocalVersionConfig")]
    static void ChangeLocalVersionConfig()
    {
        TextAsset config = Resources.Load<TextAsset>("Config/VersionInfoConfig");
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(config.text);

        string platform = "";
        XmlNode node_info = null;

#if UNITY_STANDALONE_WIN
        platform = "VersionInfoPC";
        node_info = xml.SelectSingleNode("VersionInfoConfig/" + platform);
#elif UNITY_ANDROID
        platform = "VersionInfoAndroid";
        node_info = xml.SelectSingleNode("VersionInfoConfig/" + platform);
#elif UNITY_IPHONE
        platform = "VersionInfoIOS";
        node_info= xml.SelectSingleNode("VersionInfoConfig/"+platform);  
#endif

        string bundle_version = node_info.Attributes["BundleVersion"].Value;
        string new_bundle_version = (int.Parse(bundle_version.Replace(".", "")) + 1).ToString();
        new_bundle_version = new_bundle_version.Insert(1, ".").Insert(4, ".");
        string data_version = node_info.Attributes["DataVersion"].Value;
        string[] texts = File.ReadAllLines(AssetDatabase.GetAssetPath(config));
        string[] new_texts = new string[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            string str = texts[i];
            if (str.Contains(platform))
            {
                str = str.Replace(bundle_version, new_bundle_version);
            }
            new_texts[i] = str;
        }
        File.WriteAllLines(AssetDatabase.GetAssetPath(config), new_texts);

        Debug.Log("修改完成");
    }

    //-------------------------------------------------------------------------
    void OnGUI()
    {
        mBundleVersion = PlayerSettings.bundleVersion;
        EditorGUILayout.LabelField("程序包版本号:", mBundleVersion);
        mDataVersion = EditorGUILayout.TextField("AssetBundle版本号:", mDataVersion);
        mResourcesPath = EditorGUILayout.TextField("资源所在路径:", mResourcesPath);
        mTargetPath = EditorGUILayout.TextField("目标路径:", mTargetPath);
        mCopyOrDelete = EditorGUILayout.Toggle("复制或删除本地资源", mCopyOrDelete);
        bool check_path = GUILayout.Button("重设路径", GUILayout.Width(200));
        if (check_path)
        {
            _checkPath();
        }

        bool click_build_asset = GUILayout.Button("打AssetBundle包(压缩)", GUILayout.Width(200));
        if (click_build_asset)
        {
            _packAssetBundleCompress();
        }

        bool click_btn = GUILayout.Button("处理存在的AssetBundle包", GUILayout.Width(200));
        if (click_btn)
        {
            _packResources();
        }

        bool clear_local_version = GUILayout.Button("清理本地保存的数据版本", GUILayout.Width(200));
        if (clear_local_version)
        {
            //PlayerPrefs.DeleteKey(VersionConfig.mDataVersionKey);
        }
    }

    //-------------------------------------------------------------------------
    void _packResources()
    {
        PlayerPrefs.SetString(mDataVersionKey, mDataVersion);
        PlayerPrefs.SetString(mResourcesPathKey, mResourcesPath);
        PlayerPrefs.SetString(mTargetPathKey, mTargetPath);
        mDataTargetPath = mTargetPath + "/DataVersion_" + mDataVersion;

        if (Directory.Exists(mResourcesPath))
        {
            if (!Directory.Exists(mTargetPath))
            {
                Directory.CreateDirectory(mTargetPath);
            }

            if (!Directory.Exists(mDataTargetPath))
            {
                Directory.CreateDirectory(mDataTargetPath);
            }

            StreamWriter sw;
            string info = mDataTargetPath + "/" + mPackInfoTextName;

            if (!File.Exists(info))
            {
                sw = File.CreateText(info);
            }
            else
            {
                sw = new StreamWriter(info);
            }

            using (sw)
            {
                _checkPackInfo(sw, mResourcesPath);
            }

            ShowNotification(new GUIContent("打包完成!"));
        }
        else
        {
            Debug.LogError("不存在该资源文件夹");
        }
    }

    //-------------------------------------------------------------------------
    void _checkPackInfo(StreamWriter sw, string path)
    {
        string[] files = Directory.GetFiles(path);
        foreach (var i in files)
        {
            string file_extension = Path.GetExtension(i);
            if (mDoNotPackFileExtention.Contains(file_extension))
            {
                continue;
            }

            string file_name = Path.GetFileName(i);
            string file_directory = Path.GetDirectoryName(i);
            string target_path = file_directory.Replace(mResourcesPath, "");
            target_path = target_path.Replace(@"\", "/");
            string file_path = i;
            bool is_apk = false;

            if (file_extension.Equals(".apk"))
            {
                is_apk = true;
                file_path = file_directory + mBundleVersion + file_extension;
                if (!File.Exists(file_path))
                {
                    FileInfo file_info = new FileInfo(i);
                    file_info.MoveTo(file_path);
                }
                file_name = mBundleVersion + file_extension;
                File.Copy(file_path, mTargetPath + "/" + target_path + "/" + file_name, true);
            }
            else
            {
                string target_p = mDataTargetPath + "/" + target_path;
                if (!Directory.Exists(target_p))
                {
                    Directory.CreateDirectory(target_p);
                }

                File.Copy(file_path, target_p + "/" + file_name, true);
            }

            if (!is_apk)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(target_path + "/" + file_name + " ");

                using (FileStream sr = File.OpenRead(file_path))
                {
                    byte[] new_bytes = mMD5.ComputeHash(sr);
                    foreach (var bytes in new_bytes)
                    {
                        sb.Append(bytes.ToString("X2"));
                    }
                }

                sw.WriteLine(sb.ToString());
            }
        }

        string[] directorys = Directory.GetDirectories(path);
        foreach (var i in directorys)
        {
            _checkPackInfo(sw, i);
        }
    }

    //-------------------------------------------------------------------------
    void _copyOrDeleteToClient(string file_path, string directory_name, string file_name)
    {
        string persistent_data_path =
#if UNITY_STANDALONE_WIN && UNITY_EDITOR
        Application.persistentDataPath + "/Pc" + directory_name;
#elif UNITY_ANDROID && UNITY_EDITOR
        Application.persistentDataPath + "/Android" + directory_name;
#elif UNITY_IPHONE&& UNITY_EDITOR
        Application.persistentDataPath  + "/iOS" + directory_name;
#else
        Application.persistentDataPath + directory_name;
#endif

        if (!Directory.Exists(persistent_data_path))
        {
            Directory.CreateDirectory(persistent_data_path);
        }

        persistent_data_path += "/" + file_name;

        if (mCopyOrDelete)
        {
            File.Copy(file_path, persistent_data_path, true);
        }
        else
        {
            if (File.Exists(persistent_data_path))
            {
                File.Delete(persistent_data_path);
            }
        }
    }

    //-------------------------------------------------------------------------
    static void _checkPath()
    {
        string current_dir = System.Environment.CurrentDirectory;
        mNotPackAssetPath = current_dir + "/Assets/" + mNotPackAsset;
        string split_sign = "";
#if UNITY_ANDROID
        split_sign = @"\";
#elif UNITY_IPHONE
        split_sign= @"/";
#endif
        current_dir = current_dir.Substring(0, current_dir.LastIndexOf(split_sign));
        if (PlayerPrefs.HasKey(mDataVersionKey))
        {
            mDataVersion = PlayerPrefs.GetString(mDataVersionKey);

            if (string.IsNullOrEmpty(mDataVersion))
            {
                mDataVersion = "1.00.000";
            }

            string data_version = (int.Parse(mDataVersion.Replace(".", "")) + 1).ToString();
            data_version = data_version.Insert(1, ".").Insert(4, ".");
            mDataVersion = data_version;
        }
        else
        {
            mDataVersion = "1.00.000";
        }

        mResourcesPath =
#if UNITY_STANDALONE_WIN
        current_dir + "/ClientDataSources/PC";
#elif UNITY_ANDROID
        current_dir + "/ClientDataSources/ANDROID";
#elif UNITY_IPHONE
        current_dir + "/ClientDataSources/IOS";
#endif

        mTargetPath =
#if UNITY_STANDALONE_WIN
        current_dir + "/ClientDataServerUse/PC";
#elif UNITY_ANDROID
        current_dir + "/ClientDataServerUse/ANDROID";
#elif UNITY_IPHONE
        current_dir + "/ClientDataServerUse/IOS";
#endif
    }

    //-------------------------------------------------------------------------
    void _packAssetBundleCompress()
    {
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        string source_path_first = AssetDatabase.GetAssetPath(selection[0]);
        string path_root = "";
        if (File.Exists(source_path_first))
        {
            string directory_name = Path.GetDirectoryName(source_path_first);
            if (directory_name.Contains("Assets"))
            {
                directory_name = directory_name.Replace("Assets", "");
            }
            directory_name = directory_name.Substring(0, directory_name.IndexOf('/'));
            path_root = mResourcesPath + directory_name;
            if (Directory.Exists(path_root))
            {
                string[] directorys = Directory.GetDirectories(path_root);
                foreach (var i in directorys)
                {
                    Directory.Delete(i, true);
                }
            }
        }

        foreach (var obj in selection)
        {
            string source_path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(source_path))
            {
                string directory_name = Path.GetDirectoryName(source_path);
                if (directory_name.Contains("Assets"))
                {
                    directory_name = directory_name.Replace("Assets", "");
                }
                string path = mResourcesPath + directory_name;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path += "/" + obj.name + ".assetbundle";

#if UNITY_STANDALONE_WIN
                _buildAssetBundleCompressed(null, obj, path, BuildTarget.StandaloneWindows64, false);
#elif UNITY_IPHONE
                _buildAssetBundleCompressed(null, obj, path, BuildTarget.iPhone, false);
#elif UNITY_ANDROID
                _buildAssetBundleCompressed(null, obj, path, BuildTarget.Android, false);
#endif
            }
        }

        if (Directory.Exists(mNotPackAssetPath))
        {
            _copyFile(mNotPackAssetPath);
        }

        Debug.Log("裸资源复制完毕!");
    }

    //-------------------------------------------------------------------------
    static void _buildAssetBundleCompressed(Object[] selection, Object singele_obj, string path, BuildTarget target, bool build_all = true)
    {
        //if (build_all)
        //{
        //    BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path,
        //      BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, target);
        //}
        //else
        //{
        //    BuildPipeline.BuildAssetBundle(singele_obj, null, path, BuildAssetBundleOptions.CollectDependencies, target);
        //}

        //EbLog.Note("Build AssetBundle BuildTarget=" + target);
    }

    //-------------------------------------------------------------------------
    void _copyFile(string path)
    {
        string[] files = Directory.GetFiles(path);
        foreach (var i in files)
        {
            if (!File.Exists(i))
            {
                continue;
            }

            string file_extension = Path.GetExtension(i);
            if (mDoNotPackFileExtention.Contains(file_extension))
            {
                continue;
            }

            string current_dir = System.Environment.CurrentDirectory;
            string asset_path = current_dir + "/Assets/";
            string file_name = Path.GetFileName(i);
            string file_directory = Path.GetDirectoryName(i);
            string target_path = file_directory.Replace(asset_path, "");
            target_path = target_path.Replace(@"\", "/");
            string file_path = i;
            string target_p = mResourcesPath + "/" + target_path;
            if (!Directory.Exists(target_p))
            {
                Directory.CreateDirectory(target_p);
            }
            File.Copy(file_path, target_p + "/" + file_name, true);
        }

        string[] directorys = Directory.GetDirectories(path);
        foreach (var i in directorys)
        {
            _copyFile(i);
        }
    }
}

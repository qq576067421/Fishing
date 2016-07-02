using System;
using System.Collections.Generic;
using UnityEngine;

public class AsyncLoadAssetMgr
{
    //-------------------------------------------------------------------------
    Dictionary<AsyncLoadAssetResult, AsyncResourcesInfo> mMapAsyncResourcesRequest;
    Dictionary<AsyncLoadAssetResult, AsyncAssetBundleInfo> mMapAsyncAssetBundleRequest;
    Dictionary<AsyncLoadAssetResult, AsyncAssetBundleInfo> mMapAsyncAssetBundle;
    Dictionary<AsyncLoadAssetResult, AsyncAssetInfo> mMapAsyncAssetRequest;    
    Dictionary<string, AssetBundle> mMapAsyncAssetBundlePath;
    Queue<AsyncLoadAssetResult> mQueueLoadedResources;
    Queue<AsyncLoadAssetResult> mQueueLoadedAssetBundle;
    Queue<AsyncLoadAssetResult> mQueueLoadedAsset;

    //-------------------------------------------------------------------------
    public void init()
    {
        mMapAsyncResourcesRequest = new Dictionary<AsyncLoadAssetResult, AsyncResourcesInfo>();
        mMapAsyncAssetBundleRequest = new Dictionary<AsyncLoadAssetResult, AsyncAssetBundleInfo>();
        mMapAsyncAssetBundle = new Dictionary<AsyncLoadAssetResult, AsyncAssetBundleInfo>();
        mMapAsyncAssetRequest = new Dictionary<AsyncLoadAssetResult, AsyncAssetInfo>();        
        mMapAsyncAssetBundlePath = new Dictionary<string, AssetBundle>();
        mQueueLoadedResources = new Queue<AsyncLoadAssetResult>();
        mQueueLoadedAssetBundle = new Queue<AsyncLoadAssetResult>();
        mQueueLoadedAsset = new Queue<AsyncLoadAssetResult>();
    }

    //-------------------------------------------------------------------------
    public void destory()
    {
        foreach (var i in mMapAsyncAssetBundlePath)
        {
            if (i.Value != null)
            {
                i.Value.Unload(false);
            }
        }
        foreach (var i in mMapAsyncAssetBundle)
        {
            if (i.Value.asset != null)
            {
                i.Value.asset.Unload(false);
            }
        }
        mMapAsyncAssetBundle.Clear();
        mMapAsyncAssetBundle = null;
        mMapAsyncResourcesRequest.Clear();
        mMapAsyncResourcesRequest = null;
        mMapAsyncAssetBundleRequest.Clear();
        mMapAsyncAssetBundleRequest = null;
        mMapAsyncAssetBundlePath.Clear();
        mMapAsyncAssetBundlePath = null;
        mQueueLoadedResources.Clear();
        mQueueLoadedResources = null;
        mQueueLoadedAssetBundle.Clear();
        mQueueLoadedAssetBundle = null;
    }

    //-------------------------------------------------------------------------
    public void clearAsset()
    {
        foreach (var i in mMapAsyncAssetBundlePath)
        {
            if (i.Value != null)
            {
                i.Value.Unload(false);
            }
        }
        foreach (var i in mMapAsyncAssetBundle)
        {
            if (i.Value.asset != null)
            {
                i.Value.asset.Unload(false);
            }
        }
        mMapAsyncAssetBundle.Clear();
        mMapAsyncResourcesRequest.Clear();
        mMapAsyncAssetBundleRequest.Clear();
        mMapAsyncAssetBundlePath.Clear();
        mQueueLoadedResources.Clear();
        mQueueLoadedAssetBundle.Clear();
    }

    //-------------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        foreach (var i in mMapAsyncResourcesRequest)
        {
            AsyncResourcesInfo resource_info = i.Value;
            if (resource_info.resource_request.isDone)
            {
                resource_info.asset_loaded(resource_info.loader_result, null, resource_info.resource_request.asset);
                mQueueLoadedResources.Enqueue(i.Key);
            }
        }

        while (mQueueLoadedResources.Count > 0)
        {
            AsyncLoadAssetResult key = mQueueLoadedResources.Dequeue();
            if (mMapAsyncResourcesRequest.ContainsKey(key))
            {
                mMapAsyncResourcesRequest.Remove(key);
            }
        }

        foreach (var i in mMapAsyncAssetBundleRequest)
        {
            AsyncAssetBundleInfo resource_info = i.Value;
            if (resource_info.asset == null)
            {
                AssetBundle asset = null;
                mMapAsyncAssetBundlePath.TryGetValue(resource_info.asset_path, out asset);
                if (asset == null)
                {
                    if (resource_info.asset_www != null && resource_info.asset_www.isDone)
                    {
                        if (string.IsNullOrEmpty(resource_info.asset_www.error))
                        {
                            resource_info.asset = resource_info.asset_www.assetBundle;
                            mMapAsyncAssetBundle[i.Key] = resource_info;
                            mMapAsyncAssetBundlePath[resource_info.asset_path] = resource_info.asset_www.assetBundle;
                            resource_info.asset_www = null;
                        }
                        else
                        {
                            Debug.LogError(resource_info.asset_www.error);
                            mMapAsyncAssetBundle[i.Key] = resource_info;
                            mQueueLoadedAssetBundle.Enqueue(i.Key);
                        }
                    }
                }
                else
                {
                    resource_info.asset = asset;
                    mMapAsyncAssetBundle[i.Key] = resource_info;
                }
            }
            else
            {
                if (resource_info.asset_www != null)
                {
                    resource_info.asset_www = null;
                }
                mMapAsyncAssetBundle[i.Key] = resource_info;
            }
        }

        foreach (var i in mMapAsyncAssetBundle)
        {
            AsyncAssetBundleInfo resource_info = i.Value;
            if (mMapAsyncAssetBundleRequest.ContainsKey(i.Key))
            {
                mMapAsyncAssetBundleRequest.Remove(i.Key);
            }
            if (resource_info.asset != null)
            {
                AssetBundleRequest assetbundle_request = resource_info.asset.LoadAssetAsync(resource_info.asset_name, resource_info.t);

                if (assetbundle_request.isDone)
                {
                    resource_info.asset_loaded(resource_info.loader_result, i.Value.asset, assetbundle_request.asset);
                    mQueueLoadedAssetBundle.Enqueue(i.Key);
                }
            }
        }

        while (mQueueLoadedAssetBundle.Count > 0)
        {
            AsyncLoadAssetResult key = mQueueLoadedAssetBundle.Dequeue();
            if (mMapAsyncAssetBundle.ContainsKey(key))
            {
                mMapAsyncAssetBundle.Remove(key);
            }
        }

        foreach (var i in mMapAsyncAssetRequest)
        {
            AsyncAssetInfo resource_info = i.Value;

            if (resource_info.asset_www != null && resource_info.asset_www.isDone)
            {
                if (string.IsNullOrEmpty(resource_info.asset_www.error))
                {                                        
                    resource_info.asset_loaded(resource_info.loader_result, null, resource_info.asset_www.texture);
                    resource_info.asset_www.Dispose();                    
                }
                else
                {
                    Debug.LogError(resource_info.asset_www.error);
                }
                mQueueLoadedAsset.Enqueue(i.Key);
            }
        }

        while (mQueueLoadedAsset.Count > 0)
        {
            AsyncLoadAssetResult key = mQueueLoadedAsset.Dequeue();
            if (mMapAsyncAssetRequest.ContainsKey(key))
            {
                mMapAsyncAssetRequest.Remove(key);
            }
        }
    }

    //-------------------------------------------------------------------------
    public AsyncLoadAssetResult asyncLoadResources<T>(string id, string resource_path, string resource_name, delegateAssetLoaded call_back) where T : UnityEngine.Object
    {
        AsyncLoadAssetResult result = new AsyncLoadAssetResult();
        result.is_done = false;
        result.id = id;
        AsyncResourcesInfo async_info;
        async_info.id = id;
        async_info.resource_name = resource_name;
        async_info.resource_path = resource_path;
        async_info.resource_request = Resources.LoadAsync<T>(async_info.resource_path);
        async_info.t = typeof(T);
        async_info.asset_loaded = call_back;
        async_info.loader_result = result;
        mMapAsyncResourcesRequest[result] = async_info;
        return result;
    }

    //-------------------------------------------------------------------------
    public AsyncLoadAssetResult asyncLoadAssetBundle<T>(string id, string resource_path, string resource_name, delegateAssetLoaded call_back) where T : UnityEngine.Object
    {
        resource_path += ".assetbundle";
        AsyncLoadAssetResult result = new AsyncLoadAssetResult();
        result.is_done = false;
        result.id = id;
        AsyncAssetBundleInfo async_info;
        async_info.id = id;
        async_info.asset_name = resource_name;
        async_info.asset_path = resource_path;
        AssetBundle asset = null;
        WWW asset_www = null;
        mMapAsyncAssetBundlePath.TryGetValue(resource_path, out asset);
        if (asset == null)
        {
            asset_www = WWW.LoadFromCacheOrDownload(resource_path, 0);
        }
        async_info.asset_www = asset_www;
        async_info.asset = asset;
        async_info.t = typeof(T);
        async_info.asset_loaded = call_back;
        async_info.loader_result = result;
        mMapAsyncAssetBundleRequest[result] = async_info;
        return result;
    }

    //-------------------------------------------------------------------------
    public AsyncLoadAssetResult asyncLoadAsset<T>(string id, string resource_path, string resource_name, delegateAssetLoaded call_back) where T : UnityEngine.Object
    {
        AsyncLoadAssetResult result = new AsyncLoadAssetResult();
        result.is_done = false;
        result.id = id;
        AsyncAssetInfo async_info;
        async_info.id = id;
        async_info.asset_name = resource_name;
        async_info.asset_path = resource_path;
        WWW asset_www = new WWW(resource_path);
        async_info.asset_www = asset_www;
        async_info.t = typeof(T);
        async_info.asset_loaded = call_back;
        async_info.loader_result = result;
        mMapAsyncAssetRequest[result] = async_info;
        return result;
    }
}

//-------------------------------------------------------------------------
public struct AsyncResourcesInfo
{
    public string id;
    public Type t;
    public string resource_path;
    public string resource_name;
    public ResourceRequest resource_request;
    public AsyncLoadAssetResult loader_result;
    public delegateAssetLoaded asset_loaded;
}

//-------------------------------------------------------------------------
public struct AsyncAssetBundleInfo
{
    public string id;
    public Type t;
    public string asset_path;
    public string asset_name;
    public WWW asset_www;
    public AssetBundle asset;
    public AsyncLoadAssetResult loader_result;
    public delegateAssetLoaded asset_loaded;
}

//-------------------------------------------------------------------------
public struct AsyncAssetInfo
{
    public string id;
    public Type t;
    public string asset_path;
    public string asset_name;
    public WWW asset_www;
    public AsyncLoadAssetResult loader_result;
    public delegateAssetLoaded asset_loaded;
}

//-------------------------------------------------------------------------
public class AsyncLoadAssetResult
{
    public bool is_done;
    public string id;
}

public delegate void delegateAssetLoaded(AsyncLoadAssetResult loader_result, AssetBundle asset_bundle, UnityEngine.Object obj);
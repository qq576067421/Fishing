using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Ps;

public class StillSpriteLoader<T> where T : StillSprite
{
    //-------------------------------------------------------------------------
#if UNITY_EDITOR
    GameObject mTKGameObject = null;
#endif

    //-------------------------------------------------------------------------
    public StillSpriteLoader()
    {
#if UNITY_EDITOR
        mTKGameObject = GameObject.Find("TKGameObject");
#endif
    }

    //-------------------------------------------------------------------------
    public T loadSpriteFromPrefab(string prefab_name, CRenderScene scene)
    {
        GameObject game_object = GameObject.Instantiate(_loadPrefabFromFile(prefab_name)) as GameObject;
        T still_sprite = game_object.GetComponent<T>();
        still_sprite.init(scene);

#if UNITY_EDITOR
        still_sprite.transform.parent = mTKGameObject.transform;
#endif
        return still_sprite;
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
    }

    //-------------------------------------------------------------------------
    UnityEngine.Object _loadPrefabFromFile(string file_name)
    {
        UnityEngine.Object prefab_object = Resources.Load(file_name);
        _assertPrefabIsNull(prefab_object, file_name);
        return prefab_object;
    }

    //-------------------------------------------------------------------------
    void _assertPrefabIsNull(UnityEngine.Object prefab_object, string file_name)
    {
        if (prefab_object != null) return;
        Debug.LogError("loadStillSprite error :: not found the file \"" + file_name + "\"");
    }
}
#define TK2D_AUTOREBUILD_REBUILD_ONSAVE

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

#if TK2D_AUTOREBUILD_REBUILD_ONSAVE
public class RespectReadOnly : UnityEditor.AssetModificationProcessor
{
	static void HandleSceneSave()
	{
		tk2dAutoRebuild.TriggerBuild();
	}

	static bool IsScene(string path)
	{
		return System.IO.Path.GetExtension(path).ToLower() == ".unity";
	}

	public static void OnWillCreateAsset (string path)
	{
		if (IsScene(path))
		{
			HandleSceneSave();			
		}
	}

	public static string[] OnWillSaveAssets (string[] paths)
	{
		foreach (string s in paths)
		{
			if (IsScene(s))
			{
				HandleSceneSave();
			}
		}

		return paths;
	}
}
#endif

[InitializeOnLoad]
public static class tk2dAutoRebuild
{
	const int rebuildWaitCount = 2;
	static int waitCounter = -1;

	static tk2dAutoRebuild()
	{
		EditorApplication.playmodeStateChanged += PlayModeStateChanged;
		EditorApplication.update += EditorUpdate;
		waitCounter = rebuildWaitCount; 
	}

	static void PlayModeStateChanged()
	{
		TriggerBuild();
	}

	public static void TriggerBuild()
	{
		waitCounter = rebuildWaitCount;
	}

	static void EditorUpdate()
	{
		if (--waitCounter == 0)
		{
			DoRebuild();
		}
	}

	static bool NeedRebuild(GameObject go)
	{
		MeshFilter mf = go.GetComponent<MeshFilter>();
		return mf != null && mf.sharedMesh == null;
	}

	static void DoRebuild()
	{
		tk2dBaseSprite[] allSprites = Object.FindObjectsOfType(typeof(tk2dBaseSprite)) as tk2dBaseSprite[];
		tk2dTextMesh[] allTextMeshes = Object.FindObjectsOfType(typeof(tk2dTextMesh)) as tk2dTextMesh[];
		tk2dStaticSpriteBatcher[] allBatchers = Object.FindObjectsOfType(typeof(tk2dStaticSpriteBatcher)) as tk2dStaticSpriteBatcher[];
		foreach (var t in allSprites) 		{ if (NeedRebuild(t.gameObject)) { t.ForceBuild(); } }
		foreach (var t in allTextMeshes) 	{ if (NeedRebuild(t.gameObject)) { t.ForceBuild(); } }
		foreach (var t in allBatchers) 		{ if (NeedRebuild(t.gameObject)) { t.ForceBuild(); } }
	}
}

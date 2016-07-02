using UnityEngine;
using System.Collections;

// Simple class to deal with Undo in Unity
// Some classes in tk2d attempt to store minimal state change
// Eg. Tilemap - On undo, the tilemap system rebuilds active meshes
// during which it will want to inhibit undos on the changes performed during
// that rebuild.
// It will be prohibitively expensive to cache undos for all the render meshes.
public static class tk2dUtil {

	// The name of all subsequent undos
#pragma warning disable 414
	static string label = "";
#pragma warning restore 414

	// This can get stuck if an exception is trigerred before it reset
	// BeginGroup will reset this flag for this reason
	static bool undoEnabled = false;
	static public bool UndoEnabled {
		get {
			return undoEnabled;
		}
		set {
			undoEnabled = value;
		}
	}
	
	public static void BeginGroup(string name) {
		undoEnabled = true;
		label = name;
	}

	public static void EndGroup() {
		label = "";
	}

	public static void DestroyImmediate( UnityEngine.Object obj ) {
		if (obj == null) {
			return;
		}

#if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (!Application.isPlaying && undoEnabled) {
			UnityEditor.Undo.DestroyObjectImmediate(obj);
		}
		else
#endif
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	public static GameObject CreateGameObject(string name) {
		GameObject go = new GameObject(name);
#if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (!Application.isPlaying && undoEnabled) {
			UnityEditor.Undo.RegisterCreatedObjectUndo(go, label);
		}
#endif
		return go;
	}

	public static Mesh CreateMesh() {
		Mesh mesh = new Mesh();
#if !UNITY_3_5
		mesh.MarkDynamic();
#endif
#if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (!Application.isPlaying && undoEnabled) {
			UnityEditor.Undo.RegisterCreatedObjectUndo(mesh, label);
		}
#endif
		return mesh;
	}

	public static T AddComponent<T>(GameObject go) where T : UnityEngine.Component {
		T t = go.AddComponent<T>();
#if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (!Application.isPlaying && undoEnabled) {
			UnityEditor.Undo.RegisterCreatedObjectUndo(t, label);
		}
#endif
		return t;
	}

#if !UNITY_3_5
	public static void SetActive( GameObject go, bool active ) {
		if (active == go.activeSelf) {
			return;
		}

#if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (!Application.isPlaying && undoEnabled) {
			UnityEditor.Undo.RegisterCompleteObjectUndo(go, label);
		}
#endif
		go.SetActive(active);
	}
#endif

	public static void SetTransformParent(Transform t, Transform parent) {
#if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (!Application.isPlaying && undoEnabled) {
			UnityEditor.Undo.SetTransformParent(t, parent, label);
		}
		else
#endif
		{
			t.parent = parent;
		}
	}

	// Replicate old pre-5.3 behaviour
	public static void SetDirty(UnityEngine.Object @object)
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(@object);

#if (UNITY_5_3 || UNITY_5_4 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9 || UNITY_6_0)
		if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(@object)))
		{
			string scenePath = UnityEditor.AssetDatabase.GetAssetOrScenePath(@object);
			var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
			if (scene.IsValid())
			{
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
			}
		}
#endif

#endif // UNITY_EDITOR
	}
}


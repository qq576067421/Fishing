using UnityEngine;
using UnityEditor;
using System.Collections;

// Undo wrappers to deal with differences in Unity 4.3
public static class tk2dUndo {
	public static void RecordObject( UnityEngine.Object obj, string name ) {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterUndo(obj, name);
#else
		Undo.RecordObject(obj, name);
#endif
	}

	public static void RegisterCompleteObjectUndo( UnityEngine.Object obj, string name ) {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterUndo(obj, name);
#else
		Undo.RegisterCompleteObjectUndo(obj, name);
#endif
	}

	public static void RecordObjects( UnityEngine.Object[] objs, string name ) {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterUndo(objs, name);
#else
		Undo.RecordObjects(objs, name);
#endif
	}
}


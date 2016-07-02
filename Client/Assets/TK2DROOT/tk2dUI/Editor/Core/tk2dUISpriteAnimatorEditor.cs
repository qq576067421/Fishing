using UnityEditor;
using UnityEngine;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUISpriteAnimator))]
public class tk2dUISpriteAnimatorEditor : tk2dSpriteAnimatorEditor {
	[MenuItem("CONTEXT/tk2dSpriteAnimator/Convert to UI Sprite Animator")]
	static void DoConvertUISpriteAnimator() {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo("Convert UI Sprite Animator");
#else
	    int undoGroup = Undo.GetCurrentGroup();
#endif

		foreach (GameObject go in Selection.gameObjects) {
			tk2dSpriteAnimator animator = go.GetComponent<tk2dSpriteAnimator>();
			if (animator != null) {
				tk2dUISpriteAnimator UIanimator = go.AddComponent<tk2dUISpriteAnimator>();
				UIanimator.Library = animator.Library;
				UIanimator.DefaultClipId = animator.DefaultClipId;
				UIanimator.playAutomatically = animator.playAutomatically;
#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
				DestroyImmediate(animator);
#else
				Undo.RegisterCreatedObjectUndo(UIanimator, "Create UI Sprite Animator");
				Undo.DestroyObjectImmediate(animator);
#endif
			}
		}

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		Undo.CollapseUndoOperations(undoGroup);
#endif			
	}
}
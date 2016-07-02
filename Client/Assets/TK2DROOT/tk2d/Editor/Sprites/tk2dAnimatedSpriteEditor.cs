using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dAnimatedSprite))]
class tk2dAnimatedSpriteEditor : tk2dSpriteEditor
{
    public override void OnInspectorGUI()
    {
    	bool doConvert = false;
		base.OnInspectorGUI();

		tk2dGuiUtility.InfoBox("The tk2dAnimatedSprite has been deprecated in favor of the new tk2dSpriteAnimator behaviour. " +
			"Using this new system will allow you to animate other kinds of sprites, etc. " +
			"The tk2dAnimatedSprite is now a wrapper to this system, but you can upgrade entirely to the new system " + 
			"if you choose to", tk2dGuiUtility.WarningLevel.Warning);
		
		GUILayout.Space(8);
		GUI.backgroundColor = Color.red;
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Convert to SpriteAnimator", GUILayout.Width(200)) && 
			EditorUtility.DisplayDialog("Convert to SpriteAnimator",
										"Converting to the SpriteAnimator system will require you to manually fix " +
										"all references of this tk2dSpriteAnimation.\n\n" +
										"Are you sure you wish to proceed?", "Yes", "No")) {
			doConvert = true;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		GUILayout.Space(8);

		if (doConvert) {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			Undo.RegisterSceneUndo("Convert animated sprite -> sprite animator");
#else
    	    int undoGroup = Undo.GetCurrentGroup();
#endif

			foreach (Object target in targets) {
				tk2dAnimatedSprite animSprite = target as tk2dAnimatedSprite;
				GameObject animSpriteGameObject = animSprite.gameObject;
				if (animSprite != null) {
					tk2dSprite sprite = animSprite.gameObject.AddComponent<tk2dSprite>();
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
					Undo.RegisterCreatedObjectUndo(sprite, "Create Sprite Animator");
#endif
					sprite.SetSprite( animSprite.Collection, animSprite.spriteId );
					sprite.color = animSprite.color;
					sprite.scale = animSprite.scale;
					// If this is not null, we assume it is already set up properly
					if (animSprite.GetComponent<tk2dSpriteAnimator>() == null) {
						tk2dSpriteAnimator spriteAnimator = animSprite.gameObject.AddComponent<tk2dSpriteAnimator>();
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
						Undo.RegisterCreatedObjectUndo(spriteAnimator, "Create Sprite Animator");
#endif
						spriteAnimator.Library = animSprite.Library;
						spriteAnimator.DefaultClipId = animSprite.DefaultClipId;
						spriteAnimator.playAutomatically = animSprite.playAutomatically;
					}

#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
					GameObject.DestroyImmediate(animSprite, true);
#else
					Undo.DestroyObjectImmediate(animSprite);
#endif
	
					tk2dUtil.SetDirty(animSpriteGameObject);
				}
			}

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			Undo.CollapseUndoOperations(undoGroup);
#endif			
		}
    }
}


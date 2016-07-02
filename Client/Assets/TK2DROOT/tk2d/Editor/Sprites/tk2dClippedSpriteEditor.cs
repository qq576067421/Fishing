using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dClippedSprite))]
class tk2dClippedSpriteEditor : tk2dSpriteEditor
{
	tk2dClippedSprite[] targetClippedSprites = new tk2dClippedSprite[0];

	new void OnEnable() {
		base.OnEnable();
		targetClippedSprites = GetTargetsOfType<tk2dClippedSprite>( targets );
	}

	private static bool showSceneClipControl = false;
	public override void OnInspectorGUI()
    {
        tk2dClippedSprite sprite = (tk2dClippedSprite)target;
		base.OnInspectorGUI();

		if (sprite.CurrentSprite == null) {
			return;
		}

		var spriteData = sprite.GetCurrentSpriteDef();
		if (spriteData != null)
			WarnSpriteRenderType(spriteData);

		bool newCreateBoxCollider = base.DrawCreateBoxColliderCheckbox(sprite.CreateBoxCollider);
		if (newCreateBoxCollider != sprite.CreateBoxCollider) {
			sprite.CreateBoxCollider = newCreateBoxCollider;
			if (sprite.CreateBoxCollider) { sprite.EditMode__CreateCollider(); }
		}

		Rect newClipRect = EditorGUILayout.RectField("Clip Region", sprite.ClipRect);
		if (newClipRect != sprite.ClipRect) {
			tk2dUndo.RecordObjects(targetClippedSprites, "Clipped Sprite Rect");
			foreach (tk2dClippedSprite spr in targetClippedSprites) {
				spr.ClipRect = newClipRect;
			}
		}

		showSceneClipControl = EditorGUILayout.Toggle("Edit Region in Scene", showSceneClipControl);

		EditorGUI.indentLevel--;

		if (GUI.changed) {
			foreach (tk2dClippedSprite spr in targetClippedSprites) {
				tk2dUtil.SetDirty(spr);
			}
		}
    }

	public new void OnSceneGUI() {
		if (tk2dPreferences.inst.enableSpriteHandles == false || !tk2dEditorUtility.IsEditable(target)) {
			return;
		}

		tk2dClippedSprite spr = (tk2dClippedSprite)target;
		var sprite = spr.CurrentSprite;
		if (sprite == null) {
			return;
		}

		Transform t = spr.transform;
		Bounds b = spr.GetUntrimmedBounds();
		Rect localRect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
		Rect clipRect = new Rect(b.min.x + b.size.x * ((spr.scale.x > 0) ? spr.clipBottomLeft.x : (1.0f - spr.clipTopRight.x)),
			b.min.y + b.size.y * ((spr.scale.y > 0) ? spr.clipBottomLeft.y : (1.0f - spr.clipTopRight.y)),
			b.size.x * spr.ClipRect.width,
			b.size.y * spr.ClipRect.height);

		// Draw rect outline
		Handles.color = new Color(1,1,1,0.5f);
		tk2dSceneHelper.DrawRect (localRect, t);

		// Clip region outline (if editing)
		if (showSceneClipControl) {
			Handles.color = new Color(1,0,0,0.5f);
			tk2dSceneHelper.DrawRect (clipRect, t);
		}

		Handles.BeginGUI ();
		if (!showSceneClipControl) {
			// Do the normal resize and rotate controls
			if (tk2dSceneHelper.RectControlsToggle ()) {
				EditorGUI.BeginChangeCheck();
				Rect resizeRect = tk2dSceneHelper.RectControl (102030, localRect, t);
				if (EditorGUI.EndChangeCheck ()) {
					tk2dUndo.RecordObjects (new Object[] {t, spr}, "Resize");
					spr.ReshapeBounds(new Vector3(resizeRect.xMin, resizeRect.yMin) - new Vector3(localRect.xMin, localRect.yMin),
						new Vector3(resizeRect.xMax, resizeRect.yMax) - new Vector3(localRect.xMax, localRect.yMax));
					tk2dUtil.SetDirty(spr);
				}
			}
			// Rotate handles
			if (!tk2dSceneHelper.RectControlsToggle ()) {
				EditorGUI.BeginChangeCheck();
				float theta = tk2dSceneHelper.RectRotateControl (405060, localRect, t, new List<int>());
				if (EditorGUI.EndChangeCheck()) {
					if (Mathf.Abs(theta) > Mathf.Epsilon) {
						tk2dUndo.RecordObject (t, "Rotate");
						t.Rotate(t.forward, theta, Space.World);
					}
				}
			}
		}
		else {
			// Clip region control
			EditorGUI.BeginChangeCheck();
			Rect resizeRect = tk2dSceneHelper.RectControl (708090, clipRect, t);
			if (EditorGUI.EndChangeCheck()) {
				Rect newSprClipRect = new Rect(((spr.scale.x > 0) ? (resizeRect.xMin - localRect.xMin) : (localRect.xMax - resizeRect.xMax)) / localRect.width,
					((spr.scale.y > 0) ? (resizeRect.yMin - localRect.yMin) : (localRect.yMax - resizeRect.yMax)) / localRect.height,
					resizeRect.width / localRect.width,
					resizeRect.height / localRect.height);
				if (newSprClipRect != spr.ClipRect) {
					tk2dUndo.RecordObject (spr, "Resize");
					spr.ClipRect = newSprClipRect;
					tk2dUtil.SetDirty(spr);
				}
			}
		}
		Handles.EndGUI ();

		// Sprite selecting
		tk2dSceneHelper.HandleSelectSprites();

		// Move targeted sprites
		tk2dSceneHelper.HandleMoveSprites(t, localRect);

    	if (GUI.changed) {
    		tk2dUtil.SetDirty(target);
    	}
	}

    [MenuItem(tk2dMenu.createBase + "Clipped Sprite", false, 12901)]
    static void DoCreateClippedSpriteObject()
    {
		tk2dSpriteGuiUtility.GetSpriteCollectionAndCreate( (sprColl) => {
			GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Clipped Sprite");
			tk2dClippedSprite sprite = go.AddComponent<tk2dClippedSprite>();
			sprite.SetSprite(sprColl, sprColl.FirstValidDefinitionIndex);
			sprite.Build();

			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Create Clipped Sprite");
		} );
    }
}


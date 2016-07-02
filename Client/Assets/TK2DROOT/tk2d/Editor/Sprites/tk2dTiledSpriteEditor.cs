using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dTiledSprite))]
class tk2dTiledSpriteEditor : tk2dSpriteEditor
{
	tk2dTiledSprite[] targetTiledSprites = new tk2dTiledSprite[0];

	new void OnEnable() {
		base.OnEnable();
		targetTiledSprites = GetTargetsOfType<tk2dTiledSprite>( targets );
	}

	public override void OnInspectorGUI()
    {
        tk2dTiledSprite sprite = (tk2dTiledSprite)target;
		base.OnInspectorGUI();
		
		if (sprite.Collection == null) {
			return;
		}
		
		EditorGUILayout.BeginVertical();
		
		var spriteData = sprite.GetCurrentSpriteDef();

		if (spriteData != null)
			WarnSpriteRenderType(spriteData);
		
		// need raw extents (excluding scale)
		Vector3 extents = spriteData.boundsData[1];

		bool newCreateBoxCollider = base.DrawCreateBoxColliderCheckbox(sprite.CreateBoxCollider);
		if (newCreateBoxCollider != sprite.CreateBoxCollider) {
			sprite.CreateBoxCollider = newCreateBoxCollider;
			if (sprite.CreateBoxCollider) { sprite.EditMode__CreateCollider(); }
		}
		
		// if either of these are zero, the division to rescale to pixels will result in a
		// div0, so display the data in fractions to avoid this situation
		bool editBorderInFractions = true;
		if (spriteData.texelSize.x != 0.0f && spriteData.texelSize.y != 0.0f && extents.x != 0.0f && extents.y != 0.0f) {
			editBorderInFractions = false;
		}
		
		if (!editBorderInFractions)
		{
			Vector2 newDimensions = EditorGUILayout.Vector2Field("Dimensions (Pixel Units)", sprite.dimensions);
			if (newDimensions != sprite.dimensions) {
				tk2dUndo.RecordObjects(targetTiledSprites, "Tiled Sprite Dimensions");
				foreach (tk2dTiledSprite spr in targetTiledSprites) {
					spr.dimensions = newDimensions;
				}
			}
			
			tk2dTiledSprite.Anchor newAnchor = (tk2dTiledSprite.Anchor)EditorGUILayout.EnumPopup("Anchor", sprite.anchor);
			if (newAnchor != sprite.anchor) {
				tk2dUndo.RecordObjects(targetTiledSprites, "Tiled Sprite Anchor");
				foreach (tk2dTiledSprite spr in targetTiledSprites) {
					spr.anchor = newAnchor;
				}
			}
		}
		else
		{
			GUILayout.Label("Border (Displayed as Fraction).\nSprite Collection needs to be rebuilt.", "textarea");
		}

		Mesh mesh = sprite.GetComponent<MeshFilter>().sharedMesh;
		if (mesh != null) {
			GUILayout.Label(string.Format("Triangles: {0}", mesh.triangles.Length / 3));
		}

		// One of the border valus has changed, so simply rebuild mesh data here		
		if (GUI.changed)
		{
			foreach (tk2dTiledSprite spr in targetTiledSprites) {
				spr.Build();
				tk2dUtil.SetDirty(spr);
			}
		}

		EditorGUILayout.EndVertical();
    }

	public new void OnSceneGUI() {
		if (tk2dPreferences.inst.enableSpriteHandles == false || !tk2dEditorUtility.IsEditable(target)) {
			return;
		}

		tk2dTiledSprite spr = (tk2dTiledSprite)target;

		Transform t = spr.transform;
		var sprite = spr.CurrentSprite;

		if (sprite == null) {
			return;
		}

		Vector2 totalMeshSize = new Vector2( spr.dimensions.x * sprite.texelSize.x * spr.scale.x, spr.dimensions.y * sprite.texelSize.y * spr.scale.y );
		Vector2 anchorOffset = tk2dSceneHelper.GetAnchorOffset(totalMeshSize, spr.anchor);

		{
			Vector3 v = new Vector3( anchorOffset.x, anchorOffset.y, 0 );
			Vector3 d = totalMeshSize;
			Rect rect0 = new Rect(v.x, v.y, d.x, d.y);

			Handles.color = new Color(1,1,1, 0.5f);
			tk2dSceneHelper.DrawRect(rect0, t);

			Handles.BeginGUI();
			// Resize handles 
			if (tk2dSceneHelper.RectControlsToggle ()) {
				EditorGUI.BeginChangeCheck();
				Rect resizeRect = tk2dSceneHelper.RectControl( 123192, rect0, t );
				if (EditorGUI.EndChangeCheck ()) {
					tk2dUndo.RecordObjects (new Object[] {t, spr}, "Resize");
					spr.ReshapeBounds(new Vector3(resizeRect.xMin, resizeRect.yMin) - new Vector3(rect0.xMin, rect0.yMin),
						new Vector3(resizeRect.xMax, resizeRect.yMax) - new Vector3(rect0.xMax, rect0.yMax));
					tk2dUtil.SetDirty(spr);
				}
			}

			// Rotate handles
			if (!tk2dSceneHelper.RectControlsToggle ()) {
				EditorGUI.BeginChangeCheck();
				List<int> hidePts = tk2dSceneHelper.getAnchorHidePtList(spr.anchor, rect0, t);
				float theta = tk2dSceneHelper.RectRotateControl( 456384, rect0, t, hidePts );
				if (EditorGUI.EndChangeCheck()) {
					if (Mathf.Abs(theta) > Mathf.Epsilon) {
						tk2dUndo.RecordObject(t, "Rotate");
						t.Rotate(t.forward, theta, Space.World);
					}
				}
			}

			Handles.EndGUI();

			// Sprite selecting
			tk2dSceneHelper.HandleSelectSprites();

			// Sprite moving (translation)
			tk2dSceneHelper.HandleMoveSprites(t, new Rect(v.x, v.y, d.x, d.y));
		}

    	if (GUI.changed) {
    		tk2dUtil.SetDirty(target);
    	}
	}

    [MenuItem(tk2dMenu.createBase + "Tiled Sprite", false, 12901)]
    static void DoCreateSlicedSpriteObject()
    {
		tk2dSpriteGuiUtility.GetSpriteCollectionAndCreate( (sprColl) => {
			GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Tiled Sprite");
			tk2dTiledSprite sprite = go.AddComponent<tk2dTiledSprite>();
			sprite.SetSprite(sprColl, sprColl.FirstValidDefinitionIndex);
			sprite.Build();
			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Create Tiled Sprite");
		} );
    }
}


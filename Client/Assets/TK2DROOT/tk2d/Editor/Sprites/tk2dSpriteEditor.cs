using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dSprite))]
class tk2dSpriteEditor : Editor
{
	// Serialized properties are going to be far too much hassle
	private tk2dBaseSprite[] targetSprites = new tk2dBaseSprite[0];
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
	private Renderer[] renderers = new Renderer[0];
#endif

    public override void OnInspectorGUI()
    {
		DrawSpriteEditorGUI();
    }

    public void OnSceneGUI()
    {
		if (tk2dPreferences.inst.enableSpriteHandles == false || !tk2dEditorUtility.IsEditable(target)) {
			return;
		}

    	tk2dSprite spr = (tk2dSprite)target;
		var sprite = spr.CurrentSprite;

		if (sprite == null) {
			return;
		}

		Transform t = spr.transform;
		Bounds b = spr.GetUntrimmedBounds();
		Rect localRect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);

		// Draw rect outline
		Handles.color = new Color(1,1,1,0.5f);
		tk2dSceneHelper.DrawRect (localRect, t);

		Handles.BeginGUI ();
		// Resize handles
		if (tk2dSceneHelper.RectControlsToggle ()) {
			EditorGUI.BeginChangeCheck ();
			Rect resizeRect = tk2dSceneHelper.RectControl (999888, localRect, t);
			if (EditorGUI.EndChangeCheck ()) {
				tk2dUndo.RecordObjects(new Object[] {t, spr}, "Resize");
				spr.ReshapeBounds(new Vector3(resizeRect.xMin, resizeRect.yMin) - new Vector3(localRect.xMin, localRect.yMin),
					new Vector3(resizeRect.xMax, resizeRect.yMax) - new Vector3(localRect.xMax, localRect.yMax));
				tk2dUtil.SetDirty(spr);
			}
		}
		// Rotate handles
		if (!tk2dSceneHelper.RectControlsToggle ()) {
			EditorGUI.BeginChangeCheck();
			float theta = tk2dSceneHelper.RectRotateControl (888999, localRect, t, new List<int>());
			if (EditorGUI.EndChangeCheck()) {
				tk2dUndo.RecordObject (t, "Rotate");
				if (Mathf.Abs(theta) > Mathf.Epsilon) {
					t.Rotate(t.forward, theta, Space.World);
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

    protected T[] GetTargetsOfType<T>( Object[] objects ) where T : UnityEngine.Object {
    	List<T> ts = new List<T>();
    	foreach (Object o in objects) {
    		T s = o as T;
    		if (s != null)
    			ts.Add(s);
    	}
    	return ts.ToArray();
    }

    protected void OnEnable()
    {
    	targetSprites = GetTargetsOfType<tk2dBaseSprite>( targets );

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
    	List<Renderer> rs = new List<Renderer>();
    	foreach (var v in targetSprites) {
    		if (v != null && v.GetComponent<Renderer>() != null) {
    			rs.Add(v.GetComponent<Renderer>());
    		}
    	}
    	renderers = rs.ToArray();
#endif
    }
	
	void OnDestroy()
	{
		targetSprites = new tk2dBaseSprite[0];

		tk2dSpriteThumbnailCache.Done();
		tk2dGrid.Done();
		tk2dEditorSkin.Done();
	}
	
	// Callback and delegate
	void SpriteChangedCallbackImpl(tk2dSpriteCollectionData spriteCollection, int spriteId, object data)
	{
		tk2dUndo.RecordObjects(targetSprites, "Sprite Change");
		
		foreach (tk2dBaseSprite s in targetSprites) {
			s.SetSprite(spriteCollection, spriteId);
			s.EditMode__CreateCollider();
			tk2dUtil.SetDirty(s);
		}
	}
	tk2dSpriteGuiUtility.SpriteChangedCallback _spriteChangedCallbackInstance = null;
	tk2dSpriteGuiUtility.SpriteChangedCallback spriteChangedCallbackInstance {
		get {
			if (_spriteChangedCallbackInstance == null) {
				_spriteChangedCallbackInstance = new tk2dSpriteGuiUtility.SpriteChangedCallback( SpriteChangedCallbackImpl );
			}
			return _spriteChangedCallbackInstance;
		}
	}

	protected void DrawSpriteEditorGUI()
	{
		Event ev = Event.current;
		tk2dSpriteGuiUtility.SpriteSelector( targetSprites[0].Collection, targetSprites[0].spriteId, spriteChangedCallbackInstance, null );

        if (targetSprites[0].Collection != null)
        {
        	if (tk2dPreferences.inst.displayTextureThumbs) {
        		tk2dBaseSprite sprite = targetSprites[0];
				tk2dSpriteDefinition def = sprite.GetCurrentSpriteDef();
				if (sprite.Collection.version < 1 || def.texelSize == Vector2.zero)
				{
					string message = "";
					
					message = "No thumbnail data.";
					if (sprite.Collection.version < 1)
						message += "\nPlease rebuild Sprite Collection.";
					
					tk2dGuiUtility.InfoBox(message, tk2dGuiUtility.WarningLevel.Info);
				}
				else
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(" ");

					int tileSize = 128;
					Rect r = GUILayoutUtility.GetRect(tileSize, tileSize, GUILayout.ExpandWidth(false));
					tk2dGrid.Draw(r);
					tk2dSpriteThumbnailCache.DrawSpriteTextureInRect(r, def, Color.white);

					GUILayout.EndHorizontal();

					r = GUILayoutUtility.GetLastRect();
					if (ev.type == EventType.MouseDown && ev.button == 0 && r.Contains(ev.mousePosition)) {
						tk2dSpriteGuiUtility.SpriteSelectorPopup( sprite.Collection, sprite.spriteId, spriteChangedCallbackInstance, null );
					}
				}
			}

            Color newColor = EditorGUILayout.ColorField("Color", targetSprites[0].color);
            if (newColor != targetSprites[0].color) {
            	tk2dUndo.RecordObjects(targetSprites, "Sprite Color");
            	foreach (tk2dBaseSprite s in targetSprites) {
            		s.color = newColor;
            	}
            }

            GUILayout.Space(8);
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			int sortingOrder = EditorGUILayout.IntField("Sorting Order In Layer", targetSprites[0].SortingOrder);
			if (sortingOrder != targetSprites[0].SortingOrder) {
            	tk2dUndo.RecordObjects(targetSprites, "Sorting Order In Layer");
            	foreach (tk2dBaseSprite s in targetSprites) {
            		s.SortingOrder = sortingOrder;
            	}
			}
#else
			if (renderers.Length > 0) {
	            string sortingLayerName = tk2dEditorUtility.SortingLayerNamePopup("Sorting Layer", renderers[0].sortingLayerName);
	            if (sortingLayerName != renderers[0].sortingLayerName) {
	            	tk2dUndo.RecordObjects(renderers, "Sorting Layer");
	            	foreach (Renderer r in renderers) {
	            		r.sortingLayerName = sortingLayerName;
	            	}
	            }

				int sortingOrder = EditorGUILayout.IntField("Order In Layer", targetSprites[0].SortingOrder);
				if (sortingOrder != targetSprites[0].SortingOrder) {
	            	tk2dUndo.RecordObjects(targetSprites, "Order In Layer");
	            	tk2dUndo.RecordObjects(renderers, "Order In Layer");
	            	foreach (tk2dBaseSprite s in targetSprites) {
	            		s.SortingOrder = sortingOrder;
	            	}
				}
			}
#endif
            GUILayout.Space(8);


			Vector3 newScale = EditorGUILayout.Vector3Field("Scale", targetSprites[0].scale);
			if (newScale != targetSprites[0].scale)
			{
				tk2dUndo.RecordObjects(targetSprites, "Sprite Scale");
				foreach (tk2dBaseSprite s in targetSprites) {
					s.scale = newScale;
					s.EditMode__CreateCollider();
				}
			}
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("HFlip", EditorStyles.miniButton))
			{
				tk2dUndo.RecordObjects(targetSprites, "Sprite HFlip");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					sprite.EditMode__CreateCollider();
					Vector3 scale = sprite.scale;
					scale.x *= -1.0f;
					sprite.scale = scale;
				}
				GUI.changed = true;
			}
			if (GUILayout.Button("VFlip", EditorStyles.miniButton))
			{
				tk2dUndo.RecordObjects(targetSprites, "Sprite VFlip");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					Vector3 s = sprite.scale;
					s.y *= -1.0f;
					sprite.scale = s;
					GUI.changed = true;
				}
			}
			
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button(new GUIContent("Reset Scale", "Set scale to 1"), EditorStyles.miniButton))
			{
				tk2dUndo.RecordObjects(targetSprites, "Sprite Reset Scale");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					Vector3 s = sprite.scale;
					s.x = Mathf.Sign(s.x);
					s.y = Mathf.Sign(s.y);
					s.z = Mathf.Sign(s.z);
					sprite.scale = s;
					GUI.changed = true;
				}
			}
			
			if (GUILayout.Button(new GUIContent("Bake Scale", "Transfer scale from transform.scale -> sprite"), EditorStyles.miniButton))
			{
				foreach (tk2dBaseSprite sprite in targetSprites) {
					tk2dScaleUtility.Bake(sprite.transform);
				}
				GUI.changed = true;
			}
			
			GUIContent pixelPerfectButton = new GUIContent("1:1", "Make Pixel Perfect for camera");
			if ( GUILayout.Button(pixelPerfectButton, EditorStyles.miniButton ))
			{
				if (tk2dPixelPerfectHelper.inst) tk2dPixelPerfectHelper.inst.Setup();
				tk2dUndo.RecordObjects(targetSprites, "Sprite Pixel Perfect");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					sprite.MakePixelPerfect();
				}
				GUI.changed = true;
			}
			
			EditorGUILayout.EndHorizontal();
        }
        else
        {
			tk2dGuiUtility.InfoBox("Please select a sprite collection.", tk2dGuiUtility.WarningLevel.Error);        
		}


		bool needUpdatePrefabs = false;
		if (GUI.changed)
		{
			foreach (tk2dBaseSprite sprite in targetSprites) {
			if (PrefabUtility.GetPrefabType(sprite) == PrefabType.Prefab)
				needUpdatePrefabs = true;
				tk2dUtil.SetDirty(sprite);
			}
		}
		
		// This is a prefab, and changes need to be propagated. This isn't supported in Unity 3.4
		if (needUpdatePrefabs)
		{
			// Rebuild prefab instances
			tk2dBaseSprite[] allSprites = Resources.FindObjectsOfTypeAll(typeof(tk2dBaseSprite)) as tk2dBaseSprite[];
			foreach (var spr in allSprites)
			{
				if (PrefabUtility.GetPrefabType(spr) == PrefabType.PrefabInstance)
				{
					Object parent = PrefabUtility.GetPrefabParent(spr.gameObject);
					bool found = false;
					foreach (tk2dBaseSprite sprite in targetSprites) {
						if (sprite.gameObject == parent) {
							found = true;
							break;
						}
					}

					if (found) {
						// Reset all prefab states
						var propMod = PrefabUtility.GetPropertyModifications(spr);
						PrefabUtility.ResetToPrefabState(spr);
						PrefabUtility.SetPropertyModifications(spr, propMod);
						
						spr.ForceBuild();
					}
				}
			}
		}
	}

	protected void WarnSpriteRenderType(tk2dSpriteDefinition sprite) {
		if (sprite.positions.Length != 4 || sprite.complexGeometry) {
			EditorGUILayout.HelpBox("Sprite type incompatible with Render Mesh setting.\nPlease use Default Render Mesh.", MessageType.Error);
		}
	}

	static void PerformActionOnGlobalSelection(string actionName, System.Action<GameObject> action) {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSceneUndo(actionName);
#else
		int undoGroup = Undo.GetCurrentGroup();
#endif
		foreach (GameObject go in Selection.gameObjects) {
			if (go.GetComponent<tk2dBaseSprite>() != null) {
				action(go);
			}
		}

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		Undo.CollapseUndoOperations(undoGroup);
#endif
	}

	static void ConvertSpriteType(GameObject go, System.Type targetType) {
		tk2dBaseSprite spr = go.GetComponent<tk2dBaseSprite>();
		System.Type sourceType = spr.GetType();

		if (sourceType != targetType) {
			tk2dBatchedSprite batchedSprite = new tk2dBatchedSprite();
			tk2dStaticSpriteBatcherEditor.FillBatchedSprite(batchedSprite, go);
			if (targetType == typeof(tk2dSprite)) batchedSprite.type = tk2dBatchedSprite.Type.Sprite;
			else if (targetType == typeof(tk2dTiledSprite)) batchedSprite.type = tk2dBatchedSprite.Type.TiledSprite;
			else if (targetType == typeof(tk2dSlicedSprite)) batchedSprite.type = tk2dBatchedSprite.Type.SlicedSprite;
			else if (targetType == typeof(tk2dClippedSprite)) batchedSprite.type = tk2dBatchedSprite.Type.ClippedSprite;

#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			if (spr.collider != null) {
				Object.DestroyImmediate(spr.collider);
			}
			Object.DestroyImmediate(spr, true);
#else
			{
				Collider[] colliders = spr.GetComponents<Collider>();
				foreach (Collider c in colliders) {
					Undo.DestroyObjectImmediate(c);
				}
				Collider2D[] collider2Ds = spr.GetComponents<Collider2D>();
				foreach (Collider2D c in collider2Ds) {
					Undo.DestroyObjectImmediate(c);
				}
			}
			Undo.DestroyObjectImmediate(spr);
#endif

			bool sourceHasDimensions = sourceType == typeof(tk2dSlicedSprite) || sourceType == typeof(tk2dTiledSprite);
			bool targetHasDimensions = targetType == typeof(tk2dSlicedSprite) || targetType == typeof(tk2dTiledSprite);

			// Some minor fixups
			if (!sourceHasDimensions && targetHasDimensions) {
				batchedSprite.Dimensions = new Vector2(100, 100);
			}
			if (targetType == typeof(tk2dClippedSprite)) {
				batchedSprite.ClippedSpriteRegionBottomLeft = Vector2.zero;
				batchedSprite.ClippedSpriteRegionTopRight = Vector2.one;
			}
			if (targetType == typeof(tk2dSlicedSprite)) {
				batchedSprite.SlicedSpriteBorderBottomLeft = new Vector2(0.1f, 0.1f);
				batchedSprite.SlicedSpriteBorderTopRight = new Vector2(0.1f, 0.1f);
			}

			tk2dStaticSpriteBatcherEditor.RestoreBatchedSprite(go, batchedSprite);
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			{ 
				tk2dBaseSprite tmpSprite = go.GetComponent<tk2dBaseSprite>();
				if (tmpSprite != null) {
					Undo.RegisterCreatedObjectUndo( tmpSprite, "Convert Sprite Type" );
				}
			}
#endif
		}
	}

	// This is used by derived classes only
	protected bool DrawCreateBoxColliderCheckbox(bool value) {
		tk2dBaseSprite sprite = target as tk2dBaseSprite;
		bool newCreateBoxCollider = EditorGUILayout.Toggle("Create Box Collider", value);
		if (newCreateBoxCollider != value) {
			tk2dUndo.RecordObjects(targetSprites, "Create Box Collider");
			if (!newCreateBoxCollider) {
				var boxCollider = sprite.GetComponent<BoxCollider>();
				if (boxCollider != null) {
					DestroyImmediate(boxCollider);
				}
				sprite.boxCollider = null;
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
				var boxCollider2D = sprite.GetComponent<BoxCollider2D>();
				if (boxCollider2D != null) {
					DestroyImmediate(boxCollider2D);
				}
				sprite.boxCollider2D = null;
#endif
			}
		}
		return newCreateBoxCollider;
	}

	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Sprite")]
	static void DoConvertSprite() { PerformActionOnGlobalSelection( "Convert to Sprite", (go) => ConvertSpriteType(go, typeof(tk2dSprite)) ); }
	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Sliced Sprite")]
	static void DoConvertSlicedSprite() { PerformActionOnGlobalSelection( "Convert to Sliced Sprite", (go) => ConvertSpriteType(go, typeof(tk2dSlicedSprite)) ); }
	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Tiled Sprite")]
	static void DoConvertTiledSprite() { PerformActionOnGlobalSelection( "Convert to Tiled Sprite", (go) => ConvertSpriteType(go, typeof(tk2dTiledSprite)) ); }
	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Clipped Sprite")]
	static void DoConvertClippedSprite() { PerformActionOnGlobalSelection( "Convert to Clipped Sprite", (go) => ConvertSpriteType(go, typeof(tk2dClippedSprite)) ); }
	

	[MenuItem("CONTEXT/tk2dBaseSprite/Add animator", true, 10000)]
	static bool ValidateAddAnimator() {
		if (Selection.activeGameObject == null) return false;
		return Selection.activeGameObject.GetComponent<tk2dSpriteAnimator>() == null;
	}
	[MenuItem("CONTEXT/tk2dBaseSprite/Add animator", false, 10000)]
	static void DoAddAnimator() {
		tk2dSpriteAnimation anim = null;
		int clipId = -1;
		if (!tk2dSpriteAnimatorEditor.GetDefaultSpriteAnimation(out anim, out clipId)) {
			EditorUtility.DisplayDialog("Create Sprite Animation", "Unable to create animated sprite as no SpriteAnimations have been found.", "Ok");
			return;
		}
		else {
			PerformActionOnGlobalSelection("Add animator", delegate(GameObject go) {
				tk2dSpriteAnimator animator = go.GetComponent<tk2dSpriteAnimator>();
				if (animator == null) {
					animator = go.AddComponent<tk2dSpriteAnimator>();
					animator.Library = anim;
					animator.DefaultClipId = clipId;
					tk2dSpriteAnimationClip clip = anim.GetClipById(clipId);
					animator.SetSprite( clip.frames[0].spriteCollection, clip.frames[0].spriteId );
				}
			});
		}
	}

	[MenuItem("CONTEXT/tk2dBaseSprite/Add AttachPoint", false, 10002)]
	static void DoRemoveAnimator() {
		PerformActionOnGlobalSelection("Add AttachPoint", delegate(GameObject go) {
			tk2dSpriteAttachPoint ap = go.GetComponent<tk2dSpriteAttachPoint>();
			if (ap == null) {
				go.AddComponent<tk2dSpriteAttachPoint>();
			}
		});	
	}

	[MenuItem(tk2dMenu.createBase + "Sprite", false, 1290)]
    static void DoCreateSpriteObject()
    {
    	tk2dSpriteGuiUtility.GetSpriteCollectionAndCreate( (sprColl) => {
			GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Sprite");
			tk2dSprite sprite = go.AddComponent<tk2dSprite>();
			sprite.SetSprite(sprColl, sprColl.FirstValidDefinitionIndex);
			sprite.GetComponent<Renderer>().material = sprColl.FirstValidDefinition.material;
			sprite.Build();
			
			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Create Sprite");
		} );
    }
}


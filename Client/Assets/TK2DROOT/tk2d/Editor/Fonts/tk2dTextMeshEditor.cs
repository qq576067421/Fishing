using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dTextMesh))]
class tk2dTextMeshEditor : Editor
{
	tk2dGenericIndexItem[] allFonts = null;	// all generators
	string[] allFontNames = null;
	Vector2 gradientScroll;
	
	// Word wrap on text area - almost impossible to use otherwise
	GUIStyle _textAreaStyle = null;
	GUIStyle textAreaStyle
	{
		get {
			if (_textAreaStyle == null)
			{
				_textAreaStyle = new GUIStyle(EditorStyles.textField);
				_textAreaStyle.wordWrap = true;
			}
			return _textAreaStyle;
		}
	}

	tk2dTextMesh[] targetTextMeshes = new tk2dTextMesh[0];
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
	Renderer[] renderers = new Renderer[0];
#endif

	void OnEnable() {
		targetTextMeshes = new tk2dTextMesh[targets.Length];
		for (int i = 0; i < targets.Length; ++i) {
			targetTextMeshes[i] = targets[i] as tk2dTextMesh;
		}

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
    	List<Renderer> rs = new List<Renderer>();
    	foreach (var v in targetTextMeshes) {
    		if (v != null && v.GetComponent<Renderer>() != null) {
    			rs.Add(v.GetComponent<Renderer>());
    		}
    	}
    	renderers = rs.ToArray();
#endif
	}

	void OnDestroy() {
		tk2dEditorSkin.Done();
	}

	// Draws the word wrap GUI
	void DrawWordWrapSceneGUI(tk2dTextMesh textMesh)
	{
		tk2dFontData font = textMesh.font;
		Transform transform = textMesh.transform;

		int px = textMesh.wordWrapWidth;

		Vector3 p0 = transform.position;
		float width = font.texelSize.x * px * transform.localScale.x;
		bool drawRightHandle = true;
		bool drawLeftHandle = false;
		switch (textMesh.anchor)
		{
			case TextAnchor.LowerCenter: case TextAnchor.MiddleCenter: case TextAnchor.UpperCenter:
				drawLeftHandle = true;
				p0 -= width * 0.5f * transform.right;
				break;
			case TextAnchor.LowerRight: case TextAnchor.MiddleRight: case TextAnchor.UpperRight:
				drawLeftHandle = true;
				drawRightHandle = false;
				p0 -= width * transform.right;
				break;
		}
		Vector3 p1 = p0 + width * transform.right;


		Handles.color = new Color32(255, 255, 255, 24);
		float subPin = font.texelSize.y * 2048;
		Handles.DrawLine(p0, p1);
		Handles.DrawLine(p0 - subPin * transform.up, p0 + subPin * transform.up);
		Handles.DrawLine(p1 - subPin * transform.up, p1 + subPin * transform.up);

		Handles.color = Color.white;
		Vector3 pin = transform.up * font.texelSize.y * 10.0f;
		Handles.DrawLine(p0 - pin, p0 + pin);
		Handles.DrawLine(p1 - pin, p1 + pin);

		if (drawRightHandle)
		{
			Vector3 newp1 = Handles.Slider(p1, transform.right, HandleUtility.GetHandleSize(p1), Handles.ArrowCap, 0.0f);
			if (newp1 != p1)
			{
				tk2dUndo.RecordObject(textMesh, "TextMesh Wrap Length");
				int newPx = (int)Mathf.Round((newp1 - p0).magnitude / (font.texelSize.x * transform.localScale.x));
				newPx = Mathf.Max(newPx, 0);
				textMesh.wordWrapWidth = newPx;
				textMesh.Commit();
			}
		}

		if (drawLeftHandle)
		{
			Vector3 newp0 = Handles.Slider(p0, -transform.right, HandleUtility.GetHandleSize(p0), Handles.ArrowCap, 0.0f);
			if (newp0 != p0)
			{
				tk2dUndo.RecordObject(textMesh, "TextMesh Wrap Length");
				int newPx = (int)Mathf.Round((p1 - newp0).magnitude / (font.texelSize.x * transform.localScale.x));
				newPx = Mathf.Max(newPx, 0);
				textMesh.wordWrapWidth = newPx;
				textMesh.Commit();
			}
		}
	}

	public void OnSceneGUI()
	{
		tk2dTextMesh textMesh = (tk2dTextMesh)target;
		if (textMesh.formatting && textMesh.wordWrapWidth > 0)
		{
			DrawWordWrapSceneGUI(textMesh);
		}

		if (tk2dPreferences.inst.enableSpriteHandles == true) {
		
			MeshFilter meshFilter = textMesh.GetComponent<MeshFilter>();
			if (!meshFilter || meshFilter.sharedMesh == null) {
				return;
			}
			Transform t = textMesh.transform;
			Bounds b = meshFilter.sharedMesh.bounds;
			Rect localRect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
			
			// Draw rect outline
			Handles.color = new Color(1,1,1,0.5f);
			tk2dSceneHelper.DrawRect (localRect, t);
			
			Handles.BeginGUI ();
			// Resize handles
			if (tk2dSceneHelper.RectControlsToggle ()) {
				EditorGUI.BeginChangeCheck ();
				Rect resizeRect = tk2dSceneHelper.RectControl (132546, localRect, t);
				if (EditorGUI.EndChangeCheck ()) {
					int newWrapWidth = (int)((float)textMesh.wordWrapWidth * resizeRect.width / localRect.width);
					newWrapWidth = Mathf.Max(1, newWrapWidth);
					Vector3 newScale = new Vector3 (textMesh.scale.x * (resizeRect.width / localRect.width),
					                                textMesh.scale.y * (resizeRect.height / localRect.height));
					if (textMesh.formatting && textMesh.wordWrapWidth > 0) {
						float fx = (float)newWrapWidth / (float)textMesh.wordWrapWidth;
						newScale.x = textMesh.scale.x * fx;
						if (Mathf.Abs(localRect.xMin - resizeRect.xMin) < Mathf.Abs(localRect.xMax - resizeRect.xMax)) {
							resizeRect.xMax = localRect.xMin + fx * localRect.width;
						} else {
							resizeRect.xMin = localRect.xMax - fx * localRect.width;
						}
					}
					float scaleMin = 0.001f;
					if (Mathf.Abs(newScale.x) < scaleMin) {
						newScale.x = scaleMin * Mathf.Sign(textMesh.scale.x);
					}
					if (Mathf.Abs(newScale.y) < scaleMin) {
						newScale.y = scaleMin * Mathf.Sign(textMesh.scale.y);
					}
					if (newScale != textMesh.scale) {
						tk2dUndo.RecordObjects (new Object[] {t, textMesh}, "Resize");
						float factorX = newScale.x / (Mathf.Approximately(textMesh.scale.x, 0.0f) ? 1.0f : textMesh.scale.x);
						float factorY = newScale.y / (Mathf.Approximately(textMesh.scale.y, 0.0f) ? 1.0f : textMesh.scale.y);
						Vector3 offset = new Vector3(resizeRect.xMin - localRect.xMin * factorX,
						                             resizeRect.yMin - localRect.yMin * factorY, 0.0f);
						Vector3 newPosition = t.TransformPoint (offset);
						if (newPosition != t.position) {
							t.position = newPosition;
						}
						textMesh.scale = newScale;
						if (textMesh.formatting && textMesh.wordWrapWidth > 0) {
							textMesh.wordWrapWidth = newWrapWidth;
						}
						textMesh.Commit ();
						tk2dUtil.SetDirty(textMesh);
					}
				}
			}
			// Rotate handles
			if (!tk2dSceneHelper.RectControlsToggle ()) {
				EditorGUI.BeginChangeCheck();
				float theta = tk2dSceneHelper.RectRotateControl (645231, localRect, t, new List<int>());
				if (EditorGUI.EndChangeCheck()) {
					if (Mathf.Abs(theta) > Mathf.Epsilon) {
						tk2dUndo.RecordObject (t, "Rotate");
						t.Rotate(t.forward, theta, Space.World);
					}
				}
			}
			Handles.EndGUI ();
			
			// Sprite selecting
			tk2dSceneHelper.HandleSelectSprites();
			
			// Move targeted sprites
			tk2dSceneHelper.HandleMoveSprites(t, localRect);
		}

    	if (GUI.changed) {
    		tk2dUtil.SetDirty(target);
    	}
	}

	void UndoableAction( System.Action<tk2dTextMesh> action ) {
		tk2dUndo.RecordObjects (targetTextMeshes, "Inspector");
		foreach (tk2dTextMesh tm in targetTextMeshes) {
			action(tm);
		}
	}

	static bool showInlineStylingHelp = false;

    public override void OnInspectorGUI()
    {
        tk2dTextMesh textMesh = (tk2dTextMesh)target;
        tk2dGuiUtility.LookLikeControls(80, 50);
		
		// maybe cache this if its too slow later
		if (allFonts == null || allFontNames == null) 
		{
			tk2dGenericIndexItem[] indexFonts = tk2dEditorUtility.GetOrCreateIndex().GetFonts();
			List<tk2dGenericIndexItem> filteredFonts = new List<tk2dGenericIndexItem>();
			foreach (var f in indexFonts)
				if (!f.managed) filteredFonts.Add(f);

			allFonts = filteredFonts.ToArray();
			allFontNames = new string[allFonts.Length];
			for (int i = 0; i < allFonts.Length; ++i)
				allFontNames[i] = allFonts[i].AssetName;
		}
		
		if (allFonts != null && allFonts.Length > 0)
        {
			if (textMesh.font == null)
			{
				textMesh.font = allFonts[0].GetAsset<tk2dFont>().data;
			}
			
			int currId = -1;
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(textMesh.font));
			for (int i = 0; i < allFonts.Length; ++i)
			{
				if (allFonts[i].dataGUID == guid)
				{
					currId = i;
				}
			}
			
			int newId = EditorGUILayout.Popup("Font", currId, allFontNames);
			if (newId != currId)
			{
				UndoableAction( tm => tm.font = allFonts[newId].GetAsset<tk2dFont>().data );
				GUI.changed = true;
			}
			
			EditorGUILayout.BeginHorizontal();
			int newMaxChars = Mathf.Clamp( EditorGUILayout.IntField("Max Chars", textMesh.maxChars), 1, 16000 );
			if (newMaxChars != textMesh.maxChars) {
				UndoableAction( tm => tm.maxChars = newMaxChars );
			}

			if (GUILayout.Button("Fit", GUILayout.MaxWidth(32.0f)))
			{
				UndoableAction( tm => tm.maxChars = tm.NumTotalCharacters() );
				GUI.changed = true;
			}
			EditorGUILayout.EndHorizontal();

			bool newFormatting = EditorGUILayout.BeginToggleGroup("Formatting", textMesh.formatting);
			if (newFormatting != textMesh.formatting) {
				UndoableAction( tm => tm.formatting = newFormatting );
				GUI.changed = true;
			}

			GUILayout.BeginHorizontal();
			++EditorGUI.indentLevel;
			if (textMesh.wordWrapWidth == 0)
			{
				EditorGUILayout.PrefixLabel("Word Wrap");
				if (GUILayout.Button("Enable", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					UndoableAction( tm => tm.wordWrapWidth = (tm.wordWrapWidth == 0) ? 500 : tm.wordWrapWidth );
					GUI.changed = true;
				}
			}
			else
			{
				int newWordWrapWidth = EditorGUILayout.IntField("Word Wrap", textMesh.wordWrapWidth);
				if (newWordWrapWidth != textMesh.wordWrapWidth) {
					UndoableAction( tm => tm.wordWrapWidth = newWordWrapWidth );
				}

				if (GUILayout.Button("Disable", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					UndoableAction( tm => tm.wordWrapWidth = 0 );
					GUI.changed = true;
				}
			}
			--EditorGUI.indentLevel;
			GUILayout.EndHorizontal();
			EditorGUILayout.EndToggleGroup();

			GUILayout.BeginHorizontal ();
			bool newInlineStyling = EditorGUILayout.Toggle("Inline Styling", textMesh.inlineStyling);
			if (newInlineStyling != textMesh.inlineStyling) {
				UndoableAction( tm => tm.inlineStyling = newInlineStyling );
			}
			if (textMesh.inlineStyling) {
				showInlineStylingHelp = GUILayout.Toggle(showInlineStylingHelp, "?", EditorStyles.miniButton, GUILayout.ExpandWidth(false));
			}
			GUILayout.EndHorizontal ();
			
			if (textMesh.inlineStyling && showInlineStylingHelp)
			{
				Color bg = GUI.backgroundColor;
				GUI.backgroundColor = new Color32(154, 176, 203, 255);
				string message = "Inline style commands\n\n" +
				                 "^cRGBA - set color\n" +
				                 "^gRGBARGBA - set top and bottom colors\n" +
				                 "      RGBA = single digit hex values (0 - f)\n\n" +
				                 "^CRRGGBBAA - set color\n" +
				                 "^GRRGGBBAARRGGBBAA - set top and bottom colors\n" +
				                 "      RRGGBBAA = 2 digit hex values (00 - ff)\n\n" +
				                 ((textMesh.font.textureGradients && textMesh.font.gradientCount > 0) ?
				 				 "^0-9 - select gradient\n" : "") +
				                 "^^ - print ^";
				tk2dGuiUtility.InfoBox( message, tk2dGuiUtility.WarningLevel.Info );
				GUI.backgroundColor = bg;						
			}
			
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Text");
			string newText = EditorGUILayout.TextArea(textMesh.text, textAreaStyle, GUILayout.Height(64));
			if (newText != textMesh.text) {
				UndoableAction( tm => tm.text = newText );
				GUI.changed = true;
			}
			GUILayout.EndHorizontal();

			if (textMesh.NumTotalCharacters() > textMesh.maxChars) {
				tk2dGuiUtility.InfoBox( "Number of printable characters in text mesh exceeds MaxChars on this text mesh. "+
					 					"The text will be clipped at " + textMesh.maxChars.ToString() + " characters.", tk2dGuiUtility.WarningLevel.Error );
			}
			
			TextAnchor newTextAnchor = (TextAnchor)EditorGUILayout.EnumPopup("Anchor", textMesh.anchor);
			if (newTextAnchor != textMesh.anchor) UndoableAction( tm => tm.anchor = newTextAnchor );

			bool newKerning = EditorGUILayout.Toggle("Kerning", textMesh.kerning);
			if (newKerning != textMesh.kerning) UndoableAction( tm => tm.kerning = newKerning );

			float newSpacing = EditorGUILayout.FloatField("Spacing", textMesh.Spacing);
			if (newSpacing != textMesh.Spacing) UndoableAction( tm => tm.Spacing = newSpacing );

			float newLineSpacing = EditorGUILayout.FloatField("Line Spacing", textMesh.LineSpacing);
			if (newLineSpacing != textMesh.LineSpacing) UndoableAction( tm => tm.LineSpacing = newLineSpacing );

#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			int sortingOrder = EditorGUILayout.IntField("Sorting Order In Layer", targetTextMeshes[0].SortingOrder);
			if (sortingOrder != targetTextMeshes[0].SortingOrder) {
            	tk2dUndo.RecordObjects(targetTextMeshes, "Sorting Order In Layer");
            	foreach (tk2dTextMesh s in targetTextMeshes) {
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
	            		tk2dUtil.SetDirty(r);
	            	}
	            }

				int sortingOrder = EditorGUILayout.IntField("Order In Layer", targetTextMeshes[0].SortingOrder);
				if (sortingOrder != targetTextMeshes[0].SortingOrder) {
	            	tk2dUndo.RecordObjects(targetTextMeshes, "Order In Layer");
	            	tk2dUndo.RecordObjects(renderers, "Order In Layer");
	            	foreach (tk2dTextMesh s in targetTextMeshes) {
	            		s.SortingOrder = sortingOrder;
	            	}
				}
			}
#endif

			Vector3 newScale = EditorGUILayout.Vector3Field("Scale", textMesh.scale);
			if (newScale != textMesh.scale) UndoableAction( tm => tm.scale = newScale );
			
			if (textMesh.font.textureGradients && textMesh.font.gradientCount > 0)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("TextureGradient");
				
				// Draw gradient scroller
				bool drawGradientScroller = true;
				if (drawGradientScroller)
				{
					textMesh.textureGradient = textMesh.textureGradient % textMesh.font.gradientCount;
					
					gradientScroll = EditorGUILayout.BeginScrollView(gradientScroll, GUILayout.ExpandHeight(false));
					Rect r = GUILayoutUtility.GetRect(textMesh.font.gradientTexture.width, textMesh.font.gradientTexture.height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
					GUI.DrawTexture(r, textMesh.font.gradientTexture);
					
					Rect hr = r;
					hr.width /= textMesh.font.gradientCount;
					hr.x += hr.width * textMesh.textureGradient;
					float ox = hr.width / 8;
					float oy = hr.height / 8;
					Vector3[] rectVerts = { new Vector3(hr.x + 0.5f + ox, hr.y + oy, 0), new Vector3(hr.x + hr.width - ox, hr.y + oy, 0), new Vector3(hr.x + hr.width - ox, hr.y + hr.height -  0.5f - oy, 0), new Vector3(hr.x + ox, hr.y + hr.height - 0.5f - oy, 0) };
					Handles.DrawSolidRectangleWithOutline(rectVerts, new Color(0,0,0,0.2f), new Color(0,0,0,1));
					
					if (GUIUtility.hotControl == 0 && Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
					{
						int newTextureGradient = (int)(Event.current.mousePosition.x / (textMesh.font.gradientTexture.width / textMesh.font.gradientCount));
						if (newTextureGradient != textMesh.textureGradient) {
							UndoableAction( delegate(tk2dTextMesh tm) {
									if (tm.useGUILayout && tm.font != null && newTextureGradient < tm.font.gradientCount) {
										tm.textureGradient = newTextureGradient;
									}
								} );
						}
						GUI.changed = true;
					}
	
					EditorGUILayout.EndScrollView();
				}
				
				
				GUILayout.EndHorizontal();
			}
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("HFlip"))
			{
				UndoableAction( delegate(tk2dTextMesh tm) {
						Vector3 s = tm.scale;
						s.x *= -1.0f;
						tm.scale = s;
					} );
				GUI.changed = true;
			}
			if (GUILayout.Button("VFlip"))
			{
				UndoableAction( delegate(tk2dTextMesh tm) {
					Vector3 s = tm.scale;
					s.y *= -1.0f;
					tm.scale = s;
					} );
				GUI.changed = true;
			}			

			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Bake Scale"))
			{
				tk2dScaleUtility.Bake(textMesh.transform);
				GUI.changed = true;
			}
			
			GUIContent pixelPerfectButton = new GUIContent("1:1", "Make Pixel Perfect");
			if ( GUILayout.Button(pixelPerfectButton ))
			{
				if (tk2dPixelPerfectHelper.inst) tk2dPixelPerfectHelper.inst.Setup();
				UndoableAction( tm => tm.MakePixelPerfect() );
				GUI.changed = true;
			}
			
			EditorGUILayout.EndHorizontal();
			
			if (textMesh.font && !textMesh.font.inst.isPacked)
			{
				bool newUseGradient = EditorGUILayout.Toggle("Use Gradient", textMesh.useGradient);
				if (newUseGradient != textMesh.useGradient) {
					UndoableAction( tm => tm.useGradient = newUseGradient );
				}

				if (textMesh.useGradient)
				{
					Color newColor = EditorGUILayout.ColorField("Top Color", textMesh.color);
					if (newColor != textMesh.color) UndoableAction( tm => tm.color = newColor );

					Color newColor2 = EditorGUILayout.ColorField("Bottom Color", textMesh.color2);
					if (newColor2 != textMesh.color2) UndoableAction( tm => tm.color2 = newColor2 );
				}
				else
				{
					Color newColor = EditorGUILayout.ColorField("Color", textMesh.color);
					if (newColor != textMesh.color) UndoableAction( tm => tm.color = newColor );
				}
			}

			if (GUI.changed)
			{
				foreach (tk2dTextMesh tm in targetTextMeshes) {
					tm.ForceBuild();
					tk2dUtil.SetDirty(tm);
				}
			}
		}
	}

    [MenuItem(tk2dMenu.createBase + "TextMesh", false, 13905)]
    static void DoCreateTextMesh()
    {
		tk2dFontData fontData = null;
		
		// Find reference in scene
        tk2dTextMesh dupeMesh = GameObject.FindObjectOfType(typeof(tk2dTextMesh)) as tk2dTextMesh;
		if (dupeMesh) 
			fontData = dupeMesh.font;
		
		// Find in library
		if (fontData == null)
		{
			tk2dGenericIndexItem[] allFontEntries = tk2dEditorUtility.GetOrCreateIndex().GetFonts();
			foreach (var v in allFontEntries)
			{
				if (v.managed) continue;
				tk2dFontData data = v.GetData<tk2dFontData>();
				if (data != null)
				{
					fontData = data;
					break;
				}
			}
		}
		
		if (fontData == null)
		{
			EditorUtility.DisplayDialog("Create TextMesh", "Unable to create text mesh as no Fonts have been found.", "Ok");
			return;
		}

		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("TextMesh");
        tk2dTextMesh textMesh = go.AddComponent<tk2dTextMesh>();
		textMesh.font = fontData;
		textMesh.text = "New TextMesh";
		textMesh.Commit();
		
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create TextMesh");
    }
}

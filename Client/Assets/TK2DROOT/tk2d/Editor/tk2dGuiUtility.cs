using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class tk2dGuiUtility  
{
	public static bool HasActivePositionHandle { get { return activePositionHandleId != 0; } }
	public static Vector2 ActiveHandlePosition { get { return activePositionHandlePosition; } }
	public static int ActiveTweakable { get; set; }
	
	static int activePositionHandleId = 0;
	static Vector2 activePositionHandlePosition = Vector2.zero;
	static Vector2 positionHandleOffset = Vector2.zero;

	static Vector2 tweakableOffset = Vector2.zero;
	static readonly Color inactiveTweakable = new Color(1, 1, 1, 0.3f);
	static readonly Color selectedTweakable = Color.white;
	static readonly Color hotTweakable = Color.white;

	public static void TweakableCircle(int id, Vector2 pos, float radius, System.Action<Vector2, float> changed) {
		Event ev = Event.current;

		Color c = (GUIUtility.hotControl == id) ? hotTweakable : (ActiveTweakable == id) ? selectedTweakable : inactiveTweakable;
		Handles.color = c;
		Handles.DrawWireDisc(pos, Vector3.forward, radius);
		Handles.color = Color.white;

		int moveId = id;

		if (ActiveTweakable == id) {
			Vector2[] offsets = new Vector2[] { new Vector2(radius, 0), new Vector2(0, radius), new Vector2(-radius, 0), new Vector2(0, -radius) };
			int offsetId = 0;
			foreach (Vector2 offset in offsets) {
				EditorGUI.BeginChangeCheck();
				Vector2 radiusPos = PositionHandle(id + "resize".GetHashCode() + offsetId, pos + offset);
				if (EditorGUI.EndChangeCheck()) {
					Vector2 refPoint = pos - offset;

					Vector2 q = radiusPos - refPoint;
					q = Vector2.Dot(q, offset.normalized) * offset.normalized;
					radiusPos = refPoint + q;

					if (!ev.alt) {
						pos = (refPoint + radiusPos) * 0.5f;
					}
					radius = Mathf.Max((radiusPos - pos).magnitude, 1);
					if (changed != null) {
						changed( pos, radius );
					}
					HandleUtility.Repaint();
				}
				offsetId++;
			}
		}

		int extra = 4;
		if (GUIUtility.hotControl == 0 && ev.type == EventType.MouseDown && (pos - ev.mousePosition).magnitude <= (radius + extra / 2)) {
			int activeId = moveId;
			GUIUtility.hotControl = activeId;
			GUIUtility.keyboardControl = 0;
			ActiveTweakable = id;
			tweakableOffset = ev.mousePosition - pos;
			HandleUtility.Repaint();
			ev.Use();
		}

		if (GUIUtility.hotControl == moveId) {
			if (ev.type == EventType.MouseUp) {
				GUIUtility.hotControl = 0;
				ActiveTweakable = id;
				HandleUtility.Repaint();
			}
			else if (ev.type == EventType.MouseDrag) {
				if (changed != null) {
					changed( ev.mousePosition - tweakableOffset, radius );
				}
				HandleUtility.Repaint();
			}
		}
	}
	
	static Vector2 Rotate(Vector2 v, float angle) {
		float angleRad = angle * Mathf.Deg2Rad;
		float cosa = Mathf.Cos(angleRad);
		float sina = -Mathf.Sin(angleRad);
		return new Vector2( v.x * cosa - v.y * sina, v.x * sina + v.y * cosa );
	}

	public static void TweakableBox(int id, Vector2 pos, Vector2 size, float angle, System.Action<Vector2, Vector2, float> changed) {
		Event ev = Event.current;
		Vector2 extents = size * 0.5f;

		Color c = (GUIUtility.hotControl == id) ? hotTweakable : (ActiveTweakable == id) ? selectedTweakable : inactiveTweakable;
		Handles.color = c;
		Vector2 right = Rotate(new Vector2(1, 0), angle);
		Vector2 up = Rotate(new Vector2(0, 1), angle);
		Vector3[] linePoints = new Vector3[] {
			pos - extents.x * right - extents.y * up,
			pos + extents.x * right - extents.y * up,
			pos + extents.x * right + extents.y * up,
			pos - extents.x * right + extents.y * up,
			pos - extents.x * right - extents.y * up,
		};
		Handles.DrawPolyLine(linePoints);
		Handles.color = Color.white;

		if (ActiveTweakable == id) {
			Vector2[] offsets = new Vector2[] { new Vector2(-0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -0.5f), new Vector2(0, 0.5f) };
			for (int i = 0; i < offsets.Length; ++i) {
				Vector2 offset = offsets[i];
				EditorGUI.BeginChangeCheck();
				Vector2 dir = right * offset.x * size.x + up * offset.y * size.y;
				Vector2 resizePos = PositionHandle(id + "resize".GetHashCode() + i, pos + dir);
				if (EditorGUI.EndChangeCheck()) {
					Vector2 refPoint = pos - dir;
					if (!ev.alt) {
						// reproject to constrain rotation
						Vector2 q = resizePos - refPoint;
						q = Vector2.Dot(q, dir.normalized) * dir.normalized;
						resizePos = refPoint + q;
					}
					Vector2 delta = resizePos - refPoint;
					pos = (refPoint + resizePos) * 0.5f;
					if (offset.x != 0) size.x = (refPoint - resizePos).magnitude;
					else size.y = (refPoint - resizePos).magnitude;
					angle = (-Mathf.Atan2(delta.y, delta.x) + Mathf.Atan2(offset.y, offset.x)) * Mathf.Rad2Deg;
					if (changed != null) {
					 	changed( pos, size, angle );
					}
					HandleUtility.Repaint();
				}
			}
		}

		if (GUIUtility.hotControl == 0 && ev.type == EventType.MouseDown) {
			Vector2 p = Rotate(pos - ev.mousePosition, -angle);
			if (Mathf.Abs(p.x) < size.x * 0.5f && Mathf.Abs(p.y) < size.y * 0.5f) {
				int activeId = id;
				GUIUtility.hotControl = activeId;
				GUIUtility.keyboardControl = 0;
				ActiveTweakable = 0;
				tweakableOffset = ev.mousePosition - pos;
				HandleUtility.Repaint();
			}
		}

		if (GUIUtility.hotControl == id) {
			if (ev.type == EventType.MouseUp) {
				GUIUtility.hotControl = 0;
				ActiveTweakable = id;
				HandleUtility.Repaint();
			}
			else if (ev.type == EventType.MouseDrag) {
				if (changed != null) {
					changed( ev.mousePosition - tweakableOffset, size, angle );
				}
				HandleUtility.Repaint();
			}
		}
	}


	
	public static void SetPositionHandleValue(int id, Vector2 val)
	{
		if (id == activePositionHandleId)
			activePositionHandlePosition = val;
	}
	
	public static Vector2 PositionHandle(int id, Vector2 position)
	{
		return Handle(tk2dEditorSkin.MoveHandle, id, position, false);
	}
	
	public static Vector2 Handle(GUIStyle style, int id, Vector2 position, bool allowKeyboardFocus)
	{
		int handleSize = (int)style.fixedWidth;
		Rect rect = new Rect(position.x - handleSize / 2, position.y - handleSize / 2, handleSize, handleSize);
		int controlID = id;
		
		switch (Event.current.GetTypeForControl(controlID))
		{
			case EventType.MouseDown:
			{
				if (rect.Contains(Event.current.mousePosition))
				{
					activePositionHandleId = id;
					if (allowKeyboardFocus) {
						GUIUtility.keyboardControl = controlID;
					}
					positionHandleOffset = Event.current.mousePosition - position;
					GUIUtility.hotControl = controlID;
					Event.current.Use();
				}
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl == controlID)				
				{
					position = Event.current.mousePosition - positionHandleOffset;
					GUI.changed = true;
					Event.current.Use();					
				}
				break;
			}
			
			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl == controlID)
				{
					activePositionHandleId = 0;
					position = Event.current.mousePosition - positionHandleOffset;
					GUIUtility.hotControl = 0;
					GUI.changed = true;
					Event.current.Use();
				}
				break;
			}
			
			case EventType.Repaint:
			{
				bool selected = (GUIUtility.keyboardControl == controlID ||
								 GUIUtility.hotControl == controlID);
				style.Draw(rect, selected, false, false, false);
				break;
			}
		}
		
		return position;
	}
	
	public enum WarningLevel
	{
		Info,
		Warning,
		Error
	}
	
	/// <summary>
	/// Display a warning box in the current GUI layout.
	/// This is expanded to fit the current GUILayout rect.
	/// </summary>
	public static void InfoBox(string message, WarningLevel warningLevel)
	{
		MessageType messageType = MessageType.None;
		switch (warningLevel)
		{
			case WarningLevel.Info: messageType = MessageType.Info; break;
			case WarningLevel.Warning: messageType = MessageType.Warning; break;
			case WarningLevel.Error: messageType = MessageType.Error; break;
		}

		EditorGUILayout.HelpBox(message, messageType);
	}
	
	/// <summary>
	/// Displays a warning box in the current GUI layout, with buttons.
	/// Returns the index of button pressed, or -1 otherwise.
	/// </summary>
	public static int InfoBoxWithButtons(string message, WarningLevel warningLevel, params string[] buttons)
	{
		InfoBox(message, warningLevel);

		Color oldBackgroundColor = GUI.backgroundColor;
		switch (warningLevel)
		{
		case WarningLevel.Info: GUI.backgroundColor = new Color32(154, 176, 203, 255); break;
		case WarningLevel.Warning: GUI.backgroundColor = new Color32(255, 255, 0, 255); break;
		case WarningLevel.Error: GUI.backgroundColor = new Color32(255, 0, 0, 255); break;
		}

		int buttonPressed = -1;
		if (buttons != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			for (int i = 0; i < buttons.Length; ++i)
			{
				if (GUILayout.Button(buttons[i], EditorStyles.miniButton))
					buttonPressed = i;
			}
			GUILayout.EndHorizontal();
		}
		GUI.backgroundColor = oldBackgroundColor;
		return buttonPressed;
	}

	public enum DragDirection
	{
		Horizontal,
	}
	// Size is the offset into the rect to draw the DragableHandle
	const float resizeBarHotSpotSize = 2.0f;
	public static float DragableHandle(int id, Rect windowRect, float offset, DragDirection direction)
	{
		int controlID = GUIUtility.GetControlID(id, FocusType.Passive);

		Vector2 positionFilter = Vector2.zero;
		Rect controlRect = windowRect;
		switch (direction)
		{
			case DragDirection.Horizontal: 
				controlRect = new Rect(controlRect.x + offset - resizeBarHotSpotSize, 
									   controlRect.y, 
									   resizeBarHotSpotSize * 2 + 1.0f, 
									   controlRect.height); 
				positionFilter.x = 1.0f;
				break;
		}
		EditorGUIUtility.AddCursorRect(controlRect, MouseCursor.ResizeHorizontal);

		if (GUIUtility.hotControl == 0)
		{
			if (Event.current.type == EventType.MouseDown && controlRect.Contains(Event.current.mousePosition))
			{
				GUIUtility.hotControl = controlID;
				Event.current.Use();
			}
		}
		else if (GUIUtility.hotControl == controlID)
		{
			if (Event.current.type == EventType.MouseDrag)
			{
				Vector2 mousePosition = Event.current.mousePosition;
				Vector2 handleOffset = new Vector2((mousePosition.x - windowRect.x) * positionFilter.x, 
												   (mousePosition.y - windowRect.y) * positionFilter.y);
				offset = handleOffset.x + handleOffset.y;
				HandleUtility.Repaint();
			}
			else if (Event.current.type == EventType.MouseUp)
			{
				GUIUtility.hotControl = 0;
			}
		}

		// Debug draw
		// GUI.Box(controlRect, "");

		return offset;
	}
	
	private static bool backupGuiChangedValue = false;
	public static void BeginChangeCheck()
	{
		backupGuiChangedValue = GUI.changed;
		GUI.changed = false;
	}
	
	public static bool EndChangeCheck()
	{
		bool hasChanged = GUI.changed;
		GUI.changed |= backupGuiChangedValue;
		return hasChanged;
	}

	public static void SpriteCollectionSize( tk2dSpriteCollectionSize scs ) {
		GUILayout.BeginHorizontal();
		scs.type = (tk2dSpriteCollectionSize.Type)EditorGUILayout.EnumPopup("Size", scs.type);
		tk2dCamera cam = tk2dCamera.Editor__Inst;
		GUI.enabled = cam != null;
		if (GUILayout.Button(new GUIContent("g", "Grab from tk2dCamera"), EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
			scs.CopyFrom( tk2dSpriteCollectionSize.ForTk2dCamera(cam) );
			GUI.changed = true;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		EditorGUI.indentLevel++;
		switch (scs.type) {
			case tk2dSpriteCollectionSize.Type.Explicit:
				scs.orthoSize = EditorGUILayout.FloatField("Ortho Size", scs.orthoSize);
				scs.height = EditorGUILayout.FloatField("Target Height", scs.height);
				break;
			case tk2dSpriteCollectionSize.Type.PixelsPerMeter:
				scs.pixelsPerMeter = EditorGUILayout.FloatField("Pixels Per Meter", scs.pixelsPerMeter);
				break;
		}
		EditorGUI.indentLevel--;
	}

	public static void LookLikeControls() {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		EditorGUIUtility.LookLikeControls();
#endif
	}
	public static void LookLikeControls(float labelWidth) {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		EditorGUIUtility.LookLikeControls(labelWidth);
#endif
	}
	public static void LookLikeControls(float labelWidth, float fieldWidth) {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		EditorGUIUtility.LookLikeControls(labelWidth, fieldWidth);
#endif
	}
	public static void LookLikeInspector() {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		EditorGUIUtility.LookLikeInspector();
#endif
	}

	public static string PlatformPopup(tk2dSystem system, string label, string platform)
	{
		if (system == null)
			return label;

		int selectedIndex = -1;
		string[] platformNames = new string[system.assetPlatforms.Length];

		for (int i = 0; i < system.assetPlatforms.Length; ++i)
		{
			platformNames[i] = system.assetPlatforms[i].name;
			if (platformNames[i] == platform) selectedIndex = i;
		}

		selectedIndex = EditorGUILayout.Popup(label, selectedIndex, platformNames);
		if (selectedIndex == -1) return "";
		else return platformNames[selectedIndex];
	}

	public static string SaveFileInProject(string title, string directory, string filename, string ext)
	{
		string path = EditorUtility.SaveFilePanel(title, directory, filename, ext);
		if (path.Length == 0) // cancelled
			return "";
		string cwd = System.IO.Directory.GetCurrentDirectory().Replace("\\","/") + "/assets/";
		if (path.ToLower().IndexOf(cwd.ToLower()) != 0)
		{
			path = "";
			EditorUtility.DisplayDialog(title, "Assets must be saved inside the Assets folder", "Ok");
		}
		else 
		{
			path = path.Substring(cwd.Length - "/assets".Length);
		}
		return path;
	}
}

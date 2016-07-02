using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( LongPressRecognizer ) )]
public class LongPressRecognizerInspector : GestureRecognizerInspector<LongPressRecognizer>
{
    protected static GUIContent LABEL_Duration = new GUIContent( "Press Duration", "How long the finger must stay pressed down without moving (in seconds)" );
    protected static GUIContent LABEL_MoveTolerance = new GUIContent( "Move Tolerance", "Distance the finger is allowed to move from its starting position" );

    protected override bool ShowRequiredFingerCount
    {
        get { return true; }
    }

    protected override void OnSettingsUI()
    {
        base.OnSettingsUI();

        Gesture.Duration = EditorGUILayout.FloatField( LABEL_Duration, Gesture.Duration );
        Gesture.MoveTolerance = DistanceField( LABEL_MoveTolerance, Gesture.MoveTolerance );
    }

    protected override void ValidateValues()
    {
        base.ValidateValues();
        Gesture.Duration = Mathf.Max( 0.001f, Gesture.Duration );
        Gesture.MoveTolerance = Mathf.Max( 0, Gesture.MoveTolerance );
    }
}

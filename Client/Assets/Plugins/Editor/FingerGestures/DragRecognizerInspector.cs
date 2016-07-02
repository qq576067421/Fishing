using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( DragRecognizer ) )]
public class DragRecognizerInspector : GestureRecognizerInspector<DragRecognizer>
{
    protected static GUIContent LABEL_MoveTolerance = new GUIContent( "Movement Tolerance", "How far the finger can move from its initial position without starting the drag gesture" );
    protected static GUIContent LABEL_ApplySameDirectionConstraint = new GUIContent( "Apply Same Direction Constraint", "Enable this if you want the gesture to fail when the fingers are not moving in the same direction.\n\nValid for multi-finger drag gestures only (RequiredFingerCount >= 2)." );
    
    protected override bool ShowRequiredFingerCount
    {
        get { return true; }
    }

    protected override void OnSettingsUI()
    {
        base.OnSettingsUI();
        Gesture.MoveTolerance = DistanceField( LABEL_MoveTolerance, Gesture.MoveTolerance ); //EditorGUILayout.FloatField( LABEL_MoveTolerance, Gesture.MoveTolerance );

        GUI.enabled = ( Gesture.RequiredFingerCount > 1 );
        Gesture.ApplySameDirectionConstraint = EditorGUILayout.Toggle( LABEL_ApplySameDirectionConstraint, Gesture.ApplySameDirectionConstraint );
        GUI.enabled = true;
    }

    protected override void ValidateValues()
    {
        base.ValidateValues();
        Gesture.MoveTolerance = Mathf.Max( 0, Gesture.MoveTolerance );
    }
}

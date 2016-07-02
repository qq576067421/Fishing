using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( SwipeRecognizer ) )]
public class SwipeRecognizerInspector : GestureRecognizerInspector<SwipeRecognizer>
{
    protected static GUIContent LABEL_MinDistance = new GUIContent( "Min Distance", "Minimum distance the finger must travel in order to produce a valid swipe" );
    protected static GUIContent LABEL_MaxDistance = new GUIContent( "Max Distance", "Finger travel distance beyond which the swipe recognition will fail.\nSetting this to 0 disables the constraint" );
    protected static GUIContent LABEL_MinVelocity = new GUIContent( "Min Velocity", "Minimum speed the finger must maintain in order to produce a valid swipe gesture" );
    protected static GUIContent LABEL_MaxDeviation = new GUIContent( "Max Deviation", "Maximum angle that the swipe direction is allowed to deviate from its initial direction (in degrees)" );

    protected override bool ShowRequiredFingerCount
    {
        get { return true; }
    }

    protected override void OnSettingsUI()
    {
        base.OnSettingsUI();

        Gesture.MinDistance = DistanceField( LABEL_MinDistance, Gesture.MinDistance );
        Gesture.MaxDistance = DistanceField( LABEL_MaxDistance, Gesture.MaxDistance );
        Gesture.MinVelocity = DistanceField( LABEL_MinVelocity, Gesture.MinVelocity, "/s" );
        Gesture.MaxDeviation = EditorGUILayout.FloatField( LABEL_MaxDeviation, Gesture.MaxDeviation );
    }

    protected override void ValidateValues()
    {
        base.ValidateValues();
        Gesture.MinDistance = Mathf.Max( 0.01f, Gesture.MinDistance );
        Gesture.MaxDistance = Mathf.Max( 0, Gesture.MaxDistance );
        Gesture.MinVelocity = Mathf.Max( 0, Gesture.MinVelocity );
        Gesture.MaxDeviation = Mathf.Max( 0, Gesture.MaxDeviation );
    }
}

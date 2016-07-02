using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( TapRecognizer ) )]
public class TapRecognizerInspector : GestureRecognizerInspector<TapRecognizer>
{
    protected static GUIContent LABEL_RequiredTaps = new GUIContent( "Required Taps", "How many consecutive taps are required to recognize the gesture." );
    protected static GUIContent LABEL_MoveTolerance = new GUIContent( "Movement Tolerance", "How far the finger can move from its initial position without making the gesture fail" );
    protected static GUIContent LABEL_MaxDelayBetweenTaps = new GUIContent( "> Max Delay Between Taps", "The maximum amount of the time that can elapse between two consecutive taps without causing the recognizer to reset.\nSet to 0 to ignore this setting." );
    protected static GUIContent LABEL_MaxDuration = new GUIContent( "Max Duration", "Maximum amount of time the fingers can be held down without failing the gesture.\nSet to 0 for infinite duration." );
    
    protected override bool ShowRequiredFingerCount
    {
        get { return true; }
    }

    protected override void OnSettingsUI()
    {
        base.OnSettingsUI();

        Gesture.RequiredTaps = EditorGUILayout.IntField( LABEL_RequiredTaps, Gesture.RequiredTaps );

        GUI.enabled = ( Gesture.RequiredTaps > 1 );
        EditorGUI.indentLevel++;
        Gesture.MaxDelayBetweenTaps = EditorGUILayout.FloatField( LABEL_MaxDelayBetweenTaps, Gesture.MaxDelayBetweenTaps );
        EditorGUI.indentLevel--;
        GUI.enabled = true;
        Gesture.MoveTolerance = DistanceField( LABEL_MoveTolerance, Gesture.MoveTolerance );
        
        Gesture.MaxDuration = EditorGUILayout.FloatField( LABEL_MaxDuration, Gesture.MaxDuration );       
    }

    protected override void ValidateValues()
    {
        base.ValidateValues();
        Gesture.RequiredTaps = Mathf.Max( 1, Gesture.RequiredTaps );
        Gesture.MoveTolerance = Mathf.Max( 0, Gesture.MoveTolerance );
        Gesture.MaxDelayBetweenTaps = Mathf.Max( 0, Gesture.MaxDelayBetweenTaps );
        Gesture.MaxDuration = Mathf.Max( 0, Gesture.MaxDuration );
    }

    protected override void OnNotices()
    {
        string multiTapName = string.Empty;

        if( Gesture.RequiredFingerCount > 1 )
            multiTapName += "multi-finger, ";

        if( Gesture.RequiredTaps == 1 )
            multiTapName += "single-tap";
        else if( Gesture.RequiredTaps == 2 )
            multiTapName += "double-tap";
        else if( Gesture.RequiredTaps == 3 )
            multiTapName += "triple-tap";
        else
            multiTapName += "multi-tap";

        EditorGUILayout.HelpBox( "Configured as a " + multiTapName + " gesture recognizer", MessageType.Info );

        base.OnNotices();
    }
}

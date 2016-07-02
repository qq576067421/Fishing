using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( FingerMotionDetector ) )]
public class FingerMotionDetectorInspector : FingerEventDetectorInspector<FingerMotionDetector>
{
    protected override void OnSettingsUI()
    {
        base.OnSettingsUI();

        Detector.TrackMove = EditorGUILayout.Toggle( "Track Move", Detector.TrackMove );
        Detector.TrackStationary = EditorGUILayout.Toggle( "Track Stationary", Detector.TrackStationary );
    }

    protected override void MessageEventsGUI()
    {
        GUI.enabled = Detector.TrackMove;
        Detector.MoveMessageName = EditorGUILayout.TextField( "Move Message Name", Detector.MoveMessageName );
        GUI.enabled = Detector.TrackStationary;
        Detector.StationaryMessageName = EditorGUILayout.TextField( "Stationary Message Name", Detector.StationaryMessageName );
        GUI.enabled = true;
    }

    protected override void OnToolbar()
    {
        base.OnToolbar();
        EventMessageToolbarButton( "Copy [Move] Event Signature To Clipboard", Detector.MoveMessageName );
        EventMessageToolbarButton( "Copy [Stationary] Event Signature To Clipboard", Detector.StationaryMessageName );
    }
}

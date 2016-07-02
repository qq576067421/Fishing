using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( FingerHoverDetector ) )]
public class FingerHoverDetectorInspector : FingerEventDetectorInspector<FingerHoverDetector>
{
    protected override void MessageEventsGUI()
    {
        Detector.MessageName = EditorGUILayout.TextField( "Message Name", Detector.MessageName );
    }

    protected override void OnToolbar()
    {
        base.OnToolbar();
        EventMessageToolbarButton( "Copy Event Signature To Clipboard", Detector.MessageName );
    }
}

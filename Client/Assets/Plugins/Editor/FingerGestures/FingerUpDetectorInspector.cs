using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( FingerUpDetector ) )]
public class FingerUpDetectorInspector : FingerEventDetectorInspector<FingerUpDetector>
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

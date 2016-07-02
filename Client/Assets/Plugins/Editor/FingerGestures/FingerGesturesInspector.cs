using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor( typeof( FingerGestures ) )]
public class FingerGesturesInspector : Editor
{
    public override void OnInspectorGUI()
    {
#if UNITY_3_5
        EditorGUIUtility.LookLikeInspector();
#endif

        base.OnInspectorGUI();

        if( Application.isPlaying )
        {
            GUILayout.Space( 10 );

            GUILayout.Label( "Registered Gesture Recognizers: " + FingerGestures.RegisteredGestureRecognizers.Count );
            foreach( GestureRecognizer recognizer in FingerGestures.RegisteredGestureRecognizers )
                EditorGUILayout.ObjectField( recognizer.EventMessageName + " - " + recognizer.GetType().Name, recognizer, typeof( GestureRecognizer ), true );            
        }
    }
}

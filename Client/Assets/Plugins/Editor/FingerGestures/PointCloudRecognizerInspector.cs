using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor( typeof( PointCloudRegognizer ) )]
public class PointCloudRecognizerInspector : GestureRecognizerInspector<PointCloudRegognizer>
{
    protected static GUIContent LABEL_Templates = new GUIContent( "Gesture Templates List", "List of gesture templates that will be matched against the user's gesture" );
    protected static GUIContent LABEL_MinDistanceBetweenSamples = new GUIContent( "Sampling Distance", "Minimum distance between two consecutive finger position samples. Smaller means more accurate recording of the gesture, but more samples to process." );
    protected static GUIContent LABEL_MaxMatchDistance = new GUIContent( "Max Match Distance", "Threshold value that controls how accurate the user-generated gesture must be in order to match its corresponding template gesture. The lower the value, the more accurate the user must be." );

    protected override bool ShowRequiredFingerCount
    {
        get { return true; }
    }

    protected override void OnSettingsUI()
    {
        base.OnSettingsUI();
        
        GUILayout.Space( 10 );

        Gesture.MaxMatchDistance = EditorGUILayout.FloatField( LABEL_MaxMatchDistance, Gesture.MaxMatchDistance );
        Gesture.MinDistanceBetweenSamples = EditorGUILayout.FloatField( LABEL_MinDistanceBetweenSamples, Gesture.MinDistanceBetweenSamples );

        serializedObject.Update();
        if( Gesture.Templates == null )
        {
            Gesture.Templates = new List<PointCloudGestureTemplate>();
            EditorUtility.SetDirty( Gesture );
        }

        EditorGUILayout.PropertyField( serializedObject.FindProperty( "Templates" ), LABEL_Templates, true );
        serializedObject.ApplyModifiedProperties();
    }

    protected override void ValidateValues()
    {
        base.ValidateValues();
        Gesture.MinDistanceBetweenSamples = Mathf.Max( 1.0f, Gesture.MinDistanceBetweenSamples );
        Gesture.MaxMatchDistance = Mathf.Max( 0.1f, Gesture.MaxMatchDistance );
    }

    protected override void OnToolbar()
    {
        base.OnToolbar();

        if( GUILayout.Button( "New Gesture Template" ) )
        {
            PointCloudGestureTemplate template = FingerGesturesEditorUtils.CreateAsset<PointCloudGestureTemplate>();
            Gesture.Templates.Add( template );
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor( typeof( PointCloudGestureTemplate ) )]
public class PointCloudGestureTemplateInspector : Editor
{
    public const float GestureEditorSize = 400;

    [MenuItem( "Assets/Create/PointCloud Gesture Template" )]
    public static void CreatePointCloudGesture()
    {
        PointCloudGestureTemplate template = FingerGesturesEditorUtils.CreateAsset<PointCloudGestureTemplate>();
        FingerGesturesEditorUtils.SelectAssetInProjectView( template );
    }

    public override void OnInspectorGUI()
    {
#if UNITY_3_5
        EditorGUIUtility.LookLikeInspector();
#endif

        PointCloudGestureTemplate template = target as PointCloudGestureTemplate;

        if( GUILayout.Button( "Edit", GUILayout.Height( 50 ) ) )
            PointCloudGestureEditor.Open( template );

        /*
        if( GUILayout.Button( "Triangle" ) )
        {
            template.BeginPoints();
            template.AddPoint( 0, 1, 1 );
            template.AddPoint( 0, 2, 2 );
            template.AddPoint( 0, 3, 1 );
            template.AddPoint( 0, 1, 1 );
            template.EndPoints();          
        }

        if( GUILayout.Button( "Square" ) )
        {
            template.BeginPoints();
            template.AddPoint( 0, 2, 1 );
            template.AddPoint( 0, 2, 3 );
            template.AddPoint( 0, 4, 3 );
            template.AddPoint( 0, 4, 1 );
            template.AddPoint( 0, 2, 1 );
            template.EndPoints();
        }*/
    }

    static GUIContent previewTitle = new GUIContent( "Gesture Preview" );

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override GUIContent GetPreviewTitle()
    {
        return previewTitle;
    }

    public override void OnPreviewSettings()
    {
        base.OnPreviewSettings();

        PointCloudGestureTemplate template = target as PointCloudGestureTemplate;
        GUILayout.Label( template.PointCount + " points, " + template.StrokeCount + " stroke(s)" );
    }

    public override void OnPreviewGUI( Rect r, GUIStyle background )
    {
        base.OnPreviewGUI( r, background );

        float size = 0.95f * Mathf.Min( r.width, r.height );
        Rect canvasRect = new Rect( r.center.x - 0.5f * size, r.center.y - 0.5f * size, size, size );
        
        PointCloudGestureTemplate template = target as PointCloudGestureTemplate;

        Vector2 center = canvasRect.center;

        float scale = 0.95f * size;

        Handles.color = Color.white;
        for( int i = 1; i < template.PointCount; ++i )
        {
            Vector2 p1 = template.GetPosition( i - 1 );
            Vector2 p2 = template.GetPosition( i );
            
            p1.y = -p1.y;
            p2.y = -p2.y;
            
            p1 = center + scale * p1;
            p2 = center + scale * p2;

            if( canvasRect.width > 100 )
            {
                float handleSize = canvasRect.width / 200.0f;
                Handles.CircleCap( 0, p1, Quaternion.identity, handleSize );
            }

            Handles.DrawLine( p1, p2 );
        }        
    }
}

using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

public static class FingerGesturesEditorUtils
{
    public static void SetClipboard( string value )
    {
        System.Type T = typeof( GUIUtility );

        PropertyInfo systemCopyBufferProperty = T.GetProperty( "systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic );

        if( systemCopyBufferProperty == null )
            throw new System.Exception( "Can't access clipboard object." );

        systemCopyBufferProperty.SetValue( null, value, null );
    }

    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static T CreateAsset<T>() where T : ScriptableObject
    {
        string path = AssetDatabase.GetAssetPath( Selection.activeObject );

        if( path == "" )
            path = "Assets";
        else if( Path.GetExtension( path ) != "" )
            path = path.Replace( Path.GetFileName( path ), "" );

        return CreateAsset<T>( path, "New " + typeof( T ).ToString() );
    }

    public static T CreateAsset<T>( string path, string name ) where T : ScriptableObject
    {
        if( string.IsNullOrEmpty( path ) )
            path = "Assets";

        if( !name.EndsWith( ".asset" ) )
            name += ".asset";
                
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( path + "/" + name );

        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset( asset, assetPathAndName );
        AssetDatabase.SaveAssets();

        return asset;
    }

    public static void SelectAssetInProjectView( Object asset )
    {
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}

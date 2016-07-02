using UnityEditor;
using UnityEngine;
using System.Reflection; // for clipboard stuff

public abstract class FingerEventDetectorInspector<T> : Editor where T : FingerEventDetector
{
    protected static GUIContent LABEL_FingerIndexFilter = new GUIContent( "Finger Index Filter", "Filters out all active fingers except the one with this index. Settings this to -1 will track any active finger (default)." );

    protected static GUIContent LABEL_EventMessageSectionTitle = new GUIContent( "Event Message Broadcasting", "Configure the broadcasting of finger event via Unity's SendMessage() API" );
    protected static GUIContent LABEL_EventMessageName = new GUIContent( "Message Name", "Name/identifier of the method to invoke on the target object when this finger event fires" );
    protected static GUIContent LABEL_EventMessageTarget = new GUIContent( "Message Target", "The target game object to send the finger event message to.\nDefaults to current object if no value is provided." );
    protected static GUIContent LABEL_SendMessageToSelection = new GUIContent( "Send Message To Selection", "Check this if you also want to forward the event to the object under the finger" );
    
    protected static GUIContent LABEL_Raycaster = new GUIContent( "Raycaster", "ScreenRaycaster component used to pick collider-equipped scene objects.\nDefaults to first ScreenRaycaster component on current object if no value is provided." );
    
    protected static GUIStyle SectionTitleStyle;

    static bool stylesInitialized = false;

    static void InitStyles()
    {
        SectionTitleStyle = new GUIStyle( GUI.skin.label );
        SectionTitleStyle.fontStyle = FontStyle.Bold;
    }

    T detector;

    protected virtual void ValidateValues() 
    {
        
    }
    
    public T Detector
    {
        get { return detector; }
    }

    public override void OnInspectorGUI()
    {
        if( !stylesInitialized )
        {
            InitStyles();
            stylesInitialized = true;
        }

        detector = (T)target;

#if UNITY_3_5
        EditorGUIUtility.LookLikeInspector();
#endif

        GUILayout.Space( 5 );

        OnSettingsUI();
			
        GUILayout.Space( 10 );

        OnMessagingUI();

        GUILayout.Space( 10 );

        OnComponentsUI();

        GUILayout.Space( 10 );

        OnToolbar();      

        GUILayout.Space( 5 );

        if( GUI.changed )
        {
            ValidateValues();
            EditorUtility.SetDirty( target );
        }
                
    }

    protected void UISectionTitle( string title )
    {
        GUILayout.Label( title, SectionTitleStyle );
    }

    protected void UISectionTitle( GUIContent title )
    {
        GUILayout.Label( title, SectionTitleStyle );
    }
        
    protected virtual void OnSettingsUI() 
    {
        UISectionTitle( "Settings" );

        Detector.FingerIndexFilter = EditorGUILayout.IntField( LABEL_FingerIndexFilter, Detector.FingerIndexFilter );
    }
		
    protected virtual void OnMessagingUI()
    {
        UISectionTitle( LABEL_EventMessageSectionTitle );
        Detector.UseSendMessage = EditorGUILayout.Toggle( "Enable Message Broadcast", Detector.UseSendMessage );
        GUI.enabled = Detector.UseSendMessage;
        MessageEventsGUI();
        Detector.MessageTarget = EditorGUILayout.ObjectField( LABEL_EventMessageTarget, Detector.MessageTarget, typeof( GameObject ), true ) as GameObject;
        Detector.SendMessageToSelection = EditorGUILayout.Toggle( LABEL_SendMessageToSelection, Detector.SendMessageToSelection );
        GUI.enabled = true;
        //gesture.Mess = EditorGUILayout.TextField( gesture.MessageTarget, "Broadcast Messages" );
    }

    protected abstract void MessageEventsGUI();
    
    protected virtual void OnComponentsUI()
    {
        UISectionTitle( "Components" );
        Detector.Raycaster = EditorGUILayout.ObjectField( LABEL_Raycaster, Detector.Raycaster, typeof( ScreenRaycaster ), true ) as ScreenRaycaster;
    }

    public void EventMessageToolbarButton( string label, string eventName )
    {
        bool ret = GUILayout.Button( label );
        if( ret )
        {
            string value = string.Format( "void {0}({1} e)", eventName, Detector.GetEventType().Name );
            value += @" { /* your code here */ }";
            FingerGesturesEditorUtils.SetClipboard( value );
            Debug.Log( value );
        }
    }

    protected virtual void OnToolbar()
    {

    }
}

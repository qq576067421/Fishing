using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#region Enums

public enum GestureRecognitionState
{
    /// <summary>
    /// The gesture recognizer is ready and waiting for the correct initial input conditions to begin
    /// </summary>
    Ready,

    /// <summary>
    /// The gesture recognition has just begun
    /// </summary>
    Started,

    /// <summary>
    /// The gesture is still ongoing and recognizer state has changed since last frame
    /// </summary>
    InProgress,

    /// <summary>
    /// The gesture detected a user input that invalidated it
    /// </summary>
    Failed,

    /// <summary>
    /// The gesture was succesfully recognized (used by continuous gestures)
    /// </summary>
    Ended,

    /// <summary>
    /// The gesture was succesfully recognized (used by discreet gestures)
    /// </summary>
    Recognized = Ended,

    /* ----------- INTERNAL -------------- */

    /// <summary>
    /// FOR INTERNAL USE ONLY (not an actual state)
    /// Used to tell the gesture to fail and immeditaly retry recognition (used by multi-tap)
    /// </summary>
    FailAndRetry,
}

/// <summary>
/// The reset mode determines when to reset a GestureRecognizer after it fails or succeeds (GestureState.Failed or GestureState.Recognized)
/// </summary>
public enum GestureResetMode
{
    /// <summary>
    /// Use the recommended value for this gesture recognizer
    /// </summary>
    Default,

    /// <summary>
    /// The gesture recognizer will reset on the next Update()
    /// </summary>
    NextFrame,

    /// <summary>
    /// The gesture recognizer will reset at the end of the current multitouch sequence
    /// </summary>
    EndOfTouchSequence,
}

#endregion

public abstract class Gesture
{
    public delegate void EventHandler( Gesture gesture );
    public event EventHandler OnStateChanged;

    // finger cluster
    internal int ClusterId = 0;

    GestureRecognizer recognizer;
    float startTime = 0;
    Vector2 startPosition = Vector2.zero;
    Vector2 position = Vector2.zero;
    GestureRecognitionState state = GestureRecognitionState.Ready;
    GestureRecognitionState prevState = GestureRecognitionState.Ready;
    FingerGestures.FingerList fingers = new FingerGestures.FingerList();

    /// <summary>
    /// Convenience operator - so you can go if( !gesture ) instead of if( gesture == null ) 
    /// </summary>
    public static implicit operator bool( Gesture gesture )
    {
        return gesture != null;
    }

    /// <summary>
    /// The fingers that began the gesture
    /// </summary>
    public FingerGestures.FingerList Fingers
    {
        get { return fingers; }
        internal set { fingers = value; }
    }

    /// <summary>
    /// The gesture recognizer that owns this gesture
    /// </summary>
    public GestureRecognizer Recognizer
    {
        get { return recognizer; }
        internal set { recognizer = value; }
    }

    /// <summary>
    /// Time at which gesture recognition started
    /// </summary>
    public float StartTime
    {
        get { return startTime; }
        internal set { startTime = value; }
    }

    /// <summary>
    /// Averaged start position
    /// </summary>
    public Vector2 StartPosition
    {
        get { return startPosition; }
        internal set { startPosition = value; }
    }

    /// <summary>
    /// Averaged current position
    /// </summary>
    public Vector2 Position
    {
        get { return position; }
        internal set { position = value; }
    }

    /// <summary>
    /// Get or set the current gesture state
    /// </summary>
    public GestureRecognitionState State
    {
        get { return state; }
        set
        {
            if( state != value )
            {
                prevState = state;
                state = value;

                if( OnStateChanged != null )
                    OnStateChanged( this );
            }
        }
    }

    /// <summary>
    /// Get the previous gesture state
    /// </summary>
    public GestureRecognitionState PreviousState
    {
        get { return prevState; }
    }

    /// <summary>
    /// Amount of time elapsed since the gesture recognition started (in seconds)
    /// </summary>
    public float ElapsedTime
    {
        get { return Time.time - StartTime; }
    }

    #region Object Picking / Raycasting

    GameObject startSelection;  // object picked at StartPosition
    GameObject selection;       // object picked at current Position
    ScreenRaycastData lastRaycast = new ScreenRaycastData();

    /// <summary>
    /// GameObject that was at the gesture start position
    /// </summary>
    public GameObject StartSelection
    {
        get { return startSelection; }
        internal set { startSelection = value; }
    }

    /// <summary>
    /// GameObject currently located at this gesture position
    /// </summary>
    public GameObject Selection
    {
        get { return selection; }
        internal set { selection = value; }
    }

    /// <summary>
    /// Last raycast hit result
    /// </summary>
    public ScreenRaycastData Raycast
    {
        get { return lastRaycast; }
        internal set { lastRaycast = value; }
    }

    internal GameObject PickObject( ScreenRaycaster raycaster, Vector2 screenPos )
    {
        if( !raycaster || !raycaster.enabled )
            return null;

        if( !raycaster.Raycast( screenPos, out lastRaycast ) )
            return null;

        return lastRaycast.GameObject;
    }

    internal void PickStartSelection( ScreenRaycaster raycaster )
    {
        StartSelection = PickObject( raycaster, StartPosition );
        Selection = StartSelection;
    }

    internal void PickSelection( ScreenRaycaster raycaster )
    {
        Selection = PickObject( raycaster, Position );
    }

    #endregion
}

/// <summary>
/// Type-safe/generic version of GestureRecognizer base class
/// </summary>
public abstract class GestureRecognizerTS<T> : GestureRecognizer where T : Gesture, new()
{
    List<T> gestures;

    public delegate void GestureEventHandler( T gesture );
    public event GestureEventHandler OnGesture;

    protected override void Start()
    {
        base.Start();
        InitGestures();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // support recompilation while running
    #if UNITY_EDITOR
        InitGestures(); 
    #endif
    }

    void InitGestures()
    {
        if( gestures == null )
        {
            gestures = new List<T>();

            for( int i = 0; i < MaxSimultaneousGestures; ++i )
                AddGesture();
        }
    }

    protected T AddGesture()
    {
        T gesture = CreateGesture();
        gesture.Recognizer = this;
        gesture.OnStateChanged += OnStateChanged;
        gestures.Add( gesture );
        return gesture;
    }

    /// <summary>
    /// Get the gestures list (not all of them are necessarily active)
    /// </summary>
    public List<T> Gestures
    {
        get { return gestures; }
    }

    /// <summary>
    /// This controls whether or not the gesture recognition should begin
    /// </summary>
    /// <param name="touches">The active touches</param>
    protected virtual bool CanBegin( T gesture, FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
            return false;

        if( IsExclusive && FingerGestures.Touches.Count != RequiredFingerCount )
            return false;

        // check with the delegate (provided we have one set)
        if( Delegate && Delegate.enabled && !Delegate.CanBegin( gesture, touches ) )
            return false;

        return true;
    }

    /// <summary>
    /// Method called when the gesture recognizer has just started recognizing a valid gesture
    /// </summary>
    /// <param name="touches">The active touches</param>
    protected abstract void OnBegin( T gesture, FingerGestures.IFingerList touches );

    /// <summary>
    /// Method called on each frame that the gesture recognizer is in an active state
    /// </summary>
    /// <param name="touches">The active touches</param>
    /// <returns>The new state the gesture recognizer should be in</returns>
    protected abstract GestureRecognitionState OnRecognize( T gesture, FingerGestures.IFingerList touches );

    /// <summary>
    /// Return the default target used when sending gesture event notifications to selected object
    /// </summary>
    protected virtual GameObject GetDefaultSelectionForSendMessage( T gesture ) { return gesture.Selection; }

    /// <summary>
    /// Instantiate a new gesture object
    /// </summary>
    protected virtual T CreateGesture()
    {
        return new T();
    }

    public override System.Type GetGestureType()
    {
        return typeof( T );
    }

    protected virtual void OnStateChanged( Gesture gesture )
    {
        //Debug.Log( this.GetType().Name + " changed state from " + gesture.PreviousState + " to " + gesture.State );
    }

    protected virtual T FindGestureByCluster( FingerClusterManager.Cluster cluster )
    {
        return gestures.Find( g => g.ClusterId == cluster.Id );
    }

    protected virtual T MatchActiveGestureToCluster( FingerClusterManager.Cluster cluster )
    {
        return null;
    }

    protected virtual T FindFreeGesture()
    {
        return gestures.Find( g => g.State == GestureRecognitionState.Ready );
    }

    protected virtual void Reset( T gesture )
    {
        ReleaseFingers( gesture );

        gesture.ClusterId = 0;
        gesture.Fingers.Clear();
        gesture.State = GestureRecognitionState.Ready;
    }

    #region Updates

    static FingerGestures.FingerList tempTouchList = new FingerGestures.FingerList();

    public virtual void Update()
    {
        if( IsExclusive )
        {
            UpdateExclusive();
        }
        else if( RequiredFingerCount == 1 )
        {
            UpdatePerFinger();
        }
        else // 2+ fingers
        {
            if( SupportFingerClustering && ClusterManager )
                UpdateUsingClusters();
            else
                UpdateExclusive();
        }
    }

    // consider all the current touches
    void UpdateExclusive()
    {
        // only one gesture to track
        T gesture = gestures[0];

        FingerGestures.IFingerList touches = FingerGestures.Touches;

        if( gesture.State == GestureRecognitionState.Ready )
        {
            if( CanBegin( gesture, touches ) )
                Begin( gesture, 0, touches );
        }

        UpdateGesture( gesture, touches );
    }

    // consider each touch individually, independently of the rest
    void UpdatePerFinger()
    {
        for( int i = 0; i < FingerGestures.Instance.MaxFingers && i < MaxSimultaneousGestures; ++i )
        {
            FingerGestures.Finger finger = FingerGestures.GetFinger( i );
            T gesture = gestures[i];

            FingerGestures.FingerList touches = tempTouchList;
            touches.Clear();

            if( finger.IsDown )
                touches.Add( finger );

            if( gesture.State == GestureRecognitionState.Ready )
            {
                if( CanBegin( gesture, touches ) )
                    Begin( gesture, 0, touches );
            }

            UpdateGesture( gesture, touches );
        }
    }

    // use the finger clusters as touch list sources (used for handling simultaneous multi-finger gestures)
    void UpdateUsingClusters()
    {
        // force cluster manager to update now (ensures we have most up to date finger state)
        ClusterManager.Update();

        for( int i = 0; i < ClusterManager.Clusters.Count; ++i )
            ProcessCluster( ClusterManager.Clusters[i] );

        for( int i = 0; i < gestures.Count; ++i )
        {
            T g = gestures[i];
            FingerClusterManager.Cluster cluster = ClusterManager.FindClusterById( g.ClusterId );
            FingerGestures.IFingerList touches = ( cluster != null ) ? cluster.Fingers : EmptyFingerList;
            UpdateGesture( g, touches );
        }
    }

    protected virtual void ProcessCluster( FingerClusterManager.Cluster cluster )
    {
        // this cluster already has a gesture associated to it
        if( FindGestureByCluster( cluster ) != null )
            return;

        // only consider clusters that match our gesture's required finger count
        if( cluster.Fingers.Count != RequiredFingerCount )
            return;

        // give a chance to an active gesture to claim that cluster
        T gesture = MatchActiveGestureToCluster( cluster );

        // found an active gesture to rebind the cluster to
        if( gesture != null )
        {
            //Debug.Log( "Gesture " + gesture + " claimed finger cluster #" + cluster.Id );

            // reassign cluster id
            gesture.ClusterId = cluster.Id;
        }
        else
        {
            // no claims - find an inactive gesture
            gesture = FindFreeGesture();

            // out of gestures
            if( gesture == null )
                return;

            // did we recognize the beginning a valid gesture?
            if( !CanBegin( gesture, cluster.Fingers ) )
                return;

            Begin( gesture, cluster.Id, cluster.Fingers );
        }
    }

    #endregion

    void ReleaseFingers( T gesture )
    {
        for( int i = 0; i < gesture.Fingers.Count; ++i )
            Release( gesture.Fingers[i] );
    }

    void Begin( T gesture, int clusterId, FingerGestures.IFingerList touches )
    {
        //Debug.Log( "Beginning " + this.GetType().Name );

        gesture.ClusterId = clusterId;
        gesture.StartTime = Time.time;

        // sanity check
#if UNITY_EDITOR
        if( gesture.Fingers.Count > 0 )
            Debug.LogWarning( this.name + " begin gesture with fingers list not properly released" );
#endif

        for( int i = 0; i < touches.Count; ++i )
        {
            FingerGestures.Finger finger = touches[i];
            gesture.Fingers.Add( finger );
            Acquire( finger );
        }

        OnBegin( gesture, touches );

        gesture.PickStartSelection( Raycaster );
        gesture.State = GestureRecognitionState.Started;
    }

    protected virtual FingerGestures.IFingerList GetTouches( T gesture )
    {
        if( SupportFingerClustering && ClusterManager )
        {
            FingerClusterManager.Cluster cluster = ClusterManager.FindClusterById( gesture.ClusterId );
            return ( cluster != null ) ? cluster.Fingers : EmptyFingerList;
        }

        return FingerGestures.Touches;
    }

    protected virtual void UpdateGesture( T gesture, FingerGestures.IFingerList touches )
    {
        if( gesture.State == GestureRecognitionState.Ready )
            return;

        if( gesture.State == GestureRecognitionState.Started )
            gesture.State = GestureRecognitionState.InProgress;

        switch( gesture.State )
        {
            case GestureRecognitionState.InProgress:
                {
                    GestureRecognitionState newState = OnRecognize( gesture, touches );

                    if( newState == GestureRecognitionState.FailAndRetry )
                    {
                        // special case for MultiTap when the Nth tap in the sequence is performed out of the tolerance radius,
                        //  fail the gesture and immeditaly reattempt a Begin() on it using current touch data

                        // this will trigger the fail event
                        gesture.State = GestureRecognitionState.Failed;

                        // save the clusterId we're currently assigned to (reset will clear it)
                        int clusterId = gesture.ClusterId;

                        // reset gesture state
                        Reset( gesture );

                        // attempt to restart recognition right away with current touch data
                        if( CanBegin( gesture, touches ) )
                            Begin( gesture, clusterId, touches );
                    }
                    else
                    {
                        if( newState == GestureRecognitionState.InProgress )
                        {
                            gesture.PickSelection( Raycaster );
                        }
                     
                        gesture.State = newState;
                    }
                }
                break;

            case GestureRecognitionState.Recognized: // Ended
            case GestureRecognitionState.Failed:
                {
                    // release the fingers right away so another recognizer can use them, even though this one isn't reset yet
                    if( gesture.PreviousState != gesture.State ) // only do this the first time we enter this state
                        ReleaseFingers( gesture );

                    // check if we should reset the gesture now
                    if( ResetMode == GestureResetMode.NextFrame || ( ResetMode == GestureResetMode.EndOfTouchSequence && touches.Count == 0 ) )
                        Reset( gesture );
                }
                break;

            default:
                Debug.LogError( this + " - Unhandled state: " + gesture.State + ". Failing gesture." );
                gesture.State = GestureRecognitionState.Failed;
                break;
        }
    }

    protected void RaiseEvent( T gesture )
    {
        if( OnGesture != null )
            OnGesture( gesture );

        FingerGestures.FireEvent( gesture );

        if( UseSendMessage && !string.IsNullOrEmpty( EventMessageName ) )
        {
            if( EventMessageTarget )
                EventMessageTarget.SendMessage( EventMessageName, gesture, SendMessageOptions.DontRequireReceiver );

            if( SendMessageToSelection != SelectionType.None )
            {
                GameObject sel = null;

                switch( SendMessageToSelection )
                {
                    case SelectionType.Default:
                        sel = GetDefaultSelectionForSendMessage( gesture );
                        break;

                    case SelectionType.CurrentSelection:
                        sel = gesture.Selection;
                        break;

                    case SelectionType.StartSelection:
                        sel = gesture.StartSelection;
                        break;
                }

                if( sel && sel != EventMessageTarget )
                    sel.SendMessage( EventMessageName, gesture, SendMessageOptions.DontRequireReceiver );
            }
        }
    }
}

public abstract class GestureRecognizer : MonoBehaviour
{
    protected static readonly FingerGestures.IFingerList EmptyFingerList = new FingerGestures.FingerList();

    public enum SelectionType
    {
        Default = 0,
        StartSelection,
        CurrentSelection,
        None,
    }

    /// <summary>
    /// Number of fingers required to perform this gesture
    /// </summary>
    [SerializeField]
    int requiredFingerCount = 1;

    /// <summary>
    /// Specify the unit to use for the distance properties
    /// </summary>
    public DistanceUnit DistanceUnit = DistanceUnit.Centimeters;

    /// <summary>
    /// Maximum number of simultaneous gestures the recognizer can keep track of
    /// </summary>
    public int MaxSimultaneousGestures = 1;

    /// <summary>
    /// Get or set the reset mode for this gesture recognizer
    /// </summary>
    public GestureResetMode ResetMode = GestureResetMode.Default;

    /// <summary>
    /// ScreenRaycaster to use to detect scene objects this gesture is interacting with
    /// </summary>
    public ScreenRaycaster Raycaster;

    /// <summary>
    /// Get or set the finger cluster manager
    /// </summary>
    public FingerClusterManager ClusterManager;

    /// <summary>
    /// Optional reference to a gesture recognizer delegate
    /// </summary>
    public GestureRecognizerDelegate Delegate = null;

    /// <summary>
    /// Use Unity's SendMessage() to broadcast the gesture event to MessageTarget
    /// </summary>
    public bool UseSendMessage = true;
    public string EventMessageName;          // null -> default to GetDefaultEventMessageName()
    public GameObject EventMessageTarget;    // null -> default to current gameobject
    public SelectionType SendMessageToSelection = SelectionType.Default;

    /// <summary>
    /// When the exclusive flag is set, this gesture recognizer will only detect the gesture when the total number
    ///  of active touches on the device is equal to RequiredFingerCount (FingerGestures.Touches.Count == RequiredFingerCount)
    /// </summary>
    public bool IsExclusive = false;
    
    /// <summary>
    /// Exact number of touches required for the gesture to be recognized
    /// </summary>
    public virtual int RequiredFingerCount
    {
        get { return requiredFingerCount; }
        set { requiredFingerCount = value; }
    }

    /// <summary>
    /// Does this type of recognizer support finger clustering to track simultaneous multi-finger gestures?
    /// </summary>
    public virtual bool SupportFingerClustering
    {
        get { return true; }
    }

    /// <summary>
    /// Get the default reset mode for this gesture recognizer. 
    /// Derived classes can override this to specify a different default value
    /// </summary>
    public virtual GestureResetMode GetDefaultResetMode()
    {
        return GestureResetMode.EndOfTouchSequence;
    }

    /// <summary>
    /// Return the default name of the method to invoke on the message target 
    /// </summary>
    public abstract string GetDefaultEventMessageName();

    /// <summary>
    /// Return type description of the internal gesture class used by the recognizer (used by editor)
    /// </summary>
    public abstract System.Type GetGestureType();

    protected virtual void Awake()
    {
        if( string.IsNullOrEmpty( EventMessageName ) )
            EventMessageName = GetDefaultEventMessageName();

        if( ResetMode == GestureResetMode.Default )
            ResetMode = GetDefaultResetMode();

        if( !EventMessageTarget )
            EventMessageTarget = this.gameObject;

        if( !Raycaster )
            Raycaster = GetComponent<ScreenRaycaster>();
    }

    protected virtual void OnEnable()
    {
        if( FingerGestures.Instance )
            FingerGestures.Register( this );
        else
            Debug.LogError( "Failed to register gesture recognizer " + this + " - FingerGestures instance is not available." );
    }

    protected virtual void OnDisable()
    {
        if( FingerGestures.Instance )
            FingerGestures.Unregister( this );
    }

    protected void Acquire( FingerGestures.Finger finger )
    {
        if( !finger.GestureRecognizers.Contains( this ) )
            finger.GestureRecognizers.Add( this );
    }

    protected bool Release( FingerGestures.Finger finger )
    {
        return finger.GestureRecognizers.Remove( this );
    }

    protected virtual void Start()
    {
        if( !FingerGestures.Instance )
        {
            Debug.LogWarning( "FingerGestures instance not found in current scene. Disabling recognizer: " + this );
            enabled = false;
            return;
        }

        if( !ClusterManager && SupportFingerClustering )
        {
            ClusterManager = GetComponent<FingerClusterManager>();

            if( !ClusterManager )
                ClusterManager = FingerGestures.DefaultClusterManager;
        }
    }

    #region Utils

    /// <summary>
    /// Check if all the touches in the list started recently
    /// </summary>
    /// <param name="touches">The touches to evaluate</param>
    /// <returns>True if the age of each touch in the list is under a set threshold</returns>
    protected bool Young( FingerGestures.IFingerList touches )
    {
        FingerGestures.Finger oldestTouch = touches.GetOldest();

        if( oldestTouch == null )
            return false;

        float elapsedTimeSinceFirstTouch = Time.time - oldestTouch.StarTime;

        return elapsedTimeSinceFirstTouch < 0.25f;
    }

    /// <summary>
    /// Convert a distance specified in the unit currently set by DistanceUnit property, and
    /// returns a distance in pixels
    /// </summary>
    public float ToPixels( float distance )
    {
        return distance.Convert( DistanceUnit, DistanceUnit.Pixels );
    }

    /// <summary>
    /// Convert distance to pixels and returns the square of pixel distance
    ///  This is NOT the same as converting the square of the distance and converting it to pixels
    /// </summary>
    public float ToSqrPixels( float distance )
    {
        float pixelDist = ToPixels( distance );
        return pixelDist * pixelDist;
    }

    #endregion
}
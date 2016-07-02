// FingerGestures v3.1
// The FingerGestures library is copyright (c) of William Ravaine
// Please send feedback or bug reports to fingergestures@fatalfrog.com
// More FingerGestures information at http://fingergestures.fatalfrog.com
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// disable "obsolete" warnings in this file
#pragma warning disable 618

/// <summary>
/// FignerGestures Manager
/// </summary>
[AddComponentMenu( "FingerGestures/Finger Gestures Singleton" )]
public class FingerGestures : MonoBehaviour
{
    /// <summary>
    /// This is a list of all the platforms that will use touch-based input instead of mouse
    /// </summary>
    public static readonly RuntimePlatform[] TouchScreenPlatforms = 
    { 
        RuntimePlatform.IPhonePlayer,
        RuntimePlatform.Android,
#if !UNITY_3_5
        RuntimePlatform.BlackBerryPlayer,
        RuntimePlatform.WP8Player,
#endif
    };

    public enum FingerPhase
    {
        None = 0,
        Begin,
        Moving,
        Stationary,
    }
    
    #region Global Events

    public static event Gesture.EventHandler OnGestureEvent;
    public static event FingerEventDetector<FingerEvent>.FingerEventHandler OnFingerEvent;
    
    public delegate void EventHandler();
    public static event EventHandler OnInputProviderChanged;

    internal static void FireEvent( Gesture gesture )
    {
        if( OnGestureEvent != null )
            OnGestureEvent( gesture );
    }

    internal static void FireEvent( FingerEvent eventData )
    {
        if( OnFingerEvent != null )
            OnFingerEvent( eventData );
    }

    #endregion

    // whether or not this object should persist accross scenes (DontDestroyOnLoad)
    public bool makePersistent = true;

    // automatically install the touchInputProvider when UnityRemote is detected
    public bool detectUnityRemote = true;

    // default input providers prefabs (optional)
    public FGInputProvider mouseInputProviderPrefab;
    public FGInputProvider touchInputProviderPrefab;

    // the default finger cluster manager
    FingerClusterManager fingerClusterManager;

    public static FingerClusterManager DefaultClusterManager
    {
        get { return Instance.fingerClusterManager; }
    }

    /// <summary>
    /// Access to the FingerGestures singleton instance
    /// </summary>
    public static FingerGestures Instance
    {
        get { return FingerGestures.instance; }
    }

    void Init()
    {
        InitInputProvider();

        fingerClusterManager = GetComponent<FingerClusterManager>();
        if( !fingerClusterManager )
            fingerClusterManager = gameObject.AddComponent<FingerClusterManager>();
    }

    #region Input Provider

    // reference to the inputProvider currently in use
    FGInputProvider inputProvider;
        
    public FGInputProvider InputProvider
    {
        get { return inputProvider; }
    }

    public class InputProviderEvent
    {
        public FGInputProvider inputProviderPrefab;
    }

    public static bool IsTouchScreenPlatform( RuntimePlatform platform )
    {
        for( int i = 0; i < TouchScreenPlatforms.Length; ++i )
        {
            if( platform == TouchScreenPlatforms[i] )
                return true;
        }

        return false;
    }

    void InitInputProvider()
    {
        InputProviderEvent e = new InputProviderEvent();

        if( IsTouchScreenPlatform( Application.platform ) )
            e.inputProviderPrefab = touchInputProviderPrefab;
        else
            e.inputProviderPrefab = mouseInputProviderPrefab;

        // let other scripts on the same game object override the e.inputProviderPrefab if they want to install a different one
        gameObject.SendMessage( "OnSelectInputProvider", e, SendMessageOptions.DontRequireReceiver );

        // install it
        InstallInputProvider( e.inputProviderPrefab );
    }

    public void InstallInputProvider( FGInputProvider inputProviderPrefab )
    {
        if( !inputProviderPrefab )
        {
            Debug.LogError( "Invalid InputProvider (null)" );
            return;
        }

        Debug.Log( "FingerGestures: using " + inputProviderPrefab.name );

        // remove any existing one
        if( inputProvider )
            Destroy( inputProvider.gameObject );
        
        inputProvider = Instantiate( inputProviderPrefab ) as FGInputProvider;
        inputProvider.name = inputProviderPrefab.name;
        inputProvider.transform.parent = this.transform;

        // Create fingers & gesture recognizers
        InitFingers( MaxFingers );

        if( OnInputProviderChanged != null )
            OnInputProviderChanged();
    }
    
    #endregion

    #region Finger

    /// <summary>
    /// Finger
    /// 
    /// This provides an abstraction for a finger that can touch and move around the screen.
    /// As opposed to Unity's Touch object, a Finger exists independently of whether it is 
    /// currently touching the screen or not
    /// </summary>
    public class Finger
    {
        #region Properties

        /// <summary>
        /// Unique identifier for this finger. 
        /// For touch screen gestures, this corresponds to Touch.index, and the button index for mouse gestures.
        /// </summary>
        public int Index
        {
            get { return index; }
        }
        
        /// <summary>
        /// Return true if the finger is currently down
        /// </summary>
        public bool IsDown
        {
            get { return phase != FingerPhase.None; }
        }

        /// <summary>
        /// Current phase
        /// </summary>
        public FingerPhase Phase
        {
            get { return phase; }
        }

        /// <summary>
        /// Phase during last frame
        /// </summary>
        public FingerPhase PreviousPhase
        {
            get { return prevPhase; }
        }

        /// <summary>
        /// Return true if the finger was down during the previous update/frame
        /// </summary>
        public bool WasDown
        {
            get { return prevPhase != FingerPhase.None; }
        }

        /// <summary>
        /// Return true if the finger is currently down & moving
        /// </summary>
        public bool IsMoving
        {
            get { return phase == FingerPhase.Moving; }
        }
        
        /// <summary>
        /// Return true if the finger was down & moving during the previous update/frame
        /// </summary>
        public bool WasMoving 
        {
            get { return prevPhase == FingerPhase.Moving; }
        }

        /// <summary>
        /// Is the finger currently stationary?
        /// </summary>
        public bool IsStationary
        {
            get { return phase == FingerPhase.Stationary; }
        }

        /// <summary>
        /// Was the finger stationary during previous frame?
        /// </summary>
        public bool WasStationary
        {
            get { return prevPhase == FingerPhase.Stationary; }
        }

        /// <summary>
        /// Return true if the finger is down and has moved at least once since its first touched the screen
        /// </summary>
        public bool Moved
        {
            get { return moved; }
        }

        /// <summary>
        /// Get the time of first screen contact
        /// </summary>
        public float StarTime
        {
            get { return startTime; }
        }

        /// <summary>
        /// Get the position of first screen contact
        /// </summary>
        public Vector2 StartPosition
        {
            get { return startPos; }
        }

        /// <summary>
        /// Get the current position
        /// </summary>
        public Vector2 Position
        {
            get { return pos; }
        }

        /// <summary>
        /// Get the position during the previous frame
        /// </summary>
        public Vector2 PreviousPosition
        {
            get { return prevPos; }
        }

        /// <summary>
        /// Get the difference between previous and current position
        /// </summary>
        public Vector2 DeltaPosition
        {
            get { return deltaPos; }
        }

        /// <summary>
        /// Get the distance traveled from initial position
        /// </summary>
        public float DistanceFromStart
        {
            get { return distFromStart; }
        }

        /// <summary>
        /// Return true if this finger is currently being filtered out via the global touch filter
        /// </summary>
        public bool IsFiltered
        {
            get { return filteredOut; }
        }

        /// <summary>
        /// Amount of time this finger has stayed stationary thus far
        /// </summary>
        public float TimeStationary
        {
            get { return elapsedTimeStationary; }
        }

        /// <summary>
        /// List of active gesture recognizers currently involving this finger
        /// </summary>
        public List<GestureRecognizer> GestureRecognizers
        {
            get { return gestureRecognizers; }
        }

        /// <summary>
        /// A key-value map of user-provided properties
        /// </summary>
        public Dictionary<string, object> ExtendedProperties
        {
            get { return extendedProperties; }
        }

        #endregion

        #region Internal

        int index = 0;
        FingerPhase phase = FingerPhase.None;
        FingerPhase prevPhase = FingerPhase.None;
        Vector2 pos = Vector2.zero;
        Vector2 startPos = Vector2.zero;
        Vector2 prevPos = Vector2.zero;
        Vector2 deltaPos = Vector2.zero;
        float startTime = 0;
        float lastMoveTime = 0;
        float distFromStart = 0;
        bool moved = false;
        bool filteredOut = true; // is this finger being filtered out?
        Collider collider;
        Collider prevCollider;
        float elapsedTimeStationary = 0;
        List<GestureRecognizer> gestureRecognizers = new List<GestureRecognizer>();
        Dictionary<string, object> extendedProperties = new Dictionary<string,object>();
        
        public Finger( int index )
        {
            this.index = index;
        }

        public override string ToString()
        {
            return "Finger" + index;
        }

        public static implicit operator bool( Finger finger )
        {
            return finger != null;
        }

        internal void Update( bool newDownState, Vector2 newPos )
        {
            // stop ignoring this touch when the finger is no longer touching the screen
            if( filteredOut && !newDownState )
                filteredOut = false;
            
            // Apply global touch filter on new touches
            if( !IsDown && newDownState && !FingerGestures.instance.ShouldProcessTouch( index, newPos ) )
            {
                filteredOut = true;
                newDownState = false;
            }

            // process touch
            prevPhase = phase;
            
            if( newDownState )
            {
                // new touch? reset finger state
                if( !WasDown )
                {
                    phase = FingerPhase.Begin;
                    
                    pos = newPos;
                    startPos = pos;
                    prevPos = pos;
                    deltaPos = Vector2.zero;
                    moved = false;
                    lastMoveTime = 0;

                    startTime = Time.time;
                    elapsedTimeStationary = 0;
                    distFromStart = 0;
                }
                else
                {
                    prevPos = pos;
                    pos = newPos;
                    distFromStart = Vector3.Distance( startPos, pos );
                    deltaPos = pos - prevPos;

                    if( deltaPos.sqrMagnitude > 0 )
                    {
                        lastMoveTime = Time.time;
                        phase = FingerPhase.Moving;
                    }
                    else if( !IsMoving || ( ( Time.time - lastMoveTime ) > 0.05f ) )    // allow for a small time buffer after we stopped moving
                    {
                        phase = FingerPhase.Stationary;
                    }

                    if( IsMoving )
                    {
                        // finger moved at least once
                        moved = true;
                    }
                    else
                    {
                        if( !WasStationary ) // begin a new stationary phase
                            elapsedTimeStationary = 0;
                        else
                            elapsedTimeStationary += Time.deltaTime;
                    }
                }
            }
            else
            {
                phase = FingerPhase.None;
            }
        }

        #endregion
    }
    
    /// <summary>
    /// Maximum number of simultaneous fingers supported
    /// </summary>
    public int MaxFingers
    {
        get { return inputProvider.MaxSimultaneousFingers; }
    }

    /// <summary>
    /// Get a finger by its index
    /// </summary>
    public static Finger GetFinger( int index )
    {
        return instance.fingers[index];
    }
    
    /// <summary>
    /// List of fingers currently touching the screen
    /// </summary>
    public static IFingerList Touches
    {
        get { return instance.touches; }
    }

    #endregion

    static List<GestureRecognizer> gestureRecognizers = new List<GestureRecognizer>();

    public static List<GestureRecognizer> RegisteredGestureRecognizers
    {
        get { return gestureRecognizers; }
    }

    public static void Register( GestureRecognizer recognizer )
    {
        if( gestureRecognizers.Contains( recognizer ) )
            return;

        gestureRecognizers.Add( recognizer );
        //Debug.Log( "Registered gesture recognizer: " + recognizer );
    }

    public static void Unregister( GestureRecognizer recognizer )
    {
        gestureRecognizers.Remove( recognizer );
    }

    #region Engine Callbacks

    void Awake()
    {
        CheckInit();
    }

    void Start()
    {
        if( makePersistent )
            DontDestroyOnLoad( this.gameObject );
    }
    
    // this is called after Awake() OR after the script is recompiled (Recompile > Disable > Enable)
    void OnEnable()
    {
        CheckInit();
    }

    void CheckInit()
    {
        if( instance == null )
        {
            instance = this;
            Init();
        }
        else if( instance != this )
        {
            Debug.LogWarning( "There is already an instance of FingerGestures created (" + instance.name + "). Destroying new one." );
            Destroy( this.gameObject );
            return;
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if( detectUnityRemote && Input.touchCount > 0 && inputProvider.GetType() != touchInputProviderPrefab.GetType() )
        {
            Debug.Log( "UnityRemote presence detected. Switching to touch input." );
            InstallInputProvider( touchInputProviderPrefab );
            detectUnityRemote = false;
            
            return; // skip a frame
        }
#endif
        if( inputProvider )
        {
            UpdateFingers();
        }
    }

    #endregion

    #region Internal

    // access to the singleton
    static FingerGestures instance;

    #region Fingers Management

    Finger[] fingers;
    FingerList touches;
    
    void InitFingers( int count )
    {
        // pre-allocate a touch data entry for each finger
        fingers = new Finger[count];

        for( int i = 0; i < count; ++i )
            fingers[i] = new Finger( i );

        touches = new FingerList();

        //InitNodes();
    }

    void UpdateFingers()
    {
        touches.Clear();

        // update all fingers
        for( int i = 0; i < fingers.Length; ++i )
        {
            Finger finger = fingers[i];

            Vector2 pos = Vector2.zero;
            bool down = false;

            // request fresh input state from provider
            inputProvider.GetInputState( finger.Index, out down, out pos );

            finger.Update( down, pos );

            if( finger.IsDown )
                touches.Add( finger );
        }
    }

    #endregion

    #region Global Input Filter

    /// <summary>
    /// Return tru
    /// </summary>
    /// <param name="fingerIndex">The index of the finger that just touched the screen</param>
    /// <param name="position">The new finger position if the input is let through</param>
    /// <returns>True to let the touch go through, or false to block it</returns>
    public delegate bool GlobalTouchFilterDelegate( int fingerIndex, Vector2 position );

    GlobalTouchFilterDelegate globalTouchFilterFunc;

    /// <summary>
    /// Can specify a method to selectively prevent new touches from being processed until they are released.
    /// This can be useful to globally deny gesture events from being fired when above a region of the screen,
    /// or when the input has been consumed by another input system
    /// </summary>
    public static GlobalTouchFilterDelegate GlobalTouchFilter
    {
        get { return instance.globalTouchFilterFunc; }
        set { instance.globalTouchFilterFunc = value; }
    }

    protected bool ShouldProcessTouch( int fingerIndex, Vector2 position )
    {
        if( globalTouchFilterFunc != null )
            return globalTouchFilterFunc( fingerIndex, position );

        return true;
    }

    #endregion

    #region Standard Global & Per-Finger Gesture Recognizers

    Transform[] fingerNodes;

    Transform CreateNode( string name, Transform parent )
    {
        GameObject go = new GameObject( name );
        go.transform.parent = parent;
        return go.transform;
    }

    void InitNodes()
    {
        int fingerCount = fingers.Length;

        if( fingerNodes != null )
        {
            foreach( Transform fingerCompNode in fingerNodes )
                Destroy( fingerCompNode.gameObject );
        }

        fingerNodes = new Transform[fingerCount];
        for( int i = 0; i < fingerNodes.Length; ++i )
            fingerNodes[i] = CreateNode( "Finger" + i, this.transform );
    }
    
    #endregion

    #endregion

    #region Finger List Data Structure

    /// <summary>
    /// Represent a read-only list of fingers, augmented with a bunch of utility methods
    /// </summary>
    public interface IFingerList : IEnumerable<Finger>
    {
        /// <summary>
        /// Get finger in array by index
        /// </summary>
        /// <param name="index">The array index</param>
        Finger this[int index]
        {
            get;
        }

        /// <summary>
        /// Number of fingers in the list
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get the average initial contact-point position of all fingers in the list
        /// </summary>
        /// <returns></returns>
        Vector2 GetAverageStartPosition();

        /// <summary>
        /// Get the average position of all the fingers in the list
        /// </summary>
        Vector2 GetAveragePosition();

        /// <summary>
        /// Get the average previous position of all the fingers in the list
        /// </summary>
        Vector2 GetAveragePreviousPosition();

        /// <summary>
        /// Get the average distance from each finger's starting position in the list
        /// </summary>
        float GetAverageDistanceFromStart();

        /// <summary>
        /// Find the finger with the oldest StartTime
        /// </summary>
        Finger GetOldest();

        /// <summary>
        /// Return true if all the touches are currently moving
        /// </summary>
        bool AllMoving();

        /// <summary>
        /// Return true if the touches are moving in the same direction
        /// </summary>
        /// <param name="tolerance">0->1 range that maps to 0->90 degrees from reference</param>
        /// <returns></returns>
        bool MovingInSameDirection( float tolerance );
    }

    /// <summary>
    /// A finger list implementation with support for write access
    /// </summary>
    [System.Serializable]
    public class FingerList : IFingerList
    {
        [SerializeField]
        List<Finger> list;

        public FingerList()
        {
            list = new List<Finger>();
        }

        public FingerList( List<Finger> list )
        {
            this.list = list;
        }

        public Finger this[int index]
        {
            get { return list[index]; }
        }

        public int Count
        {
            get { return list.Count; }
        }

        public IEnumerator<Finger> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add( Finger touch )
        {
            list.Add( touch );
        }

        public bool Remove( Finger touch )
        {
            return list.Remove( touch );
        }

        public bool Contains( Finger touch )
        {
            return list.Contains( touch );
        }

        public void AddRange( IEnumerable<Finger> touches )
        {
            list.AddRange( touches );
        }

        public void Clear()
        {
            list.Clear();
        }

        public delegate T FingerPropertyGetterDelegate<T>( Finger finger );
        
        public Vector2 AverageVector( FingerPropertyGetterDelegate<Vector2> getProperty )
        {
            Vector2 avg = Vector2.zero;

            if( Count > 0 )
            {
                for( int i = 0; i < list.Count; ++i )
                    avg += getProperty( list[i] );

                avg /= Count;
            }

            return avg;
        }

        public float AverageFloat( FingerPropertyGetterDelegate<float> getProperty )
        {
            float avg = 0;

            if( Count > 0 )
            {
                for( int i = 0; i < list.Count; ++i )
                    avg += getProperty( list[i] );

                avg /= Count;
            }

            return avg;
        }

        // cache delegates so we do not allocate them at runtime on each call
        static FingerPropertyGetterDelegate<Vector2> delGetFingerStartPosition = GetFingerStartPosition;
        static FingerPropertyGetterDelegate<Vector2> delGetFingerPosition = GetFingerPosition;
        static FingerPropertyGetterDelegate<Vector2> delGetFingerPreviousPosition = GetFingerPreviousPosition;
        static FingerPropertyGetterDelegate<float> delGetFingerDistanceFromStart = GetFingerDistanceFromStart;

        static Vector2 GetFingerStartPosition( Finger finger ) { return finger.StartPosition; }
        static Vector2 GetFingerPosition( Finger finger ) { return finger.Position; }
        static Vector2 GetFingerPreviousPosition( Finger finger ) { return finger.PreviousPosition; }
        static float GetFingerDistanceFromStart( Finger finger ) { return finger.DistanceFromStart; }

        public Vector2 GetAverageStartPosition()
        {
            return AverageVector( delGetFingerStartPosition );
        }

        public Vector2 GetAveragePosition()
        {
            return AverageVector( delGetFingerPosition );
        }

        public Vector2 GetAveragePreviousPosition()
        {
            return AverageVector( delGetFingerPreviousPosition );
        }

        public float GetAverageDistanceFromStart()
        {
            return AverageFloat( delGetFingerDistanceFromStart );
        }

        public Finger GetOldest()
        {
            Finger oldest = null;

            foreach( Finger finger in list )
            {
                if( oldest == null || ( finger.StarTime < oldest.StarTime ) )
                    oldest = finger;
            }

            return oldest;
        }

        public bool MovingInSameDirection( float tolerance )
        {
            if( Count < 2 )
                return true;

            float minDOT = Mathf.Max( 0.1f, 1.0f - tolerance ); 
            
            Vector2 refDir = this[0].Position - this[0].StartPosition;
            refDir.Normalize();
            
            for( int i = 1; i < Count; ++i )
            {
                Vector2 dir = this[i].Position - this[i].StartPosition;
                dir.Normalize();

                if( Vector2.Dot( refDir, dir ) < minDOT )
                    return false;
            }

            return true;
        }

        public bool AllMoving()
        {
            if( Count == 0 )
                return false;

            // all touches must be moving
            for( int i = 0; i < list.Count; ++i )
            {
                if( !list[i].IsMoving )
                    return false;
            }

            return true;
        }
    }

    #endregion

    #region Swipe Direction

    /// <summary>
    /// Supported swipe gesture directions
    /// </summary>
    [System.Flags]
    public enum SwipeDirection
    {
        /// <summary>
        /// Moved to the right
        /// </summary>
        Right = 1 << 0,

        /// <summary>
        /// Moved to the left
        /// </summary>
        Left = 1 << 1,

        /// <summary>
        /// Moved up
        /// </summary>
        Up = 1 << 2,

        /// <summary>
        /// Moved down
        /// </summary>
        Down = 1 << 3,

        /// <summary>
        /// North-East diagonal
        /// </summary>
        UpperLeftDiagonal = 1 << 4,

        /// <summary>
        /// North-West diagonal
        /// </summary>
        UpperRightDiagonal = 1 << 5,

        /// <summary>
        /// South-East diagonal
        /// </summary>
        LowerRightDiagonal = 1 << 6,

        /// <summary>
        /// South-West diagonal
        /// </summary>
        LowerLeftDiagonal = 1 << 7,

        //--------------------

        None = 0,
        Vertical = Up | Down,
        Horizontal = Right | Left,
        Cross = Vertical | Horizontal,
        UpperDiagonals = UpperLeftDiagonal | UpperRightDiagonal,
        LowerDiagonals = LowerLeftDiagonal | LowerRightDiagonal,
        Diagonals = UpperDiagonals | LowerDiagonals,
        All = Cross | Diagonals,
    }

    static readonly SwipeDirection[] AngleToDirectionMap = new SwipeDirection[]
    {
        SwipeDirection.Right,               // 0
        SwipeDirection.UpperRightDiagonal,  // 45
        SwipeDirection.Up,                  // 90
        SwipeDirection.UpperLeftDiagonal,   // 135
        SwipeDirection.Left,                // 180
        SwipeDirection.LowerLeftDiagonal,   // 225
        SwipeDirection.Down,                // 270
        SwipeDirection.LowerRightDiagonal,  // 315
    };

    /// <summary>
    /// Extract a swipe direction from a direction vector and a tolerance percent 
    /// </summary>
    /// <param name="dir">A normalized swipe motion vector (must be normalized!)</param>
    /// <param name="tolerance">Percentage of tolerance (0..1)</param>
    /// <returns>The constrained swipe direction identifier</returns>
    public static SwipeDirection GetSwipeDirection( Vector2 dir, float tolerance )
    {
        // max +/- 22.5 degrees around reference angle
        float maxAngleDelta = Mathf.Max( Mathf.Clamp01( tolerance ) * 22.5f, 0.0001f );

        // get the angle formed by the dir vector (0 = right, 90 = up, 180 = left, 270 = down)
        float angle = NormalizeAngle360( Mathf.Rad2Deg * Mathf.Atan2( dir.y, dir.x ) );

        if( angle >= 360 - 22.5f )
            angle -= 360;   // wrap around in negative direction

        for( int i = 0; i < 8; ++i )
        {
            float refAngle = 45.0f * i;

            if( angle <= refAngle + 22.5f )
            {
                float minAngle = refAngle - maxAngleDelta;
                float maxAngle = refAngle + maxAngleDelta;

                if( angle >= minAngle && angle <= maxAngle )
                    return AngleToDirectionMap[i];

                break;
            }
        }

        // not a valid direction / not within tolerance zone
        return SwipeDirection.None;
    }

    /// <summary>
    /// Extract a swipe direction from an input direction vector, with 100% tolerance (this is guaranteed to return a valid direction)
    /// </summary>
    public static SwipeDirection GetSwipeDirection( Vector2 dir )
    {
        return GetSwipeDirection( dir, 1.0f );
    }

    #endregion
    
    #region Utils

    public static bool UsingUnityRemote()
    {
        //return ( Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor );
        return false;
    }

    /// <summary>
    /// Check if all the fingers are moving
    /// </summary>
    public static bool AllFingersMoving( Finger finger0, Finger finger1 )
    {
        return finger0.IsMoving && finger1.IsMoving;
    }

    /// <summary>
    /// Check if the input fingers are moving in opposite direction
    /// </summary>
    public static bool FingersMovedInOppositeDirections( Finger finger0, Finger finger1, float minDOT )
    {
        float dot = Vector2.Dot( finger0.DeltaPosition.normalized, finger1.DeltaPosition.normalized );
        return dot < minDOT;
    }

    /// <summary>
    /// returns signed angle in radians between "from" -> "to"
    /// </summary>
    public static float SignedAngle( Vector2 from, Vector2 to )
    {
        // perpendicular dot product
        float perpDot = ( from.x * to.y ) - ( from.y * to.x );
        return Mathf.Atan2( perpDot, Vector2.Dot( from, to ) );
    }

    // normalize angle to (0, 360) range
    public static float NormalizeAngle360( float angleInDegrees )
    {
        angleInDegrees = angleInDegrees % 360.0f;

        if( angleInDegrees < 0 )
            angleInDegrees += 360;

        return angleInDegrees;
    }

    #endregion

    #region DPI & Distance Units

    /*
iPhone3        320x480  163 ppi
iPhone4        640著60  326 ppi 
iPhone4S       640著60  326 ppi 
iPhone5        640逐136 326 ppi 
iPhone5S       640x1136 326 ppi
iPad          1024x768  132 ppi
iPad2         1024x768  132 ppi
iPad (3gen)   2048x1536 264 ppi
iPad (4gen)   2048x1536 264 ppi
iPad mini     1024x768  163 ppi
iPad mini2    2048x1536 326 ppi
         */

    const float DESKTOP_SCREEN_STANDARD_DPI = 96; // default win7 dpi
    const float INCHES_TO_CENTIMETERS = 2.54f; // 1 inch = 2.54 cm
    const float CENTIMETERS_TO_INCHES = 1.0f / INCHES_TO_CENTIMETERS; // 1 cm = 0.3937... inches

    static float screenDPI = 0;

    /// <summary>
    /// Screen Dots-Per-Inch
    /// </summary>
    public static float ScreenDPI
    {
        get 
        {
            // not intialized?
            if( screenDPI <= 0 )
            {
                screenDPI = Screen.dpi;

                // on desktop, dpi can be 0 - default to a standard dpi for screens
                if( screenDPI <= 0 )
                    screenDPI = DESKTOP_SCREEN_STANDARD_DPI;

#if UNITY_IPHONE
                // try to detect some devices that aren't supported by Unity (yet)
                if( iPhone.generation == iPhoneGeneration.Unknown ||
                    iPhone.generation == iPhoneGeneration.iPadUnknown ||
                    iPhone.generation == iPhoneGeneration.iPhoneUnknown )
                {
                    // ipad mini 2 ?
                    if( Screen.width == 2048 && Screen.height == 1536 && screenDPI == 260 )
                        screenDPI = 326;
                }
#endif
            }

            return FingerGestures.screenDPI; 
        }

        set { FingerGestures.screenDPI = value; }
    }

    
    public static float Convert( float distance, DistanceUnit fromUnit, DistanceUnit toUnit )
    {
        float dpi = ScreenDPI;
        float pixelDistance; 

        switch( fromUnit )
        {
            case DistanceUnit.Centimeters:
                pixelDistance = distance * CENTIMETERS_TO_INCHES * dpi; // cm -> in -> px
                break;

            case DistanceUnit.Inches:
                pixelDistance = distance * dpi; // in -> px
                break;

            case DistanceUnit.Pixels:
            default:
                pixelDistance = distance;
                break;
        }

        switch( toUnit )
        {
            case DistanceUnit.Inches:
                return pixelDistance / dpi; // px -> in

            case DistanceUnit.Centimeters:
                return ( pixelDistance / dpi ) * INCHES_TO_CENTIMETERS;  // px -> in -> cm
            
            case DistanceUnit.Pixels:
                return pixelDistance;
        }

        return pixelDistance;
    }

    // convert a 2D motion vector from one distance unit to another one
    public static Vector2 Convert( Vector2 v, DistanceUnit fromUnit, DistanceUnit toUnit )
    {
        return new Vector2( Convert( v.x, fromUnit, toUnit ),
                            Convert( v.y, fromUnit, toUnit ) );
    }

    #endregion
}

public enum DistanceUnit
{
    Pixels,
    Inches,
    Centimeters,
}

/// <summary>
/// Utility extension methods
/// </summary>
public static class FingerGesturesExtensions
{
    /// <summary>
    /// Return short version of unit name (cm, in, px)
    /// </summary>
    public static string Abreviation( this DistanceUnit unit )
    {
        switch( unit )
        {
            case DistanceUnit.Centimeters:
                return "cm";

            case DistanceUnit.Inches:
                return "in";

            case DistanceUnit.Pixels:
                return "px";
        }

        return unit.ToString();
    }

    /// <summary>
    /// Convert the current value from 'fromUnit' to 'toUnit'
    /// </summary>
    public static float Convert( this float value, DistanceUnit fromUnit, DistanceUnit toUnit )
    {
        return FingerGestures.Convert( value, fromUnit, toUnit );
    }

    /// <summary>
    /// Convert the current pixel-distance based value to the desired unit
    /// </summary>
    public static float In( this float valueInPixels, DistanceUnit toUnit )
    {
        return valueInPixels.Convert( DistanceUnit.Pixels, toUnit );
    }

    /// <summary>
    /// Convert the current pixel-distance based value to centimeters
    /// </summary>
    public static float Centimeters( this float valueInPixels )
    {
        return valueInPixels.In( DistanceUnit.Centimeters );
    }

    /// <summary>
    /// Convert the current pixel-distance based value to inches
    /// </summary>
    public static float Inches( this float valueInPixels )
    {
        return valueInPixels.In( DistanceUnit.Inches );
    }

    /// <summary>
    /// Convert the current Vector2 from 'fromUnit' to 'toUnit'
    /// </summary>
    public static Vector2 Convert( this Vector2 v, DistanceUnit fromUnit, DistanceUnit toUnit )
    {
        return FingerGestures.Convert( v, fromUnit, toUnit );
    }

    /// <summary>
    /// Convert the current pixel-based Vector2 to the desired unit
    /// </summary>
    public static Vector2 In( this Vector2 vecInPixels, DistanceUnit toUnit )
    {
        return vecInPixels.Convert( DistanceUnit.Pixels, toUnit );
    }

    /// <summary>
    /// Convert the current pixel-based Vector2 to centimeters
    /// </summary>
    public static Vector2 Centimeters( this Vector2 vecInPixels )
    {
        return vecInPixels.In( DistanceUnit.Centimeters );
    }

    /// <summary>
    /// Convert the current pixel-based Vector2 to inches
    /// </summary>
    public static Vector2 Inches( this Vector2 vecInPixels )
    {
        return vecInPixels.In( DistanceUnit.Inches );
    }
}

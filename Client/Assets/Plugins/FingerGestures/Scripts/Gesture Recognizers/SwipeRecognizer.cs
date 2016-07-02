using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwipeGesture : DiscreteGesture
{
    Vector2 move = Vector2.zero;
    float velocity = 0;
    FingerGestures.SwipeDirection direction = FingerGestures.SwipeDirection.None;

    internal int MoveCounter = 0;
    internal float Deviation = 0; // current total angular deviation on swipe direction
    
    /// <summary>
    /// Total swipe vector
    /// </summary>
    public Vector2 Move
    {
        get { return move; }
        internal set { move = value; }
    }

    /// <summary>
    /// Instant gesture velocity in pixels per second
    /// </summary>
    public float Velocity
    {
        get { return velocity; }
        internal set { velocity = value; }
    }

    /// <summary>
    /// Approximate direction of the swipe gesture
    /// </summary>
    public FingerGestures.SwipeDirection Direction
    {
        get { return direction; }
        internal set { direction = value; }
    }
}

/// <summary>
/// Swipe Gesture Recognizer
///   A quick drag motion and release in a single direction
/// </summary>
[AddComponentMenu( "FingerGestures/Gestures/Swipe Recognizer" )]
public class SwipeRecognizer : DiscreteGestureRecognizer<SwipeGesture>
{
    /// <summary>
    /// Directions to restrict the swipe gesture to
    /// </summary>
    private FingerGestures.SwipeDirection ValidDirections = FingerGestures.SwipeDirection.All;  //FIXME: public

    /// <summary>
    /// Minimum distance the finger must travel in order to produce a valid swipe
    /// <seealso cref="DistanceUnit"/>
    /// </summary>
    public float MinDistance = 0.5f;

    /// <summary>
    /// Finger travel distance beyond which the swipe recognition will fail.
    /// Setting this to 0 disables the MaxDistance constraint
    /// <seealso cref="DistanceUnit"/>
    /// </summary>
    public float MaxDistance = 0;

    /// <summary>
    /// Minimum speed the finger motion must maintain in order to generate a valid swipe
    /// <seealso cref="DistanceUnit"/>
    /// </summary>
    public float MinVelocity = 5.0f;    // 5 cm/s

    /// <summary>
    /// Amount of tolerance used when determining whether or not the current swipe motion is still moving in a valid direction
    /// The maximum angle, in degrees, that the current swipe direction is allowed to deviate from its initial direction
    /// </summary>
    public float MaxDeviation = 25.0f; // degrees
        
    public override string GetDefaultEventMessageName()
    {
        return "OnSwipe";
    }

    protected override bool CanBegin( SwipeGesture gesture, FingerGestures.IFingerList touches )
    {
        if( !base.CanBegin( gesture, touches ) )
            return false;

        if( touches.GetAverageDistanceFromStart() < 0.5f )
            return false;

        // all touches must be moving
        if( !touches.AllMoving() )
            return false;

        // if multiple touches, make sure they're all going in roughly the same direction
        if( !touches.MovingInSameDirection( 0.35f ) )
            return false;

        return true;
    }    

    protected override void OnBegin( SwipeGesture gesture, FingerGestures.IFingerList touches )
    {
        gesture.StartPosition = touches.GetAverageStartPosition();
        gesture.Position = touches.GetAveragePosition();
        gesture.Move = Vector3.zero;
        gesture.MoveCounter = 0;
        gesture.Deviation = 0;
        gesture.Direction = FingerGestures.SwipeDirection.None;

        //Debug.Log( "BeginSwipe: " + EventMessageName + " touches.Count=" + FingerGestures.Touches.Count );
    }

    protected override GestureRecognitionState OnRecognize( SwipeGesture gesture, FingerGestures.IFingerList touches )
    {
        float minDistanceInPixels = ToPixels( MinDistance );
        float maxDistanceInPixels = ToPixels( MaxDistance );

        if( touches.Count != RequiredFingerCount )
        {
            // more fingers were added - fail right away
            if( touches.Count > RequiredFingerCount )
                return GestureRecognitionState.Failed;

            //
            // fingers were lifted-off
            //

            // didn't swipe far enough
            if( gesture.Move.magnitude < Mathf.Max( 1, minDistanceInPixels ) )
                return GestureRecognitionState.Failed;

            // get approx swipe direction
            gesture.Direction = FingerGestures.GetSwipeDirection( gesture.Move );
            return GestureRecognitionState.Recognized;
        }

        Vector2 previousMotion = gesture.Move;
        gesture.Position = touches.GetAveragePosition();
        gesture.Move = gesture.Position - gesture.StartPosition;

        // pixel-adjusted distance
        float distance = gesture.Move.magnitude;

        // moved too far
        if( maxDistanceInPixels > minDistanceInPixels && distance > maxDistanceInPixels )
        {
            //Debug.LogWarning( "Too far: " + distance );
            return GestureRecognitionState.Failed;
        }

        if( gesture.ElapsedTime > 0 )
            gesture.Velocity = distance / gesture.ElapsedTime;
        else
            gesture.Velocity = 0;
        
        // we're going too slow
        if( gesture.MoveCounter > 2 && gesture.Velocity < ToPixels( MinVelocity ) )
        {
            //Debug.LogWarning( "Too slow: " + gesture.Velocity );
            return GestureRecognitionState.Failed;
        }
        
        // check if we have deviated too much from our initial direction
        if( distance > 50.0f && gesture.MoveCounter > 2 )
        {
            // accumulate delta angle
            gesture.Deviation += Mathf.Rad2Deg * FingerGestures.SignedAngle( previousMotion, gesture.Move );

            if( Mathf.Abs( gesture.Deviation ) > MaxDeviation )
            {
                //Debug.LogWarning( "Swipe: deviated too much from initial direction (" + gesture.Deviation + ")" );
                return GestureRecognitionState.Failed;
            }
        }

        ++gesture.MoveCounter;
        return GestureRecognitionState.InProgress;
    }

    /// <summary>
    /// Return true if the input direction is supported
    /// </summary>
    public bool IsValidDirection( FingerGestures.SwipeDirection dir )
    {
        if( dir == FingerGestures.SwipeDirection.None )
            return false;

        return ( ( ValidDirections & dir ) == dir );
    }
}

using UnityEngine;
using System.Collections;

[System.Serializable]
public class TapGesture : DiscreteGesture
{
    int taps = 0;

    /// <summary>
    /// Number of taps performed
    /// </summary>
    public int Taps
    {
        get { return taps; }
        internal set { taps = value; }
    }

    internal bool Down = false;
    internal bool WasDown = false;
    internal float LastDownTime = 0;
    internal float LastTapTime = 0;
}

/// <summary>
/// Tap Gesture Recognizer
///   A press and release sequence at the same location
/// </summary>
[AddComponentMenu( "FingerGestures/Gestures/Tap Recognizer" )]
public class TapRecognizer : DiscreteGestureRecognizer<TapGesture>
{
    /// <summary>
    /// Exact number of taps required to succesfully recognize the tap gesture. Must be greater or equal to 1.
    /// </summary>
    /// <seealso cref="Taps"/>
    public int RequiredTaps = 1;

    /// <summary>
    /// How far the finger can move from its initial position without making the gesture fail
    /// <seealso cref="DistanceUnit"/>
    /// </summary>
    public float MoveTolerance = 0.5f;

    /// <summary>
    /// Maximum amount of time the fingers can be held down without failing the gesture. Set to 0 for infinite duration.
    /// </summary>
    public float MaxDuration = 0;

    /// <summary>
    /// The maximum amount of the time that can elapse between two consecutive taps without causing the recognizer to reset.
    /// Set to 0 to ignore this setting.
    /// </summary>
    public float MaxDelayBetweenTaps = 0.5f;

    bool IsMultiTap
    {
        get { return RequiredTaps > 1; }
    }

    bool HasTimedOut( TapGesture gesture )
    {
        // check elapsed time since beginning of gesture
        if( MaxDuration > 0 && ( gesture.ElapsedTime > MaxDuration ) )
            return true;

        // check elapsed time since last tap
        if( IsMultiTap && MaxDelayBetweenTaps > 0 && ( Time.time - gesture.LastTapTime > MaxDelayBetweenTaps ) )
            return true;

        return false;
    }

    protected override void Reset( TapGesture gesture )
    {
        //Debug.Log( "Resetting XTapRecognizer" );
        gesture.Taps = 0;
        gesture.Down = false;
        gesture.WasDown = false;
        base.Reset( gesture );
    }

    public override bool SupportFingerClustering
    {
        get
        {
            // don't support multi-finger multi-tap
            if( IsMultiTap )
                return false;

            return base.SupportFingerClustering;
        }
    }

    GestureRecognitionState RecognizeSingleTap( TapGesture gesture, FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
        {
            // all fingers lifted - fire the tap event
            if( touches.Count == 0 )
                return GestureRecognitionState.Recognized;

            // either lifted off some fingers or added some new ones
            return GestureRecognitionState.Failed;
        }

        if( HasTimedOut( gesture ) )
            return GestureRecognitionState.Failed;

        // check if finger moved too far from start position
        float sqrDist = Vector3.SqrMagnitude( touches.GetAveragePosition() - gesture.StartPosition );
        
        if( sqrDist >= ToSqrPixels( MoveTolerance ) )
            return GestureRecognitionState.Failed;

        return GestureRecognitionState.InProgress;
    }

    GestureRecognitionState RecognizeMultiTap( TapGesture gesture, FingerGestures.IFingerList touches )
    {
        gesture.WasDown = gesture.Down;
        gesture.Down = false;

        if( touches.Count == RequiredFingerCount )
        {
            gesture.Down = true;
            gesture.LastDownTime = Time.time;
        }
        else if( touches.Count == 0 )
        {
            gesture.Down = false;
        }
        else
        {
            // some fingers were lifted off
            if( touches.Count < RequiredFingerCount )
            {
                // give a bit of buffer time to lift-off the remaining fingers
                if( Time.time - gesture.LastDownTime > 0.25f )
                {
                    //Debug.LogWarning( "MultiTap - some fingers were lifted off" );
                    return GestureRecognitionState.Failed;
                }
            }
            else // fingers were added
            {
                if( !Young( touches ) )
                {
                    //Debug.LogWarning( "MultiTap - some fingers were added" );
                    return GestureRecognitionState.Failed;
                }
            }
        }

        if( HasTimedOut( gesture ) )
        {
            //Debug.LogWarning( "MultiTap timed out" );
            return GestureRecognitionState.Failed;
        }

        if( gesture.Down )
        {
            // check if finger moved too far from start position
            float sqrDist = Vector3.SqrMagnitude( touches.GetAveragePosition() - gesture.StartPosition );

            if( sqrDist >= ToSqrPixels( MoveTolerance ) )
            {
                //Debug.LogWarning( "MultiTap - moved away from original position, requesting restart" );
                return GestureRecognitionState.FailAndRetry;
            }
        }

        if( gesture.WasDown != gesture.Down )
        {
            // fingers were just released
            if( !gesture.Down )
            {
                ++gesture.Taps;
                gesture.LastTapTime = Time.time;

                // If the requested tap count has been reached, validate the gesture and stop
                if( gesture.Taps >= RequiredTaps )
                    return GestureRecognitionState.Recognized;
            }
        }

        return GestureRecognitionState.InProgress;
    }
    
    public override string GetDefaultEventMessageName()
    {
        return "OnTap";
    }

    protected override void OnBegin( TapGesture gesture, FingerGestures.IFingerList touches )
    {
        gesture.Position = touches.GetAveragePosition();
        gesture.StartPosition = gesture.Position;
        gesture.LastTapTime = Time.time;
    }

    protected override GestureRecognitionState OnRecognize( TapGesture gesture, FingerGestures.IFingerList touches )
    {
        return IsMultiTap ? RecognizeMultiTap( gesture, touches ) : RecognizeSingleTap( gesture, touches );
    }
}

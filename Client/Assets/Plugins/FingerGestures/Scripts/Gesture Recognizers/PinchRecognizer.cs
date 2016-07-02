using UnityEngine;
using System.Collections;

public class PinchGesture : ContinuousGesture
{
    float delta = 0;
    float gap = 0;
    
    /// <summary>
    /// Gap difference since last frame, in pixels
    /// </summary>
    public float Delta
    {
        get { return delta; }
        internal set { delta = value; }
    }

    /// <summary>
    /// Current gap distance between the two pinching fingers, in pixels
    /// </summary>
    public float Gap
    {
        get { return gap; }
        internal set { gap = value; }
    }
}

/// <summary>
/// Pinch Gesture Recognizer
///   Two fingers moving closer or further away from each other
/// </summary>
[AddComponentMenu( "FingerGestures/Gestures/Pinch Recognizer" )]
public class PinchRecognizer : ContinuousGestureRecognizer<PinchGesture>
{
    /// <summary>
    /// Pinch DOT product treshold - this controls how tolerant the pinch gesture detector is to the two fingers
    /// moving in opposite directions.
    /// Setting this to -1 means the fingers have to move in exactly opposite directions to each other.
    /// this value should be kept between -1 and 0 excluded.
    /// </summary>    
    public float MinDOT = -0.7f;

    /// <summary>
    /// Minimum pinch distance required to trigger the pinch gesture
    /// <seealso cref="DistanceUnit"/>
    /// </summary>
    public float MinDistance = 0.25f;

    public override string GetDefaultEventMessageName()
    {
        return "OnPinch";
    }

    // Only support 2 simultaneous fingers right now
    public override int RequiredFingerCount
    {
        get { return 2; }
        set 
        { 
            if( Application.isPlaying )
                Debug.LogWarning( "Pinch only supports 2 fingers" ); 
        }
    }

    // TEMP: multi-gesture tracking is not supported for Pinch gesture yet
    public override bool SupportFingerClustering
    {
        get { return false; }
    }

    protected override GameObject GetDefaultSelectionForSendMessage( PinchGesture gesture )
    {
        return gesture.StartSelection;
    }

    public override GestureResetMode GetDefaultResetMode()
    {
        return GestureResetMode.NextFrame;
    }

    protected override bool CanBegin( PinchGesture gesture, FingerGestures.IFingerList touches )
    {
        if( !base.CanBegin( gesture, touches ) )
            return false;

        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        if( !FingerGestures.AllFingersMoving( finger0, finger1 ) )
            return false;

        if( !FingersMovedInOppositeDirections( finger0, finger1 ) )
            return false;

        float startGapSqr = Vector2.SqrMagnitude( finger0.StartPosition - finger1.StartPosition );
        float curGapSqr = Vector2.SqrMagnitude( finger0.Position - finger1.Position );

        if( Mathf.Abs( startGapSqr - curGapSqr ) < ToSqrPixels( MinDistance ) )
            return false;

        return true;
    }

    protected override void OnBegin( PinchGesture gesture, FingerGestures.IFingerList touches )
    {
        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        gesture.StartPosition = 0.5f * ( finger0.StartPosition + finger1.StartPosition );
        gesture.Position = 0.5f * ( finger0.Position + finger1.Position );

        float prevGap = Vector2.Distance( finger0.PreviousPosition, finger1.PreviousPosition );
        float curGap = Vector2.Distance( finger0.Position, finger1.Position );
        gesture.Delta = curGap - prevGap;
        gesture.Gap = curGap;
    }

    protected override GestureRecognitionState OnRecognize( PinchGesture gesture, FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
        {
            gesture.Delta = 0;

            // fingers were lifted?
            if( touches.Count < RequiredFingerCount )
                return GestureRecognitionState.Recognized;

            // more fingers added, gesture failed
            return GestureRecognitionState.Failed;
        }

        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        gesture.Position = 0.5f * ( finger0.Position + finger1.Position );

        // dont do anything if both fingers arent moving
        if( !FingerGestures.AllFingersMoving( finger0, finger1 ) )
            return GestureRecognitionState.InProgress;

        float curGap = Vector2.Distance( finger0.Position, finger1.Position );
        float newDelta = curGap - gesture.Gap;
        gesture.Gap = curGap;

        if( Mathf.Abs( newDelta ) > 0.001f )
        {
            if( !FingersMovedInOppositeDirections( finger0, finger1 ) )
            {
                // skip without firing event
                return GestureRecognitionState.InProgress; //TODO: might want to make this configurable, so the recognizer can fail if fingers move in same direction
            }

            gesture.Delta = newDelta;
            RaiseEvent( gesture );
        }

        return GestureRecognitionState.InProgress;
    }

    #region Utils

    bool FingersMovedInOppositeDirections( FingerGestures.Finger finger0, FingerGestures.Finger finger1 )
    {
        return FingerGestures.FingersMovedInOppositeDirections( finger0, finger1, MinDOT );
    }

    #endregion
}

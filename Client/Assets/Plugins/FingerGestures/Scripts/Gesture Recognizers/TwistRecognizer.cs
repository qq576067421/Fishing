using UnityEngine;
using System.Collections;

public class TwistGesture : ContinuousGesture
{
    /// <summary>
    /// Rotation angle change since last move (in degrees)
    /// </summary>
    public float DeltaRotation { get; internal set; }

    /// <summary>
    /// Total rotation angle since gesture started (in degrees)
    /// </summary>
    public float TotalRotation { get; internal set; }

    /// <summary>
    /// The finger used as pivot (only valid when using TwistMethod.Pivot)
    /// </summary>
    public FingerGestures.Finger Pivot { get; internal set; }
}

public enum TwistMethod
{
    /// <summary>
    /// Both fingers are rotating around a mid point
    /// </summary>
    Standard,

    /// <summary>
    /// One finger act as the pivot while the other one rotates around it
    /// </summary>
    Pivot,
}

/// <summary>
/// Twist Gesture Recognizer (formerly known as rotation gesture)
///   Two fingers moving around a pivot point in circular/opposite directions
/// </summary>
[AddComponentMenu( "FingerGestures/Gestures/Twist Recognizer" )]
public class TwistRecognizer : ContinuousGestureRecognizer<TwistGesture>
{
    /// <summary>
    /// Used to select the desired twist interaction method
    /// </summary>
    public TwistMethod Method = TwistMethod.Standard;

    /// <summary>
    /// Rotation DOT product treshold - this controls how tolerant the twist gesture detector is to the two fingers
    /// moving in opposite directions.
    /// Setting this to -1 means the fingers have to move in exactly opposite directions to each other.
    /// this value should be kept between -1 and 0 excluded.
    /// </summary>
    public float MinDOT = -0.7f;

    /// <summary>
    /// Minimum amount of rotation required to start the rotation gesture (in degrees)
    /// </summary>
    public float MinRotation = 1.0f;

    /// <summary>
    /// How far the pivot finger is allowed to move away from its initial position without causing the recognition to fail
    /// </summary>
    public float PivotMoveTolerance = 0.5f; 
    
    public override string GetDefaultEventMessageName()
    {
        return "OnTwist";
    }

    // Only support 2 simultaneous fingers right now
    public override int RequiredFingerCount
    {
        get { return 2; }
        set 
        {
            if( Application.isPlaying )
                Debug.LogWarning( "Twist only supports 2 fingers" );
        }
    }

    // TEMP: multi-gesture tracking is not supported for the Twist gesture yet
    public override bool SupportFingerClustering
    {
        get { return false; }
    }

    public override GestureResetMode GetDefaultResetMode()
    {
        return GestureResetMode.NextFrame;
    }
    
    protected override GameObject GetDefaultSelectionForSendMessage( TwistGesture gesture )
    {
        return gesture.StartSelection;
    }

    protected override void Reset( TwistGesture gesture )
    {
        base.Reset( gesture );
        gesture.Pivot = null;
    }

    FingerGestures.Finger GetTwistPivot( FingerGestures.Finger finger0, FingerGestures.Finger finger1 )
    {
        // exactly one finger must be moving
        if( finger0.IsMoving == finger1.IsMoving ) // both are either moving or not moving
        {
            //Debug.LogWarning( "Not exactly one finger moving" );
            return null;
        }

        // the pivot is the finger that's not moving
        FingerGestures.Finger pivot = finger0.IsMoving ? finger1 : finger0;

        // ensure the pivot has not moved away from its initial position beyond the move tolerance radius
        if( pivot.DistanceFromStart > ToPixels( PivotMoveTolerance ) )
        {
            //Debug.LogWarning( "Pivot moved too far" );
            return null;
        }

        return pivot;
    }

    protected override bool CanBegin( TwistGesture gesture, FingerGestures.IFingerList touches )
    {
        if( !base.CanBegin( gesture, touches ) )
            return false;

        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        if( Method == TwistMethod.Pivot )
        {
            if( !GetTwistPivot( finger0, finger1 ) )
                return false;
        }
        else // standard
        {
            if( !FingerGestures.AllFingersMoving( finger0, finger1 ) )
                return false;

            if( !FingersMovedInOppositeDirections( finger0, finger1 ) )
                return false;
        }

        // check if we went past the minimum rotation amount treshold
        float rotation = SignedAngularGap( finger0, finger1, finger0.StartPosition, finger1.StartPosition );
        if( Mathf.Abs( rotation ) < MinRotation )
            return false;

        return true;
    }

    protected override void OnBegin( TwistGesture gesture, FingerGestures.IFingerList touches )
    {
        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        if( Method == TwistMethod.Pivot )
        {
            gesture.Pivot = GetTwistPivot( finger0, finger1 );
            gesture.StartPosition = gesture.Pivot.StartPosition;
        }
        else
        {
            gesture.Pivot = null;
            gesture.StartPosition = 0.5f * ( finger0.Position + finger1.Position ); //( finger0.StartPosition + finger1.StartPosition );
        }
        
        gesture.Position = gesture.StartPosition;
        gesture.TotalRotation = 0;
        gesture.DeltaRotation = 0;
    }

    protected override GestureRecognitionState OnRecognize( TwistGesture gesture, FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
        {
            gesture.DeltaRotation = 0;

            // fingers were lifted?
            if( touches.Count < RequiredFingerCount )
                return GestureRecognitionState.Ended;

            // more fingers added, gesture failed
            return GestureRecognitionState.Failed;
        }

        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];
        
        // dont do anything if both fingers arent moving
        if( Method == TwistMethod.Pivot )
        {
            if( gesture.Pivot == null )
            {
                Debug.LogWarning( "Twist - pivot finger is null!", this );
                return GestureRecognitionState.Failed;
            }

            if( gesture.Pivot != finger0 && gesture.Pivot != finger1 )
            {
                Debug.LogWarning( "Twist - lost track of pivot finger!", this );
                return GestureRecognitionState.Failed;
            }

            gesture.Position = gesture.Pivot.Position;
        }
        else // standard twist
        {
            // mid point between finger0 and finger1
            gesture.Position = 0.5f * ( finger0.Position + finger1.Position );
        }

        gesture.DeltaRotation = SignedAngularGap( finger0, finger1, finger0.PreviousPosition, finger1.PreviousPosition );
        
        // only raise event when the twist angle has changed
        if( Mathf.Abs( gesture.DeltaRotation ) > Mathf.Epsilon )
        {
            gesture.TotalRotation += gesture.DeltaRotation;
            RaiseEvent( gesture );
        }

        return GestureRecognitionState.InProgress;
    }

    #region Utils

    bool FingersMovedInOppositeDirections( FingerGestures.Finger finger0, FingerGestures.Finger finger1 )
    {
        return FingerGestures.FingersMovedInOppositeDirections( finger0, finger1, MinDOT );
    }

    // return signed angle in degrees between current finger position and ref positions
    static float SignedAngularGap( FingerGestures.Finger finger0, FingerGestures.Finger finger1, Vector2 refPos0, Vector2 refPos1 )
    {
        Vector2 curDir = ( finger0.Position - finger1.Position ).normalized;
        Vector2 refDir = ( refPos0 - refPos1 ).normalized;

        // check if we went past the minimum rotation amount treshold
        return Mathf.Rad2Deg * FingerGestures.SignedAngle( refDir, curDir );
    }

    #endregion
}

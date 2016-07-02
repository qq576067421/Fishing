using UnityEngine;
using System.Collections;

[System.Serializable]
public class LongPressGesture : DiscreteGesture
{
    // nothing for now!
}

/// <summary>
/// Long-Press gesture: detects when the finger is held down without moving, for a specific duration
/// </summary>
[AddComponentMenu( "FingerGestures/Gestures/Long Press Recognizer" )]
public class LongPressRecognizer : DiscreteGestureRecognizer<LongPressGesture>
{
    /// <summary>
    /// How long the finger must stay down without moving in order to validate the gesture
    /// </summary>
    public float Duration = 1.0f;

    /// <summary>
    /// How far the finger is allowed to move around its starting position without breaking the gesture
    /// <seealso cref="DistanceUnit"/>
    /// </summary>
    public float MoveTolerance = 0.5f;

    public override string GetDefaultEventMessageName()
    {
        return "OnLongPress";
    }
    
    protected override void OnBegin( LongPressGesture gesture, FingerGestures.IFingerList touches )
    {
        gesture.Position = touches.GetAveragePosition();
        gesture.StartPosition = gesture.Position;
    }

    protected override GestureRecognitionState OnRecognize( LongPressGesture gesture, FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
            return GestureRecognitionState.Failed;

        if( gesture.ElapsedTime >= Duration )
            return GestureRecognitionState.Recognized;

        // check if we moved too far from initial position
        if( touches.GetAverageDistanceFromStart() > ToPixels( MoveTolerance ) )
            return GestureRecognitionState.Failed;

        return GestureRecognitionState.InProgress;
    }
}

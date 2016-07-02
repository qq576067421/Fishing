using UnityEngine;
using System.Collections;

/// <summary>
/// A gesture recognizer delegate provides several hooks into the core gesture recognizer logic
/// and can be used to customize the behaviour of a specific gesture recognizer
/// </summary>
public abstract class GestureRecognizerDelegate : MonoBehaviour 
{
    /// <summary>
    /// Hoow to control whether or not a gesture recognizer is allowed to begin the gesture detection
    /// </summary>
    /// <param name="gesture">The gesture recognizer to consider</param>
    /// <param name="touches">A list of the touches forwarded to the gesture recognizer in this frame</param>
    public abstract bool CanBegin( Gesture gesture, FingerGestures.IFingerList touches );
}

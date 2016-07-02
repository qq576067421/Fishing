using UnityEngine;
using System.Collections;

/// <summary>
/// NOTE: if you implement your own input provider, make sure to adjust its execution order to run BEFORE FingerGestures.cs
/// to avoid off-by-one frame lag.
/// </summary>
public abstract class FGInputProvider : MonoBehaviour
{
    /// <summary>
    /// Maximum number of fingers/touches that can be down at once
    /// </summary>
    public abstract int MaxSimultaneousFingers { get; }

    /// <summary>
    /// Request the most recent input state for the given finger index
    /// </summary>
    /// <param name="fingerIndex"></param>
    /// <param name="down"></param>
    /// <param name="position"></param>
    public abstract void GetInputState( int fingerIndex, out bool down, out Vector2 position );
}

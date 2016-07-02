using UnityEngine;
using System.Collections;

public enum ContinuousGesturePhase
{
    None = 0,
    Started,
    Updated,
    Ended,
}

public abstract class ContinuousGesture : Gesture
{
    // for continuous gestures, current gesture phase (move this to own XContinuousGesture class)
    public ContinuousGesturePhase Phase
    {
        get
        {
            switch( State )
            {
                case GestureRecognitionState.Started:
                    return ContinuousGesturePhase.Started;

                case GestureRecognitionState.InProgress:
                    return ContinuousGesturePhase.Updated;

                case GestureRecognitionState.Ended:
                case GestureRecognitionState.Failed:
                    return ContinuousGesturePhase.Ended;

                default:
                    return ContinuousGesturePhase.None;
            }
        }
    }
}

/// <summary>
/// NOTE: continuous gestures are responsible for calling RaiseEvent() while State == InProgress in order to raise 
/// an event with Phase.Updated
/// </summary>
public abstract class ContinuousGestureRecognizer<T> : GestureRecognizerTS<T> where T : ContinuousGesture, new()
{
    protected override void Reset( T gesture )
    {
        base.Reset( gesture );
    }

    protected override void OnStateChanged( Gesture sender )
    {
        base.OnStateChanged( sender );

        T gesture = (T)sender;

        switch( gesture.State )
        {
            case GestureRecognitionState.Started:
                RaiseEvent( gesture );
                break;

            case GestureRecognitionState.Ended:
                RaiseEvent( gesture );
                break;

            case GestureRecognitionState.Failed:
                // dont raise event if we failed directly from Ready state
                if( gesture.PreviousState != GestureRecognitionState.Ready )
                    RaiseEvent( gesture );
                break;
        }
    }
}



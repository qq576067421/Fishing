using UnityEngine;
using System.Collections;

public class DiscreteGesture : Gesture { }

public abstract class DiscreteGestureRecognizer<T> : GestureRecognizerTS<T> where T : DiscreteGesture, new()
{
    protected override void OnStateChanged( Gesture sender )
    {
        base.OnStateChanged( sender );

        T gesture = (T)sender;

        if( gesture.State == GestureRecognitionState.Recognized )
            RaiseEvent( gesture );
    }
}

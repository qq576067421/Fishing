using UnityEngine;
using System.Collections;

public class FingerUpEvent : FingerEvent
{
    float timeHeldDown = 0;

    public float TimeHeldDown
    {
        get { return timeHeldDown; }
        internal set { timeHeldDown = value; }
    }
}

[AddComponentMenu( "FingerGestures/Finger Events/Finger Up Detector" )]
public class FingerUpDetector : FingerEventDetector<FingerUpEvent>
{
    public event FingerEventHandler OnFingerUp;
    public string MessageName = "OnFingerUp";
    
    protected override void ProcessFinger( FingerGestures.Finger finger )
    {
        if( !finger.IsDown && finger.WasDown )
        {
            FingerUpEvent e = GetEvent( finger );
            e.Name = MessageName;
            e.TimeHeldDown = Mathf.Max( 0, Time.time - finger.StarTime );
            UpdateSelection( e );

            if( OnFingerUp != null )
                OnFingerUp( e );

            TrySendMessage( e );
        }
    }
}

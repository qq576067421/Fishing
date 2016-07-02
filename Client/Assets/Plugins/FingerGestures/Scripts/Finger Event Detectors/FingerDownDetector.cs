using UnityEngine;
using System.Collections;

public class FingerDownEvent : FingerEvent 
{

}

[AddComponentMenu( "FingerGestures/Finger Events/Finger Down Detector" )]
public class FingerDownDetector : FingerEventDetector<FingerDownEvent>
{
    public event FingerEventHandler OnFingerDown;
    public string MessageName = "OnFingerDown";    
    
    protected override void ProcessFinger( FingerGestures.Finger finger )
    {
        if( finger.IsDown && !finger.WasDown )
        {
            FingerDownEvent e = GetEvent( finger.Index );
            e.Name = MessageName;
            UpdateSelection( e );

            if( OnFingerDown != null )
                OnFingerDown( e );

            TrySendMessage( e );
        }
    }
}

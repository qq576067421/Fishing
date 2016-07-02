using UnityEngine;
using System.Collections;

public enum FingerHoverPhase
{
    None = 0,
    Enter,
    Exit,
}

public class FingerHoverEvent : FingerEvent
{
    FingerHoverPhase phase = FingerHoverPhase.None;
    internal GameObject PreviousSelection;  // one entry per finger, keeps track of object under finger during previous frame

    /// <summary>
    /// Indicates the phase of the event (Enter/Exit)
    /// </summary>
    public FingerHoverPhase Phase
    {
        get { return phase; }
        internal set { phase = value; }
    }
}

/// <summary>
/// Tracks when a finger enters/stays/exits a valid collider
/// </summary>
[AddComponentMenu( "FingerGestures/Finger Events/Finger Hover Detector" )]
public class FingerHoverDetector : FingerEventDetector<FingerHoverEvent>
{
    public event FingerEventHandler OnFingerHover;
    public string MessageName = "OnFingerHover";

    protected override void Start()
    {
        base.Start();

        // Must have a Raycaster otherwise we can't find out what's under the finger!
        if( !Raycaster )
            Debug.LogWarning( "FingerHoverDetector component on " + this.name + " has no Raycaster set." );
    }

    bool FireEvent( FingerHoverEvent e, FingerHoverPhase phase )
    {
        e.Name = MessageName;
        e.Phase = phase;
        
        if( OnFingerHover != null )
            OnFingerHover( e );

        TrySendMessage( e );
        return true;
    }

    protected override void ProcessFinger( FingerGestures.Finger finger )
    {
        FingerHoverEvent e = GetEvent( finger );

        GameObject prevSelection = e.PreviousSelection;
        GameObject newSelection = finger.IsDown ? PickObject( finger.Position ) : null;

        if( newSelection != prevSelection )
        {
            if( prevSelection )
                FireEvent( e, FingerHoverPhase.Exit );

            if( newSelection )
            {
                e.Selection = newSelection;
                e.Raycast = Raycast;

                FireEvent( e, FingerHoverPhase.Enter );
            }
        }

        e.PreviousSelection = newSelection;
    }
}

using UnityEngine;
using System.Collections;

public enum FingerMotionPhase
{
    None = 0,
    Started,
    Updated,
    Ended,
}

public class FingerMotionEvent : FingerEvent
{
    FingerMotionPhase phase = FingerMotionPhase.None;
    Vector2 position = Vector2.zero;

    internal float StartTime = 0;

    public override Vector2 Position
    {
        get { return position; }
        internal set { position = value; }
    }

    /// <summary>
    /// Indicates the phase of the event
    /// </summary>
    public FingerMotionPhase Phase
    {
        get { return phase; }
        internal set { phase = value; }
    }

    /// <summary>
    /// How much time has elapsed since the "started" phase
    /// </summary>
    public float ElapsedTime
    {
        get { return Mathf.Max( 0, Time.time - StartTime ); }
    }
}

/// <summary>
/// Tracks the moving/stationary state of a given finger
/// </summary>
[AddComponentMenu( "FingerGestures/Finger Events/Finger Motion Detector" )]
public class FingerMotionDetector : FingerEventDetector<FingerMotionEvent>
{
    enum EventType
    {
        Move,
        Stationary
    }

    public event FingerEventHandler OnFingerMove;
    public event FingerEventHandler OnFingerStationary;

    public string MoveMessageName = "OnFingerMove";
    public string StationaryMessageName = "OnFingerStationary";
    public bool TrackMove = true;
    public bool TrackStationary = true;
    
    bool FireEvent( FingerMotionEvent e, EventType eventType, FingerMotionPhase phase, Vector2 position, bool updateSelection )
    {
        if( ( !TrackMove && eventType == EventType.Move ) || ( !TrackStationary && eventType == EventType.Stationary ) )
            return false;

        e.Phase = phase;
        e.Position = position;

        if( e.Phase == FingerMotionPhase.Started )
            e.StartTime = Time.time;

        if( updateSelection )
            UpdateSelection( e );

        if( eventType == EventType.Move )
        {
            e.Name = MoveMessageName;

            if( OnFingerMove != null )
                OnFingerMove( e );

            TrySendMessage( e );
        }
        else if( eventType == EventType.Stationary )
        {
            e.Name = StationaryMessageName;
            
            if( OnFingerStationary != null )
                OnFingerStationary( e );

            TrySendMessage( e );
        }
        else
        {
            Debug.LogWarning( "Unhandled FingerMotionDetector event type: " + eventType );
            return false;
        }

        return true;
    }

    protected override void ProcessFinger( FingerGestures.Finger finger )
    {
        FingerMotionEvent e = GetEvent( finger );

        bool selectionUpdated = false;

        // finger phase changed
        if( finger.Phase != finger.PreviousPhase )
        {
            switch( finger.PreviousPhase )
            {
                case FingerGestures.FingerPhase.Moving:
                    selectionUpdated |= FireEvent( e, EventType.Move, FingerMotionPhase.Ended, finger.Position, !selectionUpdated );
                    break;

                case FingerGestures.FingerPhase.Stationary:
                    selectionUpdated |= FireEvent( e, EventType.Stationary, FingerMotionPhase.Ended, finger.PreviousPosition, !selectionUpdated );
                    break;
            }

            switch( finger.Phase )
            {
                case FingerGestures.FingerPhase.Moving:
                    selectionUpdated |= FireEvent( e, EventType.Move, FingerMotionPhase.Started, finger.PreviousPosition, !selectionUpdated );
                    selectionUpdated |= FireEvent( e, EventType.Move, FingerMotionPhase.Updated, finger.Position, !selectionUpdated );
                    break;

                case FingerGestures.FingerPhase.Stationary:
                    selectionUpdated |= FireEvent( e, EventType.Stationary, FingerMotionPhase.Started, finger.Position, !selectionUpdated );
                    selectionUpdated |= FireEvent( e, EventType.Stationary, FingerMotionPhase.Updated, finger.Position, !selectionUpdated );
                    break;
            }
        }
        else // finger phase still the same
        {
            switch( finger.Phase )
            {
                case FingerGestures.FingerPhase.Moving:
                    selectionUpdated |= FireEvent( e, EventType.Move, FingerMotionPhase.Updated, finger.Position, !selectionUpdated );
                    break;

                case FingerGestures.FingerPhase.Stationary:
                    selectionUpdated |= FireEvent( e, EventType.Stationary, FingerMotionPhase.Updated, finger.Position, !selectionUpdated );
                    break;
            }
        }
    }
}

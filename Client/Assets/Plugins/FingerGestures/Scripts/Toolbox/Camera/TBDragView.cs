// FingerGestures copyright (c) William Ravaine

using UnityEngine;
using System.Collections;

/// <summary>
/// TBDragView
/// Utility script to control the an object/camera rotation using drag gesture. This is a bit similar to "google stree view".
/// </summary>
[AddComponentMenu( "FingerGestures/Toolbox/Camera/Drag View" )]
[RequireComponent( typeof( DragRecognizer ) )]
public class TBDragView : MonoBehaviour
{
    public bool allowUserInput = true;  // set this to false to prevent the user from dragging the view
    public float sensitivity = 360.0f;
    public float dragAcceleration = 40.0f;
    public float dragDeceleration = 15.0f;
    public bool reverseControls = false;
    public float minPitchAngle = -60.0f;
    public float maxPitchAngle = 60.0f;
    public float idealRotationSmoothingSpeed = 7.0f; // set to 0 to disable smoothing when rotating toward ideal direction

    Transform cachedTransform;
    Vector2 angularVelocity = Vector2.zero;
    Quaternion idealRotation;
    bool useAngularVelocity = false;

    DragGesture dragGesture;

    void Awake()
    {
        cachedTransform = this.transform;
    }

    void Start()
    {
        IdealRotation = cachedTransform.rotation;

        // sanity check
        if( !GetComponent<DragRecognizer>() )
        {
            Debug.LogWarning( "No drag recognizer found on " + this.name + ". Disabling TBDragView." );
            enabled = false;
        }
    }

    public bool Dragging
    {
        get { return dragGesture != null; }
    }

    // Handle Gesture Event (sent by the DragRecognizer component)
    void OnDrag( DragGesture gesture )
    {
        if( gesture.Phase != ContinuousGesturePhase.Ended )
            dragGesture = gesture;
        else
            dragGesture = null;
    }

    void Update()
    {
        if( Dragging && allowUserInput )
            useAngularVelocity = true;

        if( useAngularVelocity )
        {
            Vector3 localAngles = transform.localEulerAngles;
            Vector2 idealAngularVelocity = Vector2.zero;

            float accel = dragDeceleration;

            if( Dragging )
            {
                idealAngularVelocity = sensitivity * dragGesture.DeltaMove.Centimeters();
                accel = dragAcceleration;
            }

            angularVelocity = Vector2.Lerp( angularVelocity, idealAngularVelocity, Time.deltaTime * accel );
            Vector2 angularMove = Time.deltaTime * angularVelocity;

            if( reverseControls )
                angularMove = -angularMove;

            // pitch angle
            localAngles.x = Mathf.Clamp( NormalizePitch( localAngles.x + angularMove.y ), minPitchAngle, maxPitchAngle );

            // yaw angle
            localAngles.y -= angularMove.x;

            // apply
            transform.localEulerAngles = localAngles;
        }
        else
        {
            if( idealRotationSmoothingSpeed > 0 )
                cachedTransform.rotation = Quaternion.Slerp( cachedTransform.rotation, IdealRotation, Time.deltaTime * idealRotationSmoothingSpeed );
            else
                cachedTransform.rotation = idealRotation;
        }
    }

    static float NormalizePitch( float angle )
    {
        if( angle > 180.0f )
            angle -= 360.0f;

        return angle;
    }

    public Quaternion IdealRotation
    {
        get { return idealRotation; }
        set
        {
            idealRotation = value;
            useAngularVelocity = false;
        }
    }

    // Point the camera at the target point
    public void LookAt( Vector3 pos )
    {
        IdealRotation = Quaternion.LookRotation( pos - cachedTransform.position );
    }
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Adaptation of the standard MouseOrbit script to use the finger drag gesture to rotate the current object using
/// the fingers/mouse around a target object
/// </summary>
[AddComponentMenu( "FingerGestures/Toolbox/Camera/Orbit" )]
public class TBOrbit : MonoBehaviour
{
    public enum PanMode
    {
        Disabled,
        OneFinger,
        TwoFingers
    }

    /// <summary>
    /// The object to orbit around
    /// </summary>
    public Transform target;

    /// <summary>
    /// Initial camera distance to target
    /// </summary>
    public float initialDistance = 10.0f;

    /// <summary>
    /// Minimum distance between camera and target
    /// </summary>
    public float minDistance = 1.0f;

    /// <summary>
    /// Maximum distance between camera and target
    /// </summary>
    public float maxDistance = 30.0f;

    /// <summary>
    /// Affects horizontal rotation speed (in degrees per cm)
    /// </summary>
    public float yawSensitivity = 45.0f;

    /// <summary>
    /// Affects vertical rotation speed (in degrees per cm)
    /// </summary>
    public float pitchSensitivity = 45.0f;

    /// <summary>
    /// Keep yaw angle value between minYaw and maxYaw?
    /// </summary>
    public bool clampYawAngle = false;
    public float minYaw = -75;
    public float maxYaw = 75;

    /// <summary>
    /// Keep pitch angle value between minPitch and maxPitch?
    /// </summary>
    public bool clampPitchAngle = true;
    public float minPitch = -20;
    public float maxPitch = 80;

    /// <summary>
    /// Allow the user to affect the orbit distance using the pinch zoom gesture
    /// </summary>
    public bool allowPinchZoom = true;

    /// <summary>
    /// Affects pinch zoom speed
    /// </summary>
    public float pinchZoomSensitivity = 5.0f;

    /// <summary>
    /// Use smooth camera motions?
    /// </summary>
    public bool smoothMotion = true;
    public float smoothZoomSpeed = 5.0f;
    public float smoothOrbitSpeed = 10.0f;

    /// <summary>
    /// Two-Finger camera panning.
    /// Panning will apply an offset to the pivot/camera target point
    /// </summary>
    public bool allowPanning = false;
    public bool invertPanningDirections = false;
    public float panningSensitivity = 1.0f;
    public Transform panningPlane;  // reference transform used to apply the panning translation (using panningPlane.right and panningPlane.up vectors)
    public bool smoothPanning = true;
    public float smoothPanningSpeed = 12.0f;

    // collision test
    public LayerMask collisionLayerMask;

    float distance = 10.0f;
    float yaw = 0;
    float pitch = 0;

    float idealDistance = 0;
    float idealYaw = 0;
    float idealPitch = 0;

    Vector3 idealPanOffset = Vector3.zero;
    Vector3 panOffset = Vector3.zero;

    PinchRecognizer pinchRecognizer;

    public float Distance
    {
        get { return distance; }
    }

    public float IdealDistance
    {
        get { return idealDistance; }
        set { idealDistance = Mathf.Clamp( value, minDistance, maxDistance ); }
    }

    public float Yaw
    {
        get { return yaw; }
    }

    public float IdealYaw
    {
        get { return idealYaw; }
        set { idealYaw = clampYawAngle ? ClampAngle( value, minYaw, maxYaw ) : value; }
    }

    public float Pitch
    {
        get { return pitch; }
    }

    public float IdealPitch
    {
        get { return idealPitch; }
        set { idealPitch = clampPitchAngle ? ClampAngle( value, minPitch, maxPitch ) : value; }
    }

    public Vector3 IdealPanOffset
    {
        get { return idealPanOffset; }
        set { idealPanOffset = value; }
    }

    public Vector3 PanOffset
    {
        get { return panOffset; }
    }

    void InstallGestureRecognizers()
    {
        List<GestureRecognizer> recogniers = new List<GestureRecognizer>( GetComponents<GestureRecognizer>() );
        DragRecognizer drag = recogniers.Find( r => r.EventMessageName == "OnDrag" ) as DragRecognizer;
        DragRecognizer twoFingerDrag = recogniers.Find( r => r.EventMessageName == "OnTwoFingerDrag" ) as DragRecognizer;
        PinchRecognizer pinch = recogniers.Find( r => r.EventMessageName == "OnPinch" ) as PinchRecognizer;

        // check if we need to automatically add a screenraycaster
        if( OnlyRotateWhenDragStartsOnObject )
        {
            ScreenRaycaster raycaster = gameObject.GetComponent<ScreenRaycaster>();

            if( !raycaster )
                raycaster = gameObject.AddComponent<ScreenRaycaster>();
        }

        if( !drag )
        {
            drag = gameObject.AddComponent<DragRecognizer>();
            drag.RequiredFingerCount = 1;
            drag.IsExclusive = true;
            drag.MaxSimultaneousGestures = 1;
            drag.SendMessageToSelection = GestureRecognizer.SelectionType.None;
        }

        if( !pinch )
            pinch = gameObject.AddComponent<PinchRecognizer>();

        if( !twoFingerDrag )
        {
            twoFingerDrag = gameObject.AddComponent<DragRecognizer>();
            twoFingerDrag.RequiredFingerCount = 2;
            twoFingerDrag.IsExclusive = true;
            twoFingerDrag.MaxSimultaneousGestures = 1;
            twoFingerDrag.ApplySameDirectionConstraint = true;
            twoFingerDrag.EventMessageName = "OnTwoFingerDrag";
        }
    }

    void Start()
    {
        InstallGestureRecognizers();

        if( !panningPlane )
            panningPlane = this.transform;

        Vector3 angles = transform.eulerAngles;

        distance = IdealDistance = initialDistance;
        yaw = IdealYaw = angles.y;
        pitch = IdealPitch = angles.x;

        // Make the rigid body not change rotation
        if( GetComponent<Rigidbody>() )
            GetComponent<Rigidbody>().freezeRotation = true;

        Apply();
    }

    #region Gesture Event Messages

    float nextDragTime = 0;

    public bool OnlyRotateWhenDragStartsOnObject = false;

    void OnDrag( DragGesture gesture )
    {
        // don't rotate unless the drag started on our target object
        if( OnlyRotateWhenDragStartsOnObject )
        {
            if( gesture.Phase == ContinuousGesturePhase.Started )
            {
                if( !gesture.Recognizer.Raycaster )
                {
                    Debug.LogWarning( "The drag recognizer on " + gesture.Recognizer.name + " has no ScreenRaycaster component set. This will prevent OnlyRotateWhenDragStartsOnObject flag from working." );
                    OnlyRotateWhenDragStartsOnObject = false;
                    return;
                }

                if( target && !target.GetComponent<Collider>() )
                {
                    Debug.LogWarning( "The target object has no collider set. OnlyRotateWhenDragStartsOnObject won't work." );
                    OnlyRotateWhenDragStartsOnObject = false;
                    return;
                }
            }

            if( !target || gesture.StartSelection != target.gameObject )
                return;
        }

        // wait for drag cooldown timer to wear off
        //  used to avoid dragging right after a pinch or pan, when lifting off one finger but the other one is still on screen
        if( Time.time < nextDragTime )
            return;

        if( target )
        {
            IdealYaw += gesture.DeltaMove.x.Centimeters() * yawSensitivity;
            IdealPitch -= gesture.DeltaMove.y.Centimeters() * pitchSensitivity;
        }
    }

    void OnPinch( PinchGesture gesture )
    {
        if( allowPinchZoom )
        {
            IdealDistance -= gesture.Delta.Centimeters() * pinchZoomSensitivity;
            nextDragTime = Time.time + 0.25f;
        }
    }

    void OnTwoFingerDrag( DragGesture gesture )
    {
        if( allowPanning )
        {
            Vector3 move = -panningSensitivity * ( panningPlane.right * gesture.DeltaMove.x.Centimeters() + panningPlane.up * gesture.DeltaMove.y.Centimeters() );

            if( invertPanningDirections )
                IdealPanOffset -= move;
            else
                IdealPanOffset += move;

            nextDragTime = Time.time + 0.25f;
        }
    }

    #endregion

    void Apply()
    {
        if( smoothMotion )
        {
            distance = Mathf.Lerp( distance, IdealDistance, Time.deltaTime * smoothZoomSpeed );
            yaw = Mathf.Lerp( yaw, IdealYaw, Time.deltaTime * smoothOrbitSpeed );
            pitch = Mathf.LerpAngle( pitch, IdealPitch, Time.deltaTime * smoothOrbitSpeed );
        }
        else
        {
            distance = IdealDistance;
            yaw = IdealYaw;
            pitch = IdealPitch;
        }

        if( smoothPanning )
            panOffset = Vector3.Lerp( panOffset, idealPanOffset, Time.deltaTime * smoothPanningSpeed );
        else
            panOffset = idealPanOffset;

        transform.rotation = Quaternion.Euler( pitch, yaw, 0 );

        Vector3 lookAtPos = ( target.position + panOffset );
        Vector3 desiredPos = lookAtPos - distance * transform.forward;

        if( collisionLayerMask != 0 )
        {
            Vector3 dir = desiredPos - lookAtPos; // from target to camera
            float dist = dir.magnitude;
            dir.Normalize();

            RaycastHit hit;
            if( Physics.Raycast( lookAtPos, dir, out hit, dist, collisionLayerMask ) )
            {
                desiredPos = hit.point - dir * 0.1f;
                distance = hit.distance;
            }
        }

        transform.position = desiredPos;
    }

    void LateUpdate()
    {
        Apply();
    }

    static float ClampAngle( float angle, float min, float max )
    {
        if( angle < -360 )
            angle += 360;

        if( angle > 360 )
            angle -= 360;

        return Mathf.Clamp( angle, min, max );
    }

    // recenter the camera
    public void ResetPanning()
    {
        IdealPanOffset = Vector3.zero;
    }
}
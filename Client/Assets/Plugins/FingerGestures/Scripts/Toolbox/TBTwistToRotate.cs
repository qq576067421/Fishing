using UnityEngine;
using System.Collections;

/// <summary>
/// A simple twist gesture handler to manipulate the current object's rotation
/// Allows the user to pick from a list of rotation axis (world/local/camera) to rotate the object around
/// </summary>
[AddComponentMenu( "FingerGestures/Toolbox/Twist To Rotate" )]
public class TBTwistToRotate : MonoBehaviour
{
    public enum RotationAxis
    {
        // global/world axis
        WorldX,
        WorldY,
        WorldZ,

        // local axis
        ObjectX,
        ObjectY,
        ObjectZ,

        // camera axis
        CameraX,
        CameraY,
        CameraZ
    }

    public float Sensitivity = 1.0f;
    public RotationAxis Axis = RotationAxis.WorldY;
    public Camera ReferenceCamera;

    void Start()
    {
        if( !ReferenceCamera )
            ReferenceCamera = Camera.main;
    }

    // determine current rotation axis
    public Vector3 GetRotationAxis()
    {
        switch( Axis )
        {
            case RotationAxis.WorldX:
                return Vector3.right;

            case RotationAxis.WorldY:
                return Vector3.up;

            case RotationAxis.WorldZ:
                return Vector3.forward;

            case RotationAxis.ObjectX:
                return transform.right;

            case RotationAxis.ObjectY:
                return transform.up;

            case RotationAxis.ObjectZ:
                return transform.forward;

            case RotationAxis.CameraX:
                return ReferenceCamera.transform.right;

            case RotationAxis.CameraY:
                return ReferenceCamera.transform.up;

            case RotationAxis.CameraZ:
                return ReferenceCamera.transform.forward;
        }

        Debug.LogWarning( "Unhandled rotation axis: " + Axis );
        return Vector3.forward;
    }

    // event message sent by FingerGestures
    void OnTwist( TwistGesture gesture )
    {
        // rotate around current rotation axis by amount proportional to rotation gesture's angle delta
        Quaternion qRot = Quaternion.AngleAxis( Sensitivity * gesture.DeltaRotation, GetRotationAxis() );

        // apply rotation to current object
        transform.rotation = qRot * transform.rotation;
    }
}

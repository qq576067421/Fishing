using UnityEngine;
using System.Collections;

/// <summary>
/// Put this script on a Camera object to allow for pinch-zoom gesture.
/// NOTE: this script does NOT require a TBInputManager instance to be present in the scene.
/// </summary>
[AddComponentMenu( "FingerGestures/Toolbox/Camera/Pinch-Zoom" )]
[RequireComponent( typeof( Camera ) )]
[RequireComponent( typeof( PinchRecognizer ) )]
public class TBPinchZoom : MonoBehaviour
{
    public enum ZoomMethod
    {
        // move the camera position forward/backward
        Position,

        // change the field of view of the camera, or projection size for orthographic cameras
        FOV,
    }

    public ZoomMethod zoomMethod = ZoomMethod.Position;
    public float zoomSpeed = 5.0f;
    public float minZoomAmount = 0;
    public float maxZoomAmount = 50;
    public float SmoothSpeed = 4.0f;

    Vector3 defaultPos = Vector3.zero;
    float defaultFov = 0;
    float defaultOrthoSize = 0;
    float idealZoomAmount = 0;
    float zoomAmount = 0;

    public Vector3 DefaultPos
    {
        get { return defaultPos; }
        set { defaultPos = value; }
    }

    public float DefaultFov
    {
        get { return defaultFov; }
        set { defaultFov = value; }
    }

    public float DefaultOrthoSize
    {
        get { return defaultOrthoSize; }
        set { defaultOrthoSize = value; }
    }

    public float IdealZoomAmount
    {
        get { return idealZoomAmount; }
        set { idealZoomAmount = Mathf.Clamp( value, minZoomAmount, maxZoomAmount ); }
    }

    public float ZoomAmount
    {
        get { return zoomAmount; }
        set
        {
            zoomAmount = Mathf.Clamp( value, minZoomAmount, maxZoomAmount );

            switch( zoomMethod )
            {
                case ZoomMethod.Position:
                    transform.position = defaultPos + zoomAmount * transform.forward;
                    break;

                case ZoomMethod.FOV:
                    if( GetComponent<Camera>().orthographic )
                    {
                        GetComponent<Camera>().orthographicSize = Mathf.Max( defaultOrthoSize - zoomAmount, 0.1f );
                    }
                    else
                    {
                        CameraFov = Mathf.Max( defaultFov - zoomAmount, 0.1f );
                    }
                    break;
            }
        }
    }

    float CameraFov
    {
#if UNITY_3_5
        get { return camera.fov; }
        set { camera.fov = value; }
#else
        get { return GetComponent<Camera>().fieldOfView; }
        set { GetComponent<Camera>().fieldOfView = value; }
#endif
    }

    public float ZoomPercent
    {
        get { return ( ZoomAmount - minZoomAmount ) / ( maxZoomAmount - minZoomAmount ); }
    }

    void Start()
    {
        if( !GetComponent<PinchRecognizer>() )
        {
            Debug.LogWarning( "No pinch recognizer found on " + this.name + ". Disabling TBPinchZoom." );
            enabled = false;
        }

        SetDefaults();
    }

    void Update()
    {
        ZoomAmount = Mathf.Lerp( ZoomAmount, IdealZoomAmount, Time.deltaTime * SmoothSpeed );
    }

    public void SetDefaults()
    {
        DefaultPos = transform.position;
        DefaultFov = CameraFov;
        DefaultOrthoSize = GetComponent<Camera>().orthographicSize;
    }

    // Handle the pinch event
    void OnPinch( PinchGesture gesture )
    {
        IdealZoomAmount += zoomSpeed * gesture.Delta.Centimeters();
    }
}

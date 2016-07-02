using UnityEngine;
using System.Collections;

public class FGMouseInputProvider : FGInputProvider
{
    public int maxButtons = 3;

    public string pinchAxis = "Mouse ScrollWheel";
    public float pinchAxisScale = 100.0f;
    public float pinchResetTimeDelay = 0.15f;
    public float initialPinchDistance = 150;

    public string twistAxis = "Mouse ScrollWheel";
    public float twistAxisScale = 100.0f;
    public KeyCode twistKey = KeyCode.LeftControl;
    public float twistResetTimeDelay = 0.15f;

    public KeyCode pivotKey = KeyCode.LeftAlt;
    bool pivoting = false;

    // holding both Shift + Control will allow to pinch & twist at same time
    public KeyCode twistAndPinchKey = KeyCode.LeftShift;

    Vector2 pivot = Vector2.zero;
    Vector2[] pos = { Vector2.zero, Vector2.zero };

    bool pinching = false;
    float pinchResetTime = 0;
    float pinchDistance = 0;

    bool twisting = false;
    float twistAngle = 0;
    float twistResetTime = 0;

    void Start()
    {
        pinchDistance = initialPinchDistance;
    }

    void Update()
    {
        bool wasPinchingOrTwisting = pinching || twisting;

        UpdatePinchEmulation();
        UpdateTwistEmulation();

        if( pinching || twisting )
        {
            // dont move the pivot point after the start phase
            if( !wasPinchingOrTwisting )
                pivot = Input.mousePosition;

            float angle = 0;
            float radius = initialPinchDistance;

            if( pinching && twisting && Input.GetKey( twistAndPinchKey ) )
            {
                angle = Mathf.Deg2Rad * twistAngle;
                radius = pinchDistance;
            }
            else if( twisting )
            {
                angle = Mathf.Deg2Rad * twistAngle;
            }
            else if( pinching )
            {
                radius = pinchDistance;
            }

            float cos = Mathf.Cos( angle );
            float sin = Mathf.Sin( angle );

            pos[0].x = pivot.x - 0.5f * radius * cos;
            pos[0].y = pivot.y - 0.5f * radius * sin;
            pos[1].x = pivot.x + 0.5f * radius * cos;
            pos[1].y = pivot.y + 0.5f * radius * sin;
        }

        if( Input.GetKey( pivotKey ) )
        {
            if( Input.GetKeyDown( pivotKey ) )
            {
                pivot = Input.mousePosition;
            }

            if( !pivoting )
            {
                if( Vector2.Distance( Input.mousePosition, pivot ) > 50.0f )
                    pivoting = true;
            }

            if( pivoting )
            {
                pos[0] = pivot;
                pos[1] = Input.mousePosition;
            }
        }
        else
        {
            pivoting = false;
        }
    }

    void UpdatePinchEmulation()
    {
        float pinchAxisMotion = pinchAxisScale * Input.GetAxis( pinchAxis );

        if( Mathf.Abs( pinchAxisMotion ) > 0.0001f )
        {
            if( !pinching )
            {
                pinching = true;
                pinchDistance = initialPinchDistance;
            }

            pinchResetTime = Time.time + pinchResetTimeDelay;
            pinchDistance = Mathf.Max( 5.0f, pinchDistance + pinchAxisMotion );

        }
        else if( pinchResetTime <= Time.time )
        {
            pinching = false;
            pinchDistance = initialPinchDistance;
        }
    }

    void UpdateTwistEmulation()
    {
        float twistAxisMotion = twistAxisScale * Input.GetAxis( twistAxis );

        if( twistKey != KeyCode.None &&
            Input.GetKey( twistKey ) &&
            Mathf.Abs( twistAxisMotion ) > 0.0001f )
        {
            if( !twisting )
            {
                twisting = true;
                twistAngle = 0;
            }

            twistResetTime = Time.time + twistResetTimeDelay;
            twistAngle += twistAxisMotion;
        }
        else if( twistResetTime <= Time.time )
        {
            twisting = false;
            twistAngle = 0;
        }
    }

    #region FGInputProvider Implementation

    public override int MaxSimultaneousFingers
    {
        get { return maxButtons; }
    }

    public override void GetInputState( int fingerIndex, out bool down, out Vector2 position )
    {
        down = Input.GetMouseButton( fingerIndex );
        position = Input.mousePosition;

        if( ( pivoting || pinching || twisting ) && ( fingerIndex == 0 || fingerIndex == 1 ) )
        {
            down = true;
            position = pos[fingerIndex];
        }
    }

    #endregion
}

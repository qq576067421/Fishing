using UnityEngine;
using System.Collections;

[AddComponentMenu( "FingerGestures/Toolbox/Pinch To Scale" )]
public class TBPinchToScale : MonoBehaviour
{
    public Vector3 scaleWeights = Vector3.one;
    public float minScaleAmount = 0.5f;
    public float maxScaleAmount = 2.0f;
    public float sensitivity = 1.0f;
    public float smoothingSpeed = 12.0f;    // set to 0 to disable smoothing

    float idealScaleAmount = 1.0f;
    float scaleAmount = 1.0f;
    Vector3 baseScale = Vector3.one;

    public float ScaleAmount
    {
        get { return scaleAmount; }
        
        set 
        { 
            value = Mathf.Clamp( value, minScaleAmount, maxScaleAmount );
            
            if( value != scaleAmount )
            {
                scaleAmount = value;

                Vector3 s = scaleAmount * baseScale;
                s.x *= scaleWeights.x;
                s.y *= scaleWeights.y;
                s.z *= scaleWeights.z;

                transform.localScale = s;
            }
        }
    }

    public float IdealScaleAmount
    {
        get { return idealScaleAmount; }
        set { idealScaleAmount = Mathf.Clamp( value, minScaleAmount, maxScaleAmount ); }
    }

    void Start()
    {
        baseScale = transform.localScale;
        IdealScaleAmount = ScaleAmount;
    }

    void Update()
    {
        if( smoothingSpeed > 0 )
            ScaleAmount = Mathf.Lerp( ScaleAmount, IdealScaleAmount, Time.deltaTime * smoothingSpeed );
        else
            ScaleAmount = IdealScaleAmount;
    }

    void OnPinch( PinchGesture gesture )
    {  
        IdealScaleAmount += sensitivity * gesture.Delta.Centimeters();
    }
}

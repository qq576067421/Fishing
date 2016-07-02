using UnityEngine;
using System.Collections.Generic;

public struct ScreenRaycastData
{
    public bool Is2D;
    public RaycastHit Hit3D;

#if !UNITY_3_5
    public RaycastHit2D Hit2D;
#endif

    public GameObject GameObject
    {
        get
        {
#if !UNITY_3_5
            if( Is2D )
                return Hit2D.collider ? Hit2D.collider.gameObject : null;
#endif
            return Hit3D.collider ? Hit3D.collider.gameObject : null;
        }
    }
}

[AddComponentMenu( "FingerGestures/Components/Screen Raycaster" )]
public class ScreenRaycaster : MonoBehaviour
{
    /// <summary>
    /// List of cameras to use for each raycast. Each camera will be considered in the order specified in this list,
    /// and the Raycast method will continue until a hit is detected.
    /// </summary>
    public Camera[] Cameras;

    /// <summary>
    /// Layers to ignore when raycasting
    /// </summary>
    public LayerMask IgnoreLayerMask;

    /// <summary>
    /// Thickness of the ray. 
    /// Setting rayThickness to 0 will use a normal Physics.Raycast()
    /// Setting rayThickness to > 0 will use Physics.SphereCast() of radius equal to half rayThickness
    ///  ** IMPORTANT NOTE ** According to Unity's documentation, Physics.SphereCast() doesn't work on colliders setup as triggers
    /// </summary>
    public float RayThickness = 0;

    /// <summary>
    /// Property used while in the editor only. 
    /// Toggles the visualization of the raycasts as red lines for misses, and green lines for hits (visible in scene view only)
    /// </summary>
    public bool VisualizeRaycasts = true;

    /// <summary>
    /// Raycast using Physics2D on orthographic cameras (Unity 4.X+ only)
    /// </summary>
    public bool UsePhysics2D = true;
    
    void Start()
    {
        // if no cameras were explicitely provided, use the current main camera
        if( Cameras == null || Cameras.Length == 0 )
            Cameras = new Camera[] { Camera.main };
    }

    public bool Raycast( Vector2 screenPos, out ScreenRaycastData hitData )
    {
        for( int i = 0; i < Cameras.Length; ++i )
        {
            Camera cam = Cameras[i];

            // dont raycast from disabled cams
            if( !cam || !cam.enabled )
                continue;

#if UNITY_3_5
            if( !cam.gameObject.active )
                continue;
#else
            if( !cam.gameObject.activeInHierarchy )
                continue;
#endif

            if( Raycast( cam, screenPos, out hitData ) )
                return true;
        }

        hitData = new ScreenRaycastData();
        return false;
    }

    bool Raycast( Camera cam, Vector2 screenPos, out ScreenRaycastData hitData )
    {
        Ray ray = cam.ScreenPointToRay( screenPos );
        bool didHit = false;

        hitData = new ScreenRaycastData();

#if !UNITY_3_5
        // try to raycast 2D first - this only makes sense on orthographic cameras (physics2D doesnt work with perspective cameras)
        if( UsePhysics2D && cam.orthographic )
        {
            hitData.Hit2D = Physics2D.Raycast( ray.origin, Vector2.zero, Mathf.Infinity, ~IgnoreLayerMask );

            if( hitData.Hit2D.collider )
            {
                hitData.Is2D = true;
                didHit = true;
            }
        }
#endif

        // regular 3D raycast
        if( !didHit )
        {
            hitData.Is2D = false;   // ensure this is false

            if( RayThickness > 0 )
                didHit = Physics.SphereCast( ray, 0.5f * RayThickness, out hitData.Hit3D, Mathf.Infinity, ~IgnoreLayerMask );
            else
                didHit = Physics.Raycast( ray, out hitData.Hit3D, Mathf.Infinity, ~IgnoreLayerMask );
        }

        // vizualise ray
    #if UNITY_EDITOR
        if( VisualizeRaycasts )
        {
            if( didHit )
            {
                Vector3 hitPos = hitData.Hit3D.point;

#if !UNITY_3_5
                if( hitData.Is2D )
                {
                    hitPos = hitData.Hit2D.point;
                    hitPos.z = hitData.GameObject.transform.position.z;
                }
#endif

                Debug.DrawLine( ray.origin, hitPos, Color.green, 0.5f );
            }
            else
            {
                Debug.DrawLine( ray.origin, ray.origin + ray.direction * 9999.0f, Color.red, 0.5f );
            }
        }
    #endif

        return didHit;
    }
}

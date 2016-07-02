// FingerGestures copyright (c) William Ravaine

using UnityEngine;
using System.Collections;

/// <summary>
/// TBLookAtTap
/// Utility script that will point the current object's transform/TBDragView at the scene point tapped by the user
/// </summary>
[AddComponentMenu( "FingerGestures/Toolbox/Camera/Look At Tap" )]
[RequireComponent( typeof( TapRecognizer ) )]
public class TBLookAtTap : MonoBehaviour
{
    TBDragView dragView;

    void Awake()
    {
        dragView = GetComponent<TBDragView>();
    }

    void Start()
    {
        // sanity check
        if( !GetComponent<TapRecognizer>() )
        {
            Debug.LogWarning( "No tap recognizer found on " + this.name + ". Disabling TBLookAtTap." );
            enabled = false;
        }
    }

    void OnTap( TapGesture gesture )
    {
        Ray ray = Camera.main.ScreenPointToRay( gesture.Position );
        RaycastHit hit;

        if( Physics.Raycast( ray, out hit ) )
        {
            if( dragView )
                dragView.LookAt( hit.point );
            else
                transform.LookAt( hit.point );
        }
    }
}

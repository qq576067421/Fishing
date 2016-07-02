using UnityEngine;
using System.Collections;

public class TBHoverChangeScale : MonoBehaviour
{
    public float hoverScaleFactor = 1.5f;
    Vector3 originalScale = Vector3.one;

    void Start()
    {
        // remember our original scale
        originalScale = transform.localScale;
    }

    void OnFingerHover( FingerHoverEvent e )
    {
        if( e.Phase == FingerHoverPhase.Enter )
        {
            // apply scale modifier
            transform.localScale = hoverScaleFactor * originalScale;
        }
        else
        {
            // restore original scale
            transform.localScale = originalScale;
        }
    }
}

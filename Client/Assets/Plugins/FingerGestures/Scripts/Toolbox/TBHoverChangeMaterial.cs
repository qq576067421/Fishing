using UnityEngine;
using System.Collections;

public class TBHoverChangeMaterial : MonoBehaviour
{
    public Material hoverMaterial;
    Material normalMaterial;

    void Start()
    {
        // remember our original material
        normalMaterial = GetComponent<Renderer>().sharedMaterial;
    }

    void OnFingerHover( FingerHoverEvent e )
    {
        if( e.Phase == FingerHoverPhase.Enter )
            GetComponent<Renderer>().sharedMaterial = hoverMaterial; // show hover-state material
        else
            GetComponent<Renderer>().sharedMaterial = normalMaterial; // restore original material
    }
}

using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUICamera")]
public class tk2dUICamera : MonoBehaviour {

	// This is multiplied with the cameras layermask
	[SerializeField]
	private LayerMask raycastLayerMask = -1;

	public enum tk2dRaycastType {
		Physics3D,
		Physics2D
	}

	[SerializeField]
	private tk2dRaycastType raycastType = tk2dRaycastType.Physics3D;

	public tk2dRaycastType RaycastType {
		get {
			return raycastType;
		}
	}

	// This is used for backwards compatiblity only
	public void AssignRaycastLayerMask( LayerMask mask ) {
		raycastLayerMask = mask;
	}

	// The actual layermask, i.e. allowedMasks & layerMask
	public LayerMask FilteredMask {
		get {
			return raycastLayerMask & GetComponent<Camera>().cullingMask;
		}
	}

	public Camera HostCamera {
		get {
			return GetComponent<Camera>();
		}
	}

	void OnEnable() {
		if (GetComponent<Camera>() == null) {
			Debug.LogError("tk2dUICamera should only be attached to a camera.");
			enabled = false;
			return;
		}

		if (!GetComponent<Camera>().orthographic && raycastType == tk2dRaycastType.Physics2D) {
			Debug.LogError("tk2dUICamera - Physics2D raycast only works with orthographic cameras.");
			enabled = false;
			return;
		}

		tk2dUIManager.RegisterCamera( this );
	}

	void OnDisable() {
		tk2dUIManager.UnregisterCamera( this );
	}
}

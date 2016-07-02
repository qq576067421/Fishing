using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dClippedSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Sprite implementation that clips the sprite using normalized clip coordinates.
/// </summary>
public class tk2dClippedSprite : tk2dBaseSprite
{
	Mesh mesh;
	Vector2[] meshUvs;
	Vector3[] meshVertices;
	Color32[] meshColors;
	Vector3[] meshNormals = null;
	Vector4[] meshTangents = null;
	int[] meshIndices;
	
	public Vector2 _clipBottomLeft = new Vector2(0, 0);
	public Vector2 _clipTopRight = new Vector2(1, 1);

	// Temp cached variables
	Rect _clipRect = new Rect(0, 0, 0, 0);

	/// <summary>
	/// Sets the clip rectangle
	/// 0, 0, 1, 1 = display the entire sprite
	/// </summary>
	public Rect ClipRect {
		get {
			_clipRect.Set( _clipBottomLeft.x, _clipBottomLeft.y, _clipTopRight.x - _clipBottomLeft.x, _clipTopRight.y - _clipBottomLeft.y );
			return _clipRect;
		}
		set {
			Vector2 v = new Vector2( value.x, value.y );
			clipBottomLeft = v;
			v.x += value.width;
			v.y += value.height;
			clipTopRight = v;
		}
	}
	
	
	/// <summary>
	/// Sets the bottom left clip area.
	/// 0, 0 = display full sprite
	/// </summary>
	public Vector2 clipBottomLeft
	{
		get { return _clipBottomLeft; }
		set 
		{ 
			if (value != _clipBottomLeft) 
			{
				_clipBottomLeft = new Vector2(value.x, value.y);
				Build();
				UpdateCollider();
			}
		}
	}

	/// <summary>
	/// Sets the top right clip area
	/// 1, 1 = display full sprite
	/// </summary>
	public Vector2 clipTopRight
	{
		get { return _clipTopRight; }
		set 
		{ 
			if (value != _clipTopRight) 
			{
				_clipTopRight = new Vector2(value.x, value.y);
				Build();
				UpdateCollider();
			}
		}
	}
	
	[SerializeField]
	protected bool _createBoxCollider = false;

	/// <summary>
	/// Create a trimmed box collider for this sprite
	/// </summary>
	public bool CreateBoxCollider {
		get { return _createBoxCollider; }
		set {
			if (_createBoxCollider != value) {
				_createBoxCollider = value;
				UpdateCollider();
			}
		}
	}
	
	new void Awake()
	{
		base.Awake();
		
		// Create mesh, independently to everything else
		mesh = new Mesh();
#if !UNITY_3_5
		mesh.MarkDynamic();
#endif
		mesh.hideFlags = HideFlags.DontSave;
		GetComponent<MeshFilter>().mesh = mesh;

		// This will not be set when instantiating in code
		// In that case, Build will need to be called
		if (Collection)
		{
			// reset spriteId if outside bounds
			// this is when the sprite collection data is corrupt
			if (_spriteId < 0 || _spriteId >= Collection.Count)
				_spriteId = 0;
			
			Build();
		}
	}
	
	protected void OnDestroy()
	{
		if (mesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(mesh);
#else
			Destroy(mesh);
#endif
		}
	}

	new protected void SetColors(Color32[] dest)
	{
		if (CurrentSprite.positions.Length == 4)
		{
			tk2dSpriteGeomGen.SetSpriteColors (dest, 0, 4, _color, collectionInst.premultipliedAlpha);
		}
	}	
	
	// Calculated center and extents
	Vector3 boundsCenter = Vector3.zero, boundsExtents = Vector3.zero;

	protected void SetGeometry(Vector3[] vertices, Vector2[] uvs)
	{
		var sprite = CurrentSprite;

		float colliderOffsetZ = ( boxCollider != null ) ? ( boxCollider.center.z ) : 0.0f;
		float colliderExtentZ = ( boxCollider != null ) ? ( boxCollider.size.z * 0.5f ) : 0.5f;
		tk2dSpriteGeomGen.SetClippedSpriteGeom( meshVertices, meshUvs, 0, out boundsCenter, out boundsExtents, sprite, _scale, _clipBottomLeft, _clipTopRight, colliderOffsetZ, colliderExtentZ );

		if (meshNormals.Length > 0 || meshTangents.Length > 0) {
			tk2dSpriteGeomGen.SetSpriteVertexNormals(meshVertices, meshVertices[0], meshVertices[3], sprite.normals, sprite.tangents, meshNormals, meshTangents);
		}
		
		// Only do this when there are exactly 4 polys to a sprite (i.e. the sprite isn't diced, and isnt a more complex mesh)
		if (sprite.positions.Length != 4 || sprite.complexGeometry)
		{
			// Only supports normal sprites
			for (int i = 0; i < vertices.Length; ++i)
				vertices[i] = Vector3.zero;
		}
	}

	public override void Build()
	{
		var spriteDef = CurrentSprite;

		meshUvs = new Vector2[4];
		meshVertices = new Vector3[4];
		meshColors = new Color32[4];
		meshNormals = new Vector3[0];
		meshTangents = new Vector4[0];
		if (spriteDef.normals != null && spriteDef.normals.Length > 0) {
			meshNormals = new Vector3[4];
		}
		if (spriteDef.tangents != null && spriteDef.tangents.Length > 0) {
			meshTangents = new Vector4[4];
		}
		
		SetGeometry(meshVertices, meshUvs);
		SetColors(meshColors);
		
		if (mesh == null)
		{
			mesh = new Mesh();
#if !UNITY_3_5
			mesh.MarkDynamic();
#endif
			mesh.hideFlags = HideFlags.DontSave;
		}
		else
		{
			mesh.Clear();
		}
		
		mesh.vertices = meshVertices;
		mesh.colors32 = meshColors;
		mesh.uv = meshUvs;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;

		int[] indices = new int[6];
		tk2dSpriteGeomGen.SetClippedSpriteIndices(indices, 0, 0, CurrentSprite);
		mesh.triangles = indices;

		mesh.RecalculateBounds();
		mesh.bounds = AdjustedMeshBounds( mesh.bounds, renderLayer );

		GetComponent<MeshFilter>().mesh = mesh;
		
		UpdateCollider();
		UpdateMaterial();
	}
	
	protected override void UpdateGeometry() { UpdateGeometryImpl(); }
	protected override void UpdateColors() { UpdateColorsImpl(); }
	protected override void UpdateVertices() { UpdateGeometryImpl(); }
	
	
	protected void UpdateColorsImpl()
	{
		if (meshColors == null || meshColors.Length == 0) {
			Build();
		}
		else {
			SetColors(meshColors);
			mesh.colors32 = meshColors;
		}
	}

	protected void UpdateGeometryImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (mesh == null)
			return;
#endif
		if (meshVertices == null || meshVertices.Length == 0) {
			Build();
		}
		else {
			SetGeometry(meshVertices, meshUvs);
			mesh.vertices = meshVertices;
			mesh.uv = meshUvs;
			mesh.normals = meshNormals;
			mesh.tangents = meshTangents;
			mesh.RecalculateBounds();
			mesh.bounds = AdjustedMeshBounds( mesh.bounds, renderLayer );
		}
	}

#region Collider
	protected override void UpdateCollider()
	{
		if (CreateBoxCollider) {
			if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D) {
				if (boxCollider != null) {
					boxCollider.size = 2 * boundsExtents;
					boxCollider.center = boundsCenter;
				}
			}
			else if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D) {
	#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
				if (boxCollider2D != null) {
					boxCollider2D.size = 2 * boundsExtents;

#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9)
					boxCollider2D.center = boundsCenter;
#else
					boxCollider2D.offset = boundsCenter;
#endif
				}
	#endif
			}
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (mesh != null) {
			Bounds b = mesh.bounds;
			Gizmos.color = Color.clear;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(b.center, b.extents * 2);
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}
	}
#endif

	protected override void CreateCollider() {
		UpdateCollider();
	}

#if UNITY_EDITOR
	public override void EditMode__CreateCollider() {
		if (CreateBoxCollider) {
			base.CreateSimpleBoxCollider();
		}
		UpdateCollider();
	}
#endif
#endregion	

	protected override void UpdateMaterial()
	{
		Renderer renderer = GetComponent<Renderer>();

		if (renderer.sharedMaterial != collectionInst.spriteDefinitions[spriteId].materialInst)
			renderer.material = collectionInst.spriteDefinitions[spriteId].materialInst;
	}
	
	protected override int GetCurrentVertexCount()
	{
#if UNITY_EDITOR
		if (meshVertices == null)
			return 0;
#endif
		return 4;
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax) { // Identical to tk2dSprite.ReshapeBounds
		float minSizeClampTexelScale = 0.1f; // Can't shrink sprite smaller than this many texels
		// Irrespective of transform
		var sprite = CurrentSprite;
		Vector3 oldAbsScale = new Vector3(Mathf.Abs(_scale.x), Mathf.Abs(_scale.y), Mathf.Abs(_scale.z));
		Vector3 oldMin = Vector3.Scale(sprite.untrimmedBoundsData[0], _scale) - 0.5f * Vector3.Scale(sprite.untrimmedBoundsData[1], oldAbsScale);
		Vector3 oldSize = Vector3.Scale(sprite.untrimmedBoundsData[1], oldAbsScale);
		Vector3 newAbsScale = oldSize + dMax - dMin;
		newAbsScale.x /= sprite.untrimmedBoundsData[1].x;
		newAbsScale.y /= sprite.untrimmedBoundsData[1].y;
		// Clamp the minimum size to avoid having the pivot move when we scale from near-zero
		if (sprite.untrimmedBoundsData[1].x * newAbsScale.x < sprite.texelSize.x * minSizeClampTexelScale && newAbsScale.x < oldAbsScale.x) {
			dMin.x = 0;
			newAbsScale.x = oldAbsScale.x;
		}
		if (sprite.untrimmedBoundsData[1].y * newAbsScale.y < sprite.texelSize.y * minSizeClampTexelScale && newAbsScale.y < oldAbsScale.y) {
			dMin.y = 0;
			newAbsScale.y = oldAbsScale.y;
		}
		// Add our wanted local dMin offset, while negating the positional offset caused by scaling
		Vector2 scaleFactor = new Vector3(Mathf.Approximately(oldAbsScale.x, 0) ? 0 : (newAbsScale.x / oldAbsScale.x),
			Mathf.Approximately(oldAbsScale.y, 0) ? 0 : (newAbsScale.y / oldAbsScale.y));
		Vector3 scaledMin = new Vector3(oldMin.x * scaleFactor.x, oldMin.y * scaleFactor.y);
		Vector3 offset = dMin + oldMin - scaledMin;
		offset.z = 0;
		transform.position = transform.TransformPoint(offset);
		scale = new Vector3(_scale.x * scaleFactor.x, _scale.y * scaleFactor.y, _scale.z);
	}
}

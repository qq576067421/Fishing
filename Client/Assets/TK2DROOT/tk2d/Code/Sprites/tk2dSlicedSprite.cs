using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSlicedSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Sprite implementation that implements 9-slice scaling.
/// </summary>
public class tk2dSlicedSprite : tk2dBaseSprite
{
	Mesh mesh;
	Vector2[] meshUvs;
	Vector3[] meshVertices;
	Color32[] meshColors;
	Vector3[] meshNormals = null;
	Vector4[] meshTangents = null;
	int[] meshIndices;
	
	[SerializeField]
	Vector2 _dimensions = new Vector2(50.0f, 50.0f);
	[SerializeField]
	Anchor _anchor = Anchor.LowerLeft;
	[SerializeField]
	bool _borderOnly = false;

	[SerializeField]
	bool legacyMode = false; // purely for fixup in 2D Toolkit 2.0
	
	/// <summary>
	/// Gets or sets border only. When true, the quad in the middle of the
	/// sliced sprite is omitted, thus only drawing a border and saving fillrate
	/// </summary>
	public bool BorderOnly
	{ 
		get { return _borderOnly; } 
		set
		{
			if (value != _borderOnly)
			{
				_borderOnly = value;
				UpdateIndices();
			}
		}
	}


	/// <summary>
	/// Gets or sets the dimensions.
	/// </summary>
	/// <value>
	/// Use this to change the dimensions of the sliced sprite in pixel units
	/// </value>
	public Vector2 dimensions
	{ 
		get { return _dimensions; } 
		set
		{
			if (value != _dimensions)
			{
				_dimensions = value;
				UpdateVertices();
				UpdateCollider();
			}
		}
	}
	
	/// <summary>
	/// The anchor position for this sliced sprite
	/// </summary>
	public Anchor anchor
	{
		get { return _anchor; }
		set
		{
			if (value != _anchor)
			{
				_anchor = value;
				UpdateVertices();
				UpdateCollider();
			}
		}
	}
	
	/// <summary>
	/// Top border in sprite fraction (0 - Top, 1 - Bottom)
	/// </summary>
	public float borderTop = 0.2f;
	/// <summary>
	/// Bottom border in sprite fraction (0 - Bottom, 1 - Top)
	/// </summary>
	public float borderBottom = 0.2f;
	/// <summary>
	/// Left border in sprite fraction (0 - Left, 1 - Right)
	/// </summary>
	public float borderLeft = 0.2f;
	/// <summary>
	/// Right border in sprite fraction (1 - Right, 0 - Left)
	/// </summary>
	public float borderRight = 0.2f;

	public void SetBorder(float left, float bottom, float right, float top) {
		if (borderLeft != left || borderBottom != bottom || borderRight != right || borderTop != top) {
			borderLeft = left;
			borderBottom = bottom;
			borderRight = right;
			borderTop = top;

			UpdateVertices();
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

#if UNITY_EDITOR
	void OnValidate()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter != null)
		{
			meshFilter.sharedMesh = mesh;
		}
	}
#endif

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
	
		// Cache box collider		
		if (boxCollider == null) {
			boxCollider = GetComponent<BoxCollider>();
		}
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (boxCollider2D == null) {
			boxCollider2D = GetComponent<BoxCollider2D>();
		}
#endif

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
		tk2dSpriteGeomGen.SetSpriteColors (dest, 0, 16, _color, collectionInst.premultipliedAlpha);
	}
	
	// Calculated center and extents
	Vector3 boundsCenter = Vector3.zero, boundsExtents = Vector3.zero;
	
	protected void SetGeometry(Vector3[] vertices, Vector2[] uvs)
	{
		var sprite = CurrentSprite;

		float colliderOffsetZ = ( boxCollider != null ) ? ( boxCollider.center.z ) : 0.0f;
		float colliderExtentZ = ( boxCollider != null ) ? ( boxCollider.size.z * 0.5f ) : 0.5f;
		tk2dSpriteGeomGen.SetSlicedSpriteGeom(meshVertices, meshUvs, 0, out boundsCenter, out boundsExtents, sprite, _scale, dimensions, new Vector2(borderLeft, borderBottom), new Vector2(borderRight, borderTop), anchor, colliderOffsetZ, colliderExtentZ);

		if (meshNormals.Length > 0 || meshTangents.Length > 0) {
			tk2dSpriteGeomGen.SetSpriteVertexNormals(meshVertices, meshVertices[0], meshVertices[15], sprite.normals, sprite.tangents, meshNormals, meshTangents);
		}

		if (sprite.positions.Length != 4 || sprite.complexGeometry)
		{
			for (int i = 0; i < vertices.Length; ++i)
				vertices[i] = Vector3.zero;
		}
	}
	
	void SetIndices()
	{
		int n = _borderOnly ? (8 * 6) : (9 * 6);
		meshIndices = new int[n];
		tk2dSpriteGeomGen.SetSlicedSpriteIndices(meshIndices, 0, 0, CurrentSprite, _borderOnly);
	}

	// returns true if value is close enough to compValue, by within 1% of scale
	bool NearEnough(float value, float compValue, float scale) {
		float diff = Mathf.Abs(value - compValue);
		return Mathf.Abs(diff / scale) < 0.01f;
	}

	void PermanentUpgradeLegacyMode() {
		tk2dSpriteDefinition def = CurrentSprite;

		// Guess anchor
		float x = def.untrimmedBoundsData[0].x;
		float y = def.untrimmedBoundsData[0].y;
		float w = def.untrimmedBoundsData[1].x;
		float h = def.untrimmedBoundsData[1].y;
		if 		(NearEnough(x,    0, w) && NearEnough(y, -h/2, h))	_anchor = tk2dBaseSprite.Anchor.UpperCenter;
		else if (NearEnough(x,    0, w) && NearEnough(y,    0, h)) 	_anchor = tk2dBaseSprite.Anchor.MiddleCenter;
		else if (NearEnough(x,    0, w) && NearEnough(y,  h/2, h)) 	_anchor = tk2dBaseSprite.Anchor.LowerCenter;
		else if (NearEnough(x, -w/2, w) && NearEnough(y, -h/2, h)) 	_anchor = tk2dBaseSprite.Anchor.UpperRight;
		else if (NearEnough(x, -w/2, w) && NearEnough(y,    0, h)) 	_anchor = tk2dBaseSprite.Anchor.MiddleRight;
		else if (NearEnough(x, -w/2, w) && NearEnough(y,  h/2, h)) 	_anchor = tk2dBaseSprite.Anchor.LowerRight;
		else if (NearEnough(x,  w/2, w) && NearEnough(y, -h/2, h)) 	_anchor = tk2dBaseSprite.Anchor.UpperLeft;
		else if (NearEnough(x,  w/2, w) && NearEnough(y,    0, h)) 	_anchor = tk2dBaseSprite.Anchor.MiddleLeft;
		else if (NearEnough(x,  w/2, w) && NearEnough(y,  h/2, h)) 	_anchor = tk2dBaseSprite.Anchor.LowerLeft;
		else {
			Debug.LogError("tk2dSlicedSprite (" + name + ") error - Unable to determine anchor upgrading from legacy mode. Please fix this manually.");
			_anchor = tk2dBaseSprite.Anchor.MiddleCenter;
		}

		// Calculate dimensions in pixel units
		float pixelWidth = w / def.texelSize.x;
		float pixelHeight = h / def.texelSize.y;
		_dimensions.x = _scale.x * pixelWidth;
		_dimensions.y = _scale.y * pixelHeight;

		_scale.Set( 1, 1, 1 );
		legacyMode = false;
	}
	
	public override void Build()
	{
		// Best guess upgrade
		if (legacyMode == true) {
			PermanentUpgradeLegacyMode();
		}

		var spriteDef = CurrentSprite;

		meshUvs = new Vector2[16];
		meshVertices = new Vector3[16];
		meshColors = new Color32[16];
		meshNormals = new Vector3[0];
		meshTangents = new Vector4[0];
		if (spriteDef.normals != null && spriteDef.normals.Length > 0) {
			meshNormals = new Vector3[16];
		}
		if (spriteDef.tangents != null && spriteDef.tangents.Length > 0) {
			meshTangents = new Vector4[16];
		}
		SetIndices();
		
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
		mesh.triangles = meshIndices;
		mesh.RecalculateBounds();
		mesh.bounds = AdjustedMeshBounds( mesh.bounds, renderLayer );
		
		GetComponent<MeshFilter>().mesh = mesh;
		
		UpdateCollider();
		UpdateMaterial();
	}
	
	protected override void UpdateGeometry() { UpdateGeometryImpl(); }
	protected override void UpdateColors() { UpdateColorsImpl(); }
	protected override void UpdateVertices() { UpdateGeometryImpl(); }
	void UpdateIndices() {
		if (mesh != null) {
			SetIndices();
			mesh.triangles = meshIndices;
		}
	}
	
	protected void UpdateColorsImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (meshColors == null || meshColors.Length == 0)
			return;
#endif
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
			
			UpdateCollider();
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
		return 16;
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax) {
		float minSizeClampTexelScale = 0.1f; // Can't shrink sprite smaller than this many texels
		// Irrespective of transform
		var sprite = CurrentSprite;
		Vector2 boundsSize = new Vector2(_dimensions.x * sprite.texelSize.x, _dimensions.y * sprite.texelSize.y);
		Vector3 oldSize = new Vector3(boundsSize.x * _scale.x, boundsSize.y * _scale.y);
		Vector3 oldMin = Vector3.zero;
		switch (_anchor) {
			case Anchor.LowerLeft: oldMin.Set(0,0,0); break;
			case Anchor.LowerCenter: oldMin.Set(0.5f,0,0); break;
			case Anchor.LowerRight: oldMin.Set(1,0,0); break;
			case Anchor.MiddleLeft: oldMin.Set(0,0.5f,0); break;
			case Anchor.MiddleCenter: oldMin.Set(0.5f,0.5f,0); break;
			case Anchor.MiddleRight: oldMin.Set(1,0.5f,0); break;
			case Anchor.UpperLeft: oldMin.Set(0,1,0); break;
			case Anchor.UpperCenter: oldMin.Set(0.5f,1,0); break;
			case Anchor.UpperRight: oldMin.Set(1,1,0); break;
		}
		oldMin = Vector3.Scale(oldMin, oldSize) * -1;
		Vector3 newScale = oldSize + dMax - dMin;
		newScale.x /= boundsSize.x;
		newScale.y /= boundsSize.y;
		// Clamp the minimum size to avoid having the pivot move when we scale from near-zero
		if (Mathf.Abs(boundsSize.x * newScale.x) < sprite.texelSize.x * minSizeClampTexelScale && Mathf.Abs(newScale.x) < Mathf.Abs(_scale.x)) {
			dMin.x = 0;
			newScale.x = _scale.x;
		}
		if (Mathf.Abs(boundsSize.y * newScale.y) < sprite.texelSize.y * minSizeClampTexelScale && Mathf.Abs(newScale.y) < Mathf.Abs(_scale.y)) {
			dMin.y = 0;
			newScale.y = _scale.y;
		}
		// Add our wanted local dMin offset, while negating the positional offset caused by scaling
		Vector2 scaleFactor = new Vector3(Mathf.Approximately(_scale.x, 0) ? 0 : (newScale.x / _scale.x),
			Mathf.Approximately(_scale.y, 0) ? 0 : (newScale.y / _scale.y));
		Vector3 scaledMin = new Vector3(oldMin.x * scaleFactor.x, oldMin.y * scaleFactor.y);
		Vector3 offset = dMin + oldMin - scaledMin;
		offset.z = 0;
		transform.position = transform.TransformPoint(offset);
		dimensions = new Vector2(_dimensions.x * scaleFactor.x, _dimensions.y * scaleFactor.y);
	}
}

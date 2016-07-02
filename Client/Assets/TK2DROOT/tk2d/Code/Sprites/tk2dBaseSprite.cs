using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("2D Toolkit/Backend/tk2dBaseSprite")]
/// <summary>
/// Sprite base class. Performs target agnostic functionality and manages state parameters.
/// </summary>
public abstract class tk2dBaseSprite : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild
{
	/// <summary>
	/// Anchor.
	/// NOTE: The order in this enum is deliberate, to initialize at LowerLeft for backwards compatibility.
	/// This is also the reason it is local here. Other Anchor enums are NOT compatbile. Do not cast.
	/// </summary>
    public enum Anchor
    {
		/// <summary>Lower left</summary>
		LowerLeft,
		/// <summary>Lower center</summary>
		LowerCenter,
		/// <summary>Lower right</summary>
		LowerRight,
		/// <summary>Middle left</summary>
		MiddleLeft,
		/// <summary>Middle center</summary>
		MiddleCenter,
		/// <summary>Middle right</summary>
		MiddleRight,
		/// <summary>Upper left</summary>
		UpperLeft,
		/// <summary>Upper center</summary>
		UpperCenter,
		/// <summary>Upper right</summary>
		UpperRight,
    }

	/// <summary>
	/// This is now private. You should use <see cref="tk2dBaseSprite.Collection">Collection</see> if you wish to read this value.
	/// Use <see cref="tk2dBaseSprite.SetSprite">SetSprite</see> when you need to switch sprite collection.
	/// </summary>
	[SerializeField]
    private tk2dSpriteCollectionData collection;

	/// <summary>
	/// Deprecation warning: the set accessor will be removed in a future version.
	/// Use <see cref="tk2dBaseSprite.SetSprite">SetSprite</see> when you need to switch sprite collection.
	/// </summary>
	public tk2dSpriteCollectionData Collection 
	{ 
		get { return collection; } 
		set { collection = value; collectionInst = collection.inst; } 
	}

    // This is the active instance of the sprite collection
    protected tk2dSpriteCollectionData collectionInst;
	
	[SerializeField] protected Color _color = Color.white;
	[SerializeField] protected Vector3 _scale = new Vector3(1.0f, 1.0f, 1.0f);
	[SerializeField] protected int _spriteId = 0;

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
	public BoxCollider2D boxCollider2D = null;
	public List<PolygonCollider2D> polygonCollider2D = new List<PolygonCollider2D>(1);
	public List<EdgeCollider2D> edgeCollider2D = new List<EdgeCollider2D>(1);
#endif
	
	/// <summary>
	/// Internal cached version of the box collider created for this sprite, if present.
	/// </summary>
	public BoxCollider boxCollider = null;
	/// <summary>
	/// Internal cached version of the mesh collider created for this sprite, if present.
	/// </summary>
	public MeshCollider meshCollider = null;
	public Vector3[] meshColliderPositions = null;
	public Mesh meshColliderMesh = null;
	
	/// <summary>
	/// This event is called whenever a sprite is changed. 
	/// A sprite is considered to be changed when the sprite itself
	/// is changed, or the scale applied to the sprite is changed.
	/// </summary>
	public event System.Action<tk2dBaseSprite> SpriteChanged;

	// This is unfortunate, but required due to the unpredictable script execution order in Unity.
	// The only problem happens in Awake(), where if another class is Awaken before this one, and tries to
	// modify this instance before it is initialized, very bad things could happen.
	// Awake also never gets called on an object which is inactive.
	void InitInstance()
	{
		if (collectionInst == null && collection != null)
			collectionInst = collection.inst;
	}

	/// <summary>
	/// Gets or sets the color.
	/// </summary>
	/// <value>
	/// Please note the range for a Unity Color is 0..1 and not 0..255.
	/// </value>
	public Color color 
	{ 
		get { return _color; } 
		set 
		{
			if (value != _color)
			{
				_color = value;
				InitInstance();
				UpdateColors();
			}
		} 
	}
	
	/// <summary>
	/// Gets or sets the scale.
	/// </summary>
	/// <value>
	/// Use the sprite scale method as opposed to transform.localScale to avoid breaking dynamic batching.
	/// </value>
	public Vector3 scale 
	{ 
		get { return _scale; } 
		set
		{
			if (value != _scale)
			{
				_scale = value;
				InitInstance();
				UpdateVertices();
#if UNITY_EDITOR
				EditMode__CreateCollider();
#else
				UpdateCollider();
#endif
				if (SpriteChanged != null) {
					SpriteChanged( this );
				}
			}
		}
	}
	
	Renderer _cachedRenderer = null;
	Renderer CachedRenderer {
		get {
			if (_cachedRenderer == null) {
				_cachedRenderer = GetComponent<Renderer>();
			}
			return _cachedRenderer;
		}
	}

	[SerializeField] protected int renderLayer = 0;
	/// <summary>
	/// Gets or sets the sorting order
	/// The sorting order lets you override draw order for sprites which are at the same z position
	/// It is similar to offsetting in z - the sprite stays at the original position
	/// This corresponds to the renderer.sortingOrder property in Unity 4.3
	/// </summary>
	public int SortingOrder {
		get { 
#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			return renderLayer; 
#else
			return CachedRenderer.sortingOrder;
#endif
		}
		set { 
#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			if (renderLayer != value) { 
				renderLayer = value; InitInstance(); UpdateVertices(); 
			} 
#else
			if (CachedRenderer.sortingOrder != value) {
				renderLayer = value; // for awake
				CachedRenderer.sortingOrder = value;
#if UNITY_EDITOR
				tk2dUtil.SetDirty(CachedRenderer);
#endif
			}
#endif
		}
	}

	/// <summary>
	/// Flips the sprite horizontally. Set FlipX to true to flip it horizontally.
	/// Note: The sprite itself may be flipped by the hierarchy above it or localScale
	/// These functions do not consider those cases.
	/// </summary>
	public bool FlipX {
		get { return _scale.x < 0; }
		set { scale = new Vector3( Mathf.Abs(_scale.x) * (value?-1:1), _scale.y, _scale.z ); }
	}
	
	/// <summary>
	/// Flips the sprite vertically. Set FlipY to true to flip it vertically.
	/// Note: The sprite itself may be flipped by the hierarchy above it or localScale
	/// These functions do not consider those cases.
	/// </summary>
	public bool FlipY {
		get { return _scale.y < 0; }
		set { scale = new Vector3( _scale.x, Mathf.Abs(_scale.y) * (value?-1:1), _scale.z ); }
	}
	
	/// <summary>
	/// Gets or sets the sprite identifier.
	/// </summary>
	/// <value>
	/// The spriteId is a unique number identifying each sprite.
	/// Use <see cref="tk2dBaseSprite.GetSpriteIdByName">GetSpriteIdByName</see> to resolve an identifier from the current sprite collection.
	/// </value>
	public int spriteId 
	{ 
		get { return _spriteId; } 
		set 
		{
			if (value != _spriteId)
			{
				InitInstance();
				value = Mathf.Clamp(value, 0, collectionInst.spriteDefinitions.Length - 1);
				if (_spriteId < 0 || _spriteId >= collectionInst.spriteDefinitions.Length ||
					GetCurrentVertexCount() != collectionInst.spriteDefinitions[value].positions.Length ||
					collectionInst.spriteDefinitions[_spriteId].complexGeometry != collectionInst.spriteDefinitions[value].complexGeometry)
				{
					_spriteId = value;
					UpdateGeometry();
				}
				else
				{
					_spriteId = value;
					UpdateVertices();
				}
				UpdateMaterial();
				UpdateCollider();

				if (SpriteChanged != null) {
					SpriteChanged( this );
				}
			}
		} 
	}

	/// <summary>
	/// Sets the sprite by identifier.
	/// </summary>
	public void SetSprite(int newSpriteId) {
		this.spriteId = newSpriteId;
	}

	/// <summary>
	/// Sets the sprite by name. The sprite will be selected from the current collection.
	/// </summary>
	public bool SetSprite(string spriteName) {
		int spriteId = collection.GetSpriteIdByName(spriteName, -1);
		if (spriteId != -1) { 
			SetSprite(spriteId);
		}
		else {
			Debug.LogError("SetSprite - Sprite not found in collection: " + spriteName);
		}
		return spriteId != -1;
	}
	
	/// <summary>
	/// Sets sprite by identifier from the new collection.
	/// </summary>
	public void SetSprite(tk2dSpriteCollectionData newCollection, int newSpriteId) {
		bool switchedCollection = false;
		if (Collection != newCollection) {
			collection = newCollection;
			collectionInst = collection.inst;
			_spriteId = -1; // force an update, but only when the collection has changed
			switchedCollection = true;
		}
		
		spriteId = newSpriteId;
		
		if (switchedCollection) {
			UpdateMaterial();
		}
	}

	/// <summary>
	/// Sets sprite by name from the new collection.
	/// </summary>
	public bool SetSprite(tk2dSpriteCollectionData newCollection, string spriteName) {
		int spriteId = newCollection.GetSpriteIdByName(spriteName, -1);
		if (spriteId != -1) { 
			SetSprite(newCollection, spriteId);
		}
		else {
			Debug.LogError("SetSprite - Sprite not found in collection: " + spriteName);
		}
		return spriteId != -1;
	}

	/// <summary>
	/// Makes the sprite pixel perfect to the active camera.
	/// Automatically detects <see cref="tk2dCamera"/> if present
	/// Otherwise uses Camera.main
	/// </summary>
	public void MakePixelPerfect()
	{
		float s = 1.0f;
		tk2dCamera cam = tk2dCamera.CameraForLayer(gameObject.layer);
		if (cam != null)
		{
			if (Collection.version < 2)
			{
				Debug.LogError("Need to rebuild sprite collection.");
			}

			float zdist = (transform.position.z - cam.transform.position.z);
			float spriteSize = (Collection.invOrthoSize * Collection.halfTargetHeight);
			s = cam.GetSizeAtDistance(zdist) * spriteSize;
		}
		else if (Camera.main)
		{
			if (Camera.main.orthographic)
			{
				s = Camera.main.orthographicSize;
			}
			else
			{
				float zdist = (transform.position.z - Camera.main.transform.position.z);
				s = tk2dPixelPerfectHelper.CalculateScaleForPerspectiveCamera(Camera.main.fieldOfView, zdist);
			}
			s *= Collection.invOrthoSize;
		}
		else
		{
			Debug.LogError("Main camera not found.");
		}
		
		
		scale = new Vector3(Mathf.Sign(scale.x) * s, Mathf.Sign(scale.y) * s, Mathf.Sign(scale.z) * s);
	}	
		
	
	protected abstract void UpdateMaterial(); // update material when switching spritecollection
	protected abstract void UpdateColors(); // reupload color data only
	protected abstract void UpdateVertices(); // reupload vertex data only
	protected abstract void UpdateGeometry(); // update full geometry (including indices)
	protected abstract int  GetCurrentVertexCount(); // return current vertex count
	
	/// <summary>
	/// Rebuilds the mesh data for this sprite. Not usually necessary to call this, unless some internal states are modified.
	/// </summary>
	public abstract void Build();
	
	/// <summary>
	/// Resolves a sprite name and returns a unique id for the sprite.
	/// Convenience alias of <see cref="tk2dSpriteCollectionData.GetSpriteIdByName"/>
	/// </summary>
	/// <returns>
	/// Unique Sprite Id.
	/// </returns>
	/// <param name='name'>Case sensitive sprite name, as defined in the sprite collection. This is usually the source filename excluding the extension</param>
	public int GetSpriteIdByName(string name)
	{
		InitInstance();
		return collectionInst.GetSpriteIdByName(name);
	}
	
	/// <summary>
	/// Adds a tk2dBaseSprite derived class as a component to the gameObject passed in, setting up necessary parameters
	/// and building geometry.
	/// </summary>
	public static T AddComponent<T>(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId) where T : tk2dBaseSprite
	{
		T sprite = go.AddComponent<T>();
		sprite._spriteId = -1;
		sprite.SetSprite(spriteCollection, spriteId);
		sprite.Build();
		return sprite;
	}
	
	/// <summary>
	/// Adds a tk2dBaseSprite derived class as a component to the gameObject passed in, setting up necessary parameters
	/// and building geometry. Shorthand using sprite name
	/// </summary>
	public static T AddComponent<T>(GameObject go, tk2dSpriteCollectionData spriteCollection, string spriteName) where T : tk2dBaseSprite
	{
		int spriteId = spriteCollection.GetSpriteIdByName(spriteName, -1);
		if (spriteId == -1) {
			Debug.LogError( string.Format("Unable to find sprite named {0} in sprite collection {1}", spriteName, spriteCollection.spriteCollectionName) );
			return null;
		}
		else {
			return AddComponent<T>(go, spriteCollection, spriteId);			
		}
	}
	
	protected int GetNumVertices()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[spriteId].positions.Length;
	}
	
	protected int GetNumIndices()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[spriteId].indices.Length;
	}
	
	protected void SetPositions(Vector3[] positions, Vector3[] normals, Vector4[] tangents)	
	{
		var sprite = collectionInst.spriteDefinitions[spriteId];
		int numVertices = GetNumVertices();
		for (int i = 0; i < numVertices; ++i)
		{
			positions[i].x = sprite.positions[i].x * _scale.x;
			positions[i].y = sprite.positions[i].y * _scale.y;
			positions[i].z = sprite.positions[i].z * _scale.z;
		}
		
		// The secondary test sprite.normals != null must have been performed prior to this function call
		int numNormals = sprite.normals.Length;
		if (normals.Length == numNormals)
		{
			for (int i = 0; i < numNormals; ++i)
			{
				normals[i] = sprite.normals[i];
			}
		}

		// The secondary test sprite.tangents != null must have been performed prior to this function call
		int numTangents = sprite.tangents.Length;
		if (tangents.Length == numTangents)
		{
			for (int i = 0; i < numTangents; ++i)
			{
				tangents[i] = sprite.tangents[i];
			}
		}
	}
	
	protected void SetColors(Color32[] dest)
	{
		Color c = _color;
        if (collectionInst.premultipliedAlpha) { c.r *= c.a; c.g *= c.a; c.b *= c.a; }
        Color32 c32 = c;

		int numVertices = GetNumVertices();
		for (int i = 0; i < numVertices; ++i)
			dest[i] = c32;
	}
	
	/// <summary>
	/// Gets the local space bounds of the sprite.
	/// </summary>
	/// <returns>
	/// Local space bounds
	/// </returns>
	public Bounds GetBounds()
	{
		InitInstance();
		var sprite = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(sprite.boundsData[0].x * _scale.x, sprite.boundsData[0].y * _scale.y, sprite.boundsData[0].z * _scale.z),
		                  new Vector3(sprite.boundsData[1].x * Mathf.Abs(_scale.x), sprite.boundsData[1].y * Mathf.Abs(_scale.y), sprite.boundsData[1].z * Mathf.Abs(_scale.z) ));
	}
	
	/// <summary>
	/// Gets untrimmed local space bounds of the sprite. This is the size of the sprite before 2D Toolkit trims away empty space in the sprite.
	/// Use this when you need to position sprites in a grid, etc, when the trimmed bounds is not sufficient.
	/// </summary>
	/// <returns>
	/// Local space untrimmed bounds
	/// </returns>
	public Bounds GetUntrimmedBounds()
	{
		InitInstance();
		var sprite = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(sprite.untrimmedBoundsData[0].x * _scale.x, sprite.untrimmedBoundsData[0].y * _scale.y, sprite.untrimmedBoundsData[0].z * _scale.z),
		                  new Vector3(sprite.untrimmedBoundsData[1].x * Mathf.Abs(_scale.x), sprite.untrimmedBoundsData[1].y * Mathf.Abs(_scale.y), sprite.untrimmedBoundsData[1].z * Mathf.Abs(_scale.z) ));
	}

	public static Bounds AdjustedMeshBounds(Bounds bounds, int renderLayer) {
		Vector3 center = bounds.center;
		center.z = -renderLayer * 0.01f;
		bounds.center = center;
		return bounds;
	}
	
	/// <summary>
	/// Gets the current sprite definition.
	/// </summary>
	/// <returns>
	/// <see cref="tk2dSpriteDefinition"/> for the currently active sprite.
	/// </returns>
	public tk2dSpriteDefinition GetCurrentSpriteDef()
	{
		InitInstance();
		return (collectionInst == null) ? null : collectionInst.spriteDefinitions[_spriteId];
	}

	/// <summary>
	/// Gets the current sprite definition.
	/// </summary>
	/// <returns>
	/// <see cref="tk2dSpriteDefinition"/> for the currently active sprite.
	/// </returns>
	public tk2dSpriteDefinition CurrentSprite {
		get {
			InitInstance();
			return (collectionInst == null) ? null : collectionInst.spriteDefinitions[_spriteId];
		}
	}

	/// <summary>
	/// Used for sprite resizing in Editor, and UILayout.
	/// </summary>
	public virtual void ReshapeBounds(Vector3 dMin, Vector3 dMax) {
		;
	}

	// Collider setup
	
	protected virtual bool NeedBoxCollider() { return false; }
	
	protected virtual void UpdateCollider()
	{
		tk2dSpriteDefinition sprite = collectionInst.spriteDefinitions[_spriteId];

		if (sprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D) {
			if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box && boxCollider == null)
			{
				// Has the user created a box collider?
				boxCollider = gameObject.GetComponent<BoxCollider>();
				
				if (boxCollider == null)
				{
					// create box collider at runtime. this won't get removed from the object
					boxCollider = gameObject.AddComponent<BoxCollider>();
				}
			}

			
			if (boxCollider != null)
			{
				if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box)
				{
					boxCollider.center = new Vector3(sprite.colliderVertices[0].x * _scale.x, sprite.colliderVertices[0].y * _scale.y, sprite.colliderVertices[0].z * _scale.z);
					boxCollider.size = new Vector3(2 * sprite.colliderVertices[1].x * _scale.x, 2 * sprite.colliderVertices[1].y * _scale.y, 2 * sprite.colliderVertices[1].z * _scale.z);
				}
				else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Unset)
				{
					// Don't do anything here, for backwards compatibility
				}
				else // in all cases, if the collider doesn't match is requested, null it out
				{
					if (boxCollider != null)
					{
						// move the box far far away, boxes with zero extents still collide
						boxCollider.center = new Vector3(0, 0, -100000.0f);
						boxCollider.size = Vector3.zero;
					}
				}
			}
		}
		else if (sprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D) {
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
				if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box)
				{
					if (boxCollider2D == null) {
						// Has the user created a box collider?
						boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
						if (boxCollider2D == null)
						{
							// create box collider at runtime. this won't get removed from the object
							boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
						}
					}

					// Turn off existing polygon colliders
					if (polygonCollider2D.Count > 0) {
						foreach (PolygonCollider2D polyCollider in polygonCollider2D) {
							if (polyCollider != null && polyCollider.enabled) {
								polyCollider.enabled = false;
							}
						}
					}
					// Turn off existing edge colliders
					if (edgeCollider2D.Count > 0) {
						foreach (EdgeCollider2D edgeCollider in edgeCollider2D) {
							if (edgeCollider != null && edgeCollider.enabled) {
								edgeCollider.enabled = false;
							}
						}
					}

					if (!boxCollider2D.enabled) {
						boxCollider2D.enabled = true;
					}
#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9)
					boxCollider2D.center = new Vector2(sprite.colliderVertices[0].x * _scale.x, sprite.colliderVertices[0].y * _scale.y);
#else
					boxCollider2D.offset = new Vector2(sprite.colliderVertices[0].x * _scale.x, sprite.colliderVertices[0].y * _scale.y);
#endif
					boxCollider2D.size = new Vector2(Mathf.Abs(2 * sprite.colliderVertices[1].x * _scale.x), Mathf.Abs(2 * sprite.colliderVertices[1].y * _scale.y));
				}
				else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
				{
					// Turn of existing box collider
					if (boxCollider2D != null && boxCollider2D.enabled) {
						boxCollider2D.enabled = false;
					}

					// Make sure none in our array are null (manually deleted, etc)
					// This doesn't handle the case where the user has deleted something manually from the polygonCOllider2D list
					int numPolyColliders = sprite.polygonCollider2D.Length;
					for (int i = 0; i < polygonCollider2D.Count; ++i) {
						if (polygonCollider2D[i] == null) {
							polygonCollider2D[i] = gameObject.AddComponent<PolygonCollider2D>();
						}
					}
					while (polygonCollider2D.Count < numPolyColliders) {
						polygonCollider2D.Add( gameObject.AddComponent<PolygonCollider2D>() );
					}
					for (int i = 0; i < numPolyColliders; ++i) {
						if (!polygonCollider2D[i].enabled) {
							polygonCollider2D[i].enabled = true;
						}
						if (_scale.x != 1 || _scale.y != 1) {
							Vector2[] sourcePoints = sprite.polygonCollider2D[i].points;
							Vector2[] scaledPoints = new Vector2[sourcePoints.Length];
							for (int j = 0; j < sourcePoints.Length; ++j) {
								scaledPoints[j] = Vector2.Scale( sourcePoints[j], _scale );
							}
							polygonCollider2D[i].points = scaledPoints;
						}
						else {
							polygonCollider2D[i].points = sprite.polygonCollider2D[i].points;
						}
					}
					for (int i = numPolyColliders; i < polygonCollider2D.Count; ++i) {
						if (polygonCollider2D[i].enabled) {
							polygonCollider2D[i].enabled = false;
						}
					}

					// Make sure none in our array are null (manually deleted, etc)
					// This doesn't handle the case where the user has deleted something manually from the polygonCOllider2D list
					int numEdgeColliders = sprite.edgeCollider2D.Length;
					for (int i = 0; i < edgeCollider2D.Count; ++i) {
						if (edgeCollider2D[i] == null) {
							edgeCollider2D[i] = gameObject.AddComponent<EdgeCollider2D>();
						}
					}
					while (edgeCollider2D.Count < numEdgeColliders) {
						edgeCollider2D.Add( gameObject.AddComponent<EdgeCollider2D>() );
					}
					for (int i = 0; i < numEdgeColliders; ++i) {
						if (!edgeCollider2D[i].enabled) {
							edgeCollider2D[i].enabled = true;
						}
						if (_scale.x != 1 || _scale.y != 1) {
							Vector2[] sourcePoints = sprite.edgeCollider2D[i].points;
							Vector2[] scaledPoints = new Vector2[sourcePoints.Length];
							for (int j = 0; j < sourcePoints.Length; ++j) {
								scaledPoints[j] = Vector2.Scale( sourcePoints[j], _scale );
							}
							edgeCollider2D[i].points = scaledPoints;
						}
						else {
							edgeCollider2D[i].points = sprite.edgeCollider2D[i].points;
						}
					}
					for (int i = numEdgeColliders; i < edgeCollider2D.Count; ++i) {
						if (edgeCollider2D[i].enabled) {
							edgeCollider2D[i].enabled = false;
						}
					}
				}
				else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.None) {
					// Turn of existing box collider
					if (boxCollider2D != null && boxCollider2D.enabled) {
						boxCollider2D.enabled = false;
					}
					// Turn off existing polygon colliders
					if (polygonCollider2D.Count > 0) {
						foreach (PolygonCollider2D polyCollider in polygonCollider2D) {
							if (polyCollider != null && polyCollider.enabled) {
								polyCollider.enabled = false;
							}
						}
					}
					// Turn off existing edge colliders
					if (edgeCollider2D.Count > 0) {
						foreach (EdgeCollider2D edgeCollider in edgeCollider2D) {
							if (edgeCollider != null && edgeCollider.enabled) {
								edgeCollider.enabled = false;
							}
						}
					}
				}
#endif			
		}
	}
	
	// This is separate to UpdateCollider, as UpdateCollider can only work with BoxColliders, and will NOT create colliders
	protected virtual void CreateCollider()
	{
		tk2dSpriteDefinition sprite = collectionInst.spriteDefinitions[_spriteId];
		if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Unset)
		{
			// do not attempt to create or modify anything if it is Unset
			return;
		}

		if (sprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D) {
			// User has created a collider
			if (GetComponent<Collider>() != null)
			{
				boxCollider = GetComponent<BoxCollider>();
				meshCollider = GetComponent<MeshCollider>();
			}
			
			if ((NeedBoxCollider() || sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box) && meshCollider == null)
			{
				if (boxCollider == null)
				{
					boxCollider = gameObject.AddComponent<BoxCollider>();
				}
			}
			else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Mesh && boxCollider == null)
			{
				// this should not be updated again (apart from scale changes in the editor, where we force regeneration of colliders)
				if (meshCollider == null)
					meshCollider = gameObject.AddComponent<MeshCollider>();
				if (meshColliderMesh == null)
					meshColliderMesh = new Mesh();
				
				meshColliderMesh.Clear();
				
				meshColliderPositions = new Vector3[sprite.colliderVertices.Length];
				for (int i = 0; i < meshColliderPositions.Length; ++i)
					meshColliderPositions[i] = new Vector3(sprite.colliderVertices[i].x * _scale.x, sprite.colliderVertices[i].y * _scale.y, sprite.colliderVertices[i].z * _scale.z);
				meshColliderMesh.vertices = meshColliderPositions;
				
				float s = _scale.x * _scale.y * _scale.z;
				
				meshColliderMesh.triangles = (s >= 0.0f)?sprite.colliderIndicesFwd:sprite.colliderIndicesBack;
				meshCollider.sharedMesh = meshColliderMesh;
				meshCollider.convex = sprite.colliderConvex;
#if (UNITY_3_5|| UNITY_4_0)
				meshCollider.smoothSphereCollisions = sprite.colliderSmoothSphereCollisions;
#endif

				// this is required so our mesh pivot is at the right point
				if (GetComponent<Rigidbody>()) GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
			}
			else if (sprite.colliderType != tk2dSpriteDefinition.ColliderType.None)
			{
				// This warning is not applicable in the editor
				if (Application.isPlaying)
				{
					Debug.LogError("Invalid mesh collider on sprite '" + name + "', please remove and try again.");
				}
			}
			
			UpdateCollider();
		}
		else if (sprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D) {
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			UpdateCollider();
#endif
		}
	}
	
#if UNITY_EDITOR
	public virtual void EditMode__CreateCollider()
	{
		// Revert to runtime behaviour when the game is running
		if (Application.isPlaying)
		{
			UpdateCollider();
			return;
		}
		
		tk2dSpriteDefinition sprite = collectionInst.spriteDefinitions[_spriteId];
		if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Unset)
			return;
		
		PhysicMaterial physicsMaterial = GetComponent<Collider>()?GetComponent<Collider>().sharedMaterial:null;
		bool isTrigger = GetComponent<Collider>()?GetComponent<Collider>().isTrigger:false;

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		PhysicsMaterial2D physicsMaterial2D = GetComponent<Collider2D>()?GetComponent<Collider2D>().sharedMaterial:null;
		if (GetComponent<Collider2D>() != null) {
			isTrigger = GetComponent<Collider2D>().isTrigger;
		}
#endif

		boxCollider = gameObject.GetComponent<BoxCollider>();
		meshCollider = gameObject.GetComponent<MeshCollider>();

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
		edgeCollider2D.Clear();
		edgeCollider2D.AddRange( gameObject.GetComponents<EdgeCollider2D>() );
		polygonCollider2D.Clear();
		polygonCollider2D.AddRange( gameObject.GetComponents<PolygonCollider2D>() );
#endif

		// Sanitize colliders - get rid of unused / incorrect ones in editor
		if (sprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D) {
			// Delete colliders from wrong physics engine
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			if (boxCollider2D != null) {
				DestroyImmediate(boxCollider2D, true);
			}
			foreach (PolygonCollider2D c2d in polygonCollider2D) {
				if (c2d != null) {
					DestroyImmediate(c2d, true);
				}
			}
			polygonCollider2D.Clear();
			foreach (EdgeCollider2D e2d in edgeCollider2D) {
				if (e2d != null) {
					DestroyImmediate(e2d, true);
				}
			}
			edgeCollider2D.Clear();
#endif

			// Delete mismatched collider
			if ((NeedBoxCollider() || sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box) && meshCollider == null)
			{
				if (meshCollider != null) {
					DestroyImmediate(meshCollider, true);
				}
			}
			else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Mesh) {
				if (boxCollider != null) {
					DestroyImmediate(boxCollider, true);
				}
			}
			else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.None) {
				if (meshCollider != null) {
					DestroyImmediate(meshCollider, true);
				}
				if (boxCollider != null) {
					DestroyImmediate(boxCollider, true);
				}
			}
		}
		else if (sprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D) {
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			// Delete colliders from wrong physics engine
			if (boxCollider != null) {
				DestroyImmediate(boxCollider, true);
			}
			if (meshCollider != null) {
				DestroyImmediate(meshCollider, true);
			}
			foreach (PolygonCollider2D c2d in polygonCollider2D) {
				if (c2d != null) {
					DestroyImmediate(c2d, true);
				}
			}
			polygonCollider2D.Clear();
			foreach (EdgeCollider2D e2d in edgeCollider2D) {
				if (e2d != null) {
					DestroyImmediate(e2d, true);
				}
			}
			edgeCollider2D.Clear();
			if (boxCollider2D != null) {
				DestroyImmediate(boxCollider2D, true);
				boxCollider2D = null;
			}
#endif
		}

		CreateCollider();
		
		if (GetComponent<Collider>())
		{
			GetComponent<Collider>().isTrigger = isTrigger;
			GetComponent<Collider>().material = physicsMaterial;
		}

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		if (boxCollider2D) {
			boxCollider2D.isTrigger = isTrigger;
			boxCollider2D.sharedMaterial = physicsMaterial2D;
		}

		foreach (EdgeCollider2D ec in edgeCollider2D) {
			ec.isTrigger = isTrigger;
			ec.sharedMaterial = physicsMaterial2D;
		}

		foreach (PolygonCollider2D pc in polygonCollider2D) {
			pc.isTrigger = isTrigger;
			pc.sharedMaterial = physicsMaterial2D;
		}
#endif

	}
#endif

	protected void Awake()
	{
		if (collection != null)
		{
			collectionInst = collection.inst;
		}

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
		CachedRenderer.sortingOrder = renderLayer;
#endif		
	}

#if UNITY_EDITOR
	private void OnEnable() {
		if (GetComponent<Renderer>() != null && Collection != null && GetComponent<Renderer>().sharedMaterial == null && Collection.inst.needMaterialInstance) {
			ForceBuild();
		}
	}
#endif
	
	// Used by derived classes only
	public void CreateSimpleBoxCollider() {
		if (CurrentSprite == null) {
			return;
		}
		if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D) {
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			boxCollider2D = GetComponent<BoxCollider2D>();
			if (boxCollider2D != null) {
				Object.DestroyImmediate(boxCollider2D, true);
			}
#endif
			boxCollider = GetComponent<BoxCollider>();
			if (boxCollider == null) {
				boxCollider = gameObject.AddComponent<BoxCollider>();
			}
		}
		else if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D) {
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			boxCollider = GetComponent<BoxCollider>();
			if (boxCollider != null) {
				Object.DestroyImmediate(boxCollider, true);
			}
			boxCollider2D = GetComponent<BoxCollider2D>();
			if (boxCollider2D == null) {
				boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
			}
#endif
		}
	}	
	
	// tk2dRuntime.ISpriteCollectionEditor
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return Collection == spriteCollection;
	}
	
	public virtual void ForceBuild()
	{
		if (collection == null) {
			return;
		}
		collectionInst = collection.inst;
		if (spriteId < 0 || spriteId >= collectionInst.spriteDefinitions.Length)
    		spriteId = 0;
#if UNITY_EDITOR
		EditMode__CreateCollider();
#endif
		Build();
		if (SpriteChanged != null) {
			SpriteChanged(this);
		}
	}

	/// <summary>
	/// Create a sprite (and gameObject) displaying the region of the texture specified.
	/// Use <see cref="tk2dSpriteCollectionData.CreateFromTexture"/> if you need to create a sprite collection
	/// with multiple sprites.
	/// </summary>
	public static GameObject CreateFromTexture<T>(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor) where T : tk2dBaseSprite
	{
		tk2dSpriteCollectionData data = tk2dRuntime.SpriteCollectionGenerator.CreateFromTexture(texture, size, region, anchor);
		if (data == null)
			return null;
		GameObject spriteGo = new GameObject();
		tk2dBaseSprite.AddComponent<T>(spriteGo, data, 0);
		return spriteGo;
	}
}

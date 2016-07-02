using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NTexture
	{
		public Texture nativeTexture { get; private set; }
		public Texture alphaTexture { get; set; }
		public NTexture root { get; private set; }
		public Rect uvRect { get; private set; }
		public Dictionary<string, MaterialManager> materialManagers { get; internal set; }
		public int refCount;
		public bool disposed;
		public float lastActive;

		Rect? _region;

		static Texture2D CreateEmptyTexture()
		{
			Texture2D emptyTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
			emptyTexture.hideFlags = DisplayOptions.hideFlags;
			emptyTexture.SetPixel(0, 0, Color.white);
			emptyTexture.Apply();
			return emptyTexture;
		}

		static NTexture _empty;
		public static NTexture Empty
		{
			get
			{
				if (_empty == null)
					_empty = new NTexture(CreateEmptyTexture());

				return _empty;
			}
		}

		public static void DisposeEmpty()
		{
			if (_empty != null)
			{
				if (_empty.nativeTexture != null)
				{
					Texture.DestroyImmediate(_empty.nativeTexture);
					_empty.nativeTexture = null;
				}
				_empty.Dispose();
				_empty = null;
			}
		}

		public NTexture(Texture texture)
		{
			root = this;
			nativeTexture = texture;
			uvRect = new Rect(0, 0, 1, 1);
		}

		public NTexture(Texture texture, float xScale, float yScale)
		{
			root = this;
			nativeTexture = texture;
			uvRect = new Rect(0, 0, xScale, yScale);
		}

		public NTexture(Texture texture, Rect region)
		{
			root = this;
			nativeTexture = texture;
			_region = region;
			uvRect = new Rect(region.x / nativeTexture.width, 1 - region.yMax / nativeTexture.height,
				region.width / nativeTexture.width, region.height / nativeTexture.height);
		}

		public NTexture(NTexture root, Rect region)
		{
			this.root = root;
			nativeTexture = root.nativeTexture;

			if (root._region != null)
			{
				region.x += ((Rect)root._region).x;
				region.y += ((Rect)root._region).y;
			}
			_region = region;

			uvRect = new Rect(region.x * root.uvRect.width / nativeTexture.width, 1 - region.yMax * root.uvRect.height / nativeTexture.height,
				region.width * root.uvRect.width / nativeTexture.width, region.height * root.uvRect.height / nativeTexture.height);
		}

		public int width
		{
			get
			{
				if (_region != null)
					return (int)((Rect)_region).width;
				else
					return nativeTexture.width;
			}
		}

		public int height
		{
			get
			{
				if (_region != null)
					return (int)((Rect)_region).height;
				else
					return nativeTexture.height;
			}
		}

		public void DestroyMaterials()
		{
			if (materialManagers != null && materialManagers.Count > 0)
			{
				foreach (KeyValuePair<string, MaterialManager> kv in materialManagers)
				{
					kv.Value.Dispose();
				}
				materialManagers.Clear();
			}
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				DestroyMaterials();
				if (nativeTexture != null)
				{
					if (root == this)
						Texture.Destroy(nativeTexture);
					nativeTexture = null;
					root = null;
				}
			}
		}
	}
}

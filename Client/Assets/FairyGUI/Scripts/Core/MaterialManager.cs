using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Every texture and shader combination has a MaterialManager.
	/// </summary>
	public class MaterialManager
	{
		public NTexture texture { get; private set; }
		public string shaderName { get; private set; }

		MaterialPool[] _pools;
		string[] _keywords;

		internal uint frameId;
		internal uint clipId;
		internal BlendMode blendMode;

		static uint _gCounter;

		static string[] GRAYED = new string[] { "GRAYED" };
		static string[] CLIPPED = new string[] { "CLIPPED" };
		static string[] CLIPPED_GRAYED = new string[] { "CLIPPED", "GRAYED" };
		static string[] SOFT_CLIPPED = new string[] { "SOFT_CLIPPED" };
		static string[] SOFT_CLIPPED_GRAYED = new string[] { "SOFT_CLIPPED", "GRAYED" };
		static string[] ALPHA_MASK = new string[] { "ALPHA_MASK" };

		public static MaterialManager GetInstance(NTexture texture, string shaderName, string[] keywords)
		{
			NTexture rootTexture = texture.root;
			if (rootTexture.materialManagers == null)
				rootTexture.materialManagers = new Dictionary<string, MaterialManager>();

			string key = shaderName;
			if (keywords != null)
			{
				//对于带指定关键字的，目前的设计是不参加共享材质了，因为逻辑会变得更复杂
				key = shaderName + "_" + _gCounter++;
			}

			MaterialManager mm;
			if (!rootTexture.materialManagers.TryGetValue(key, out mm))
			{
				mm = new MaterialManager(rootTexture);
				mm.shaderName = shaderName;
				mm._keywords = keywords;
				rootTexture.materialManagers.Add(key, mm);
			}

			return mm;
		}

		public MaterialManager(NTexture texture)
		{
			this.texture = texture;

			_pools = new MaterialPool[7];
			_pools[0] = new MaterialPool(this, null); //none
			_pools[1] = new MaterialPool(this, GRAYED); //grayed
			_pools[2] = new MaterialPool(this, CLIPPED); //clipped
			_pools[3] = new MaterialPool(this, CLIPPED_GRAYED); //clipped+grayed
			_pools[4] = new MaterialPool(this, SOFT_CLIPPED); //softClipped
			_pools[5] = new MaterialPool(this, SOFT_CLIPPED_GRAYED); //softClipped+grayed
			_pools[6] = new MaterialPool(this, ALPHA_MASK); //stencil mask
		}

		public Material GetMaterial(NGraphics grahpics, UpdateContext context)
		{
			frameId = UpdateContext.frameId;
			blendMode = grahpics.blendMode;
			int pool;

			if (context.clipped)
			{
				clipId = context.clipInfo.clipId;

				if (grahpics.maskFrameId == UpdateContext.frameId)
					pool = 6;
				else if (context.rectMaskDepth == 0)
				{
					if (grahpics.grayed)
						pool = 1;
					else
						pool = 0;
				}
				else
				{
					if (context.clipInfo.soft)
					{
						if (grahpics.grayed)
							pool = 5;
						else
							pool = 4;
					}
					else
					{
						if (grahpics.grayed)
							pool = 3;
						else
							pool = 2;
					}
				}
			}
			else
			{
				clipId = 0;
				if (grahpics.grayed)
					pool = 1;
				else
					pool = 0;
			}
			return _pools[pool].Get();
		}

		public NMaterial CreateMaterial()
		{
			NMaterial mat = new NMaterial(ShaderConfig.GetShader(shaderName));
			mat.mainTexture = texture.nativeTexture;
			if (texture.alphaTexture != null)
			{
				mat.EnableKeyword("COMBINED");
				mat.SetTexture("_AlphaTex", texture.alphaTexture);
			}
			if (_keywords != null)
			{
				foreach (string v in _keywords)
					mat.EnableKeyword(v);
			}
			mat.hideFlags = DisplayOptions.hideFlags;

			return mat;
		}

		public void Dispose()
		{
			foreach (MaterialPool pool in _pools)
				pool.Dispose();
		}

		public void Release()
		{
			if (_keywords != null)
				Dispose();
		}
	}
}

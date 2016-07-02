using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NMaterial : Material
	{
		public uint frameId;
		public uint clipId;
		public bool stencilSet;
		public BlendMode blendMode;

		public NMaterial(Shader shader)
			: base(shader)
		{
		}
	}
}

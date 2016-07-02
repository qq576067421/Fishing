using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public interface IHtmlObject
	{
		float width { get; }
		float height { get; }
		HtmlElement element { get; }

		void Create(RichTextField owner, HtmlElement element);
		void SetPosition(float x, float y);
		void Add();
		void Remove();
		void Dispose();
	}
}

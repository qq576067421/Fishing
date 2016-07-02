using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public enum HtmlElementType
	{
		Text,
		LinkStart,
		LinkEnd,
		Image,
		Input,
		Select,
		Object
	}

	/// <summary>
	/// 
	/// </summary>
	public class HtmlElement
	{
		public HtmlElementType type;
		public string name;
		public string text;
		public TextFormat format;

		public IHtmlObject htmlObject;
		public int space;

		internal int quadStart;
		internal int quadEnd;
		internal Hashtable attributes;

		public HtmlElement()
		{
			format = new TextFormat();
		}

		public object Get(string attrName)
		{
			if (attributes == null)
				return null;

			return attributes[attrName];
		}

		public void Set(string attrName, object attrValue)
		{
			if (attributes == null)
				attributes = new Hashtable();

			attributes[attrName] = attrValue;
		}

		public string GetString(string attrName)
		{
			return GetString(attrName, null);
		}

		public string GetString(string attrName, string defValue)
		{
			if (attributes == null)
				return defValue;

			string ret = (string)attributes[attrName];
			if (ret != null)
				return ret;
			else
				return defValue;
		}

		public int GetInt(string attrName)
		{
			return GetInt(attrName, 0);
		}

		public int GetInt(string attrName, int defValue)
		{
			string value = GetString(attrName);
			if (value == null || value.Length == 0)
				return defValue;

			int ret;
			if (int.TryParse(value, out ret))
				return ret;
			else
				return defValue;
		}

		public float GetFloat(string attrName)
		{
			return GetFloat(attrName, 0);
		}

		public float GetFloat(string attrName, float defValue)
		{
			string value = GetString(attrName);
			if (value == null || value.Length == 0)
				return defValue;

			float ret;
			if (float.TryParse(value, out ret))
				return ret;
			else
				return defValue;
		}

		public bool GetBool(string attrName)
		{
			return GetBool(attrName, false);
		}

		public bool GetBool(string attrName, bool defValue)
		{
			string value = GetString(attrName);
			if (value == null || value.Length == 0)
				return defValue;

			bool ret;
			if (bool.TryParse(value, out ret))
				return ret;
			else
				return defValue;
		}

		public Color GetColor(string attrName, Color defValue)
		{
			string value = GetString(attrName);
			if (value == null || value.Length == 0)
				return defValue;

			return ToolSet.ConvertFromHtmlColor(value);
		}

		#region Pool Support

		static Stack<HtmlElement> elementPool = new Stack<HtmlElement>();

		public static HtmlElement GetElement(HtmlElementType type)
		{
			HtmlElement ret;
			if (elementPool.Count > 0)
				ret = elementPool.Pop();
			else
				ret = new HtmlElement();
			ret.type = type;

			if (type != HtmlElementType.Text && ret.attributes == null)
				ret.attributes = new Hashtable();

			return ret;
		}

		public static void ReturnElement(HtmlElement element)
		{
			element.name = null;
			element.text = null;
			element.htmlObject = null;
			if (element.attributes != null)
				element.attributes.Clear();
			elementPool.Push(element);
		}

		public static void ReturnElements(List<HtmlElement> elements)
		{
			int count = elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = elements[i];
				ReturnElement(element);
			}
			elements.Clear();
		}

		#endregion
	}
}

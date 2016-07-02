using System.Collections;
using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PageOptionSet
	{
		public Controller controller;
		List<string> _items;

		public PageOptionSet()
		{
		}

		public void Add(int pageIndex)
		{
			if (_items == null)
				_items = new List<string>();

			string id = controller.GetPageId(pageIndex);
			int i = _items.IndexOf(id);
			if (i == -1)
				_items.Add(id);
		}

		public void Remove(int pageIndex)
		{
			if (_items == null)
				return;

			string id = controller.GetPageId(pageIndex);
			int i = _items.IndexOf(id);
			if (i != -1)
				_items.RemoveAt(i);
		}

		public void AddByName(string pageName)
		{
			if (_items == null)
				_items = new List<string>();

			string id = controller.GetPageIdByName(pageName);
			int i = _items.IndexOf(id);
			if (i != -1)
				_items.Add(id);
		}

		public void RemoveByName(string pageName)
		{
			if (_items == null)
				return;

			string id = controller.GetPageIdByName(pageName);
			int i = _items.IndexOf(id);
			if (i != -1)
				_items.RemoveAt(i);
		}

		public void Clear()
		{
			if (_items == null)
				return;

			_items.Clear();
		}

		public bool isEmpty
		{
			get { return _items != null && _items.Count == 0; }
		}

		internal void AddById(string id)
		{
			if (_items == null)
				_items = new List<string>();

			_items.Add(id);
		}

		internal bool ContainsId(string id)
		{
			return _items != null && _items.IndexOf(id) != -1;
		}
	}
}

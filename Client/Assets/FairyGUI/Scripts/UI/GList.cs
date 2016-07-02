using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// Callback function when an item is needed to update its look.
	/// </summary>
	/// <param name="index">Item index.</param>
	/// <param name="item">Item object.</param>
	public delegate void ListItemRenderer(int index, GObject item);

	/// <summary>
	/// GList class.
	/// </summary>
	public class GList : GComponent
	{
		/// <summary>
		/// Resource url of the default item.
		/// </summary>
		public string defaultItem;

		/// <summary>
		/// If the item will resize itself to fit the list width/height.
		/// </summary>
		public bool autoResizeItem;

		/// <summary>
		/// List selection mode
		/// </summary>
		/// <seealso cref="ListSelectionMode"/>
		public ListSelectionMode selectionMode;

		/// <summary>
		/// Callback function when an item is needed to update its look.
		/// </summary>
		public ListItemRenderer itemRenderer;

		/// <summary>
		/// Dispatched when a list item being clicked.
		/// </summary>
		public EventListener onClickItem { get; private set; }

		ListLayoutType _layout;
		int _lineItemCount;
		int _lineGap;
		int _columnGap;
		GObjectPool _pool;
		bool _selectionHandled;
		int _lastSelectedIndex;

		//Virtual List support
		bool _virtual;
		bool _loop;
		int _numItems;
		int _firstIndex; //the top left index
		int _viewCount; //item count in view
		int _curLineItemCount; //item count in one line
		Vector2 _itemSize;
		int _virtualListChanged; //1-content changed, 2-size changed
		bool _eventLocked;

		public GList()
			: base()
		{
			_pool = new GObjectPool();
			_trackBounds = true;
			autoResizeItem = true;
			this.opaque = true;

			onClickItem = new EventListener(this, "onClickItem");
		}

		public override void Dispose()
		{
			_pool.Clear();
			base.Dispose();
		}

		/// <summary>
		/// List layout type.
		/// </summary>
		public ListLayoutType layout
		{
			get { return _layout; }
			set
			{
				if (_layout != value)
				{
					_layout = value;
					SetBoundsChangedFlag();
					if (_virtual)
						SetVirtualListChangedFlag(true);
				}
			}
		}

		/// <summary>
		/// Item count in one line.
		/// </summary>
		public int lineItemCount
		{
			get { return _lineItemCount; }
			set
			{
				if (_lineItemCount != value)
				{
					_lineItemCount = value;
					SetBoundsChangedFlag();
					if (_virtual)
						SetVirtualListChangedFlag(true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int lineGap
		{
			get { return _lineGap; }
			set
			{
				if (_lineGap != value)
				{
					_lineGap = value;
					SetBoundsChangedFlag();
					if (_virtual)
						SetVirtualListChangedFlag(true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int columnGap
		{
			get { return _columnGap; }
			set
			{
				if (_columnGap != value)
				{
					_columnGap = value;
					SetBoundsChangedFlag();
					if (_virtual)
						SetVirtualListChangedFlag(true);
				}
			}
		}

		public Vector2 virtualItemSize
		{
			get { return _itemSize; }
			set
			{
				_itemSize = value;
				if (_virtual)
					SetVirtualListChangedFlag(true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public GObjectPool itemPool
		{
			get { return _pool; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public GObject GetFromPool(string url)
		{
			if (string.IsNullOrEmpty(url))
				url = defaultItem;

			GObject ret = _pool.GetObject(url);
			if (ret != null)
				ret.visible = true;
			return ret;
		}

		void ReturnToPool(GObject obj)
		{
			_pool.ReturnObject(obj);
		}

		/// <summary>
		/// Add a item to list, same as GetFromPool+AddChild
		/// </summary>
		/// <returns>Item object</returns>
		public GObject AddItemFromPool()
		{
			GObject obj = GetFromPool(null);

			return AddChild(obj);
		}

		/// <summary>
		/// Add a item to list, same as GetFromPool+AddChild
		/// </summary>
		/// <param name="url">Item resource url</param>
		/// <returns>Item object</returns>
		public GObject AddItemFromPool(string url)
		{
			GObject obj = GetFromPool(url);

			return AddChild(obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		override public GObject AddChildAt(GObject child, int index)
		{
			if (autoResizeItem)
			{
				if (_layout == ListLayoutType.SingleColumn)
					child.width = this.viewWidth;
				else if (_layout == ListLayoutType.SingleRow)
					child.height = this.viewHeight;
			}

			base.AddChildAt(child, index);
			if (child is GButton)
			{
				GButton button = (GButton)child;
				button.selected = false;
				button.changeStateOnClick = false;
			}

			child.onTouchBegin.Add(__itemTouchBegin);
			child.onClick.Add(__clickItem);

			return child;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="dispose"></param>
		/// <returns></returns>
		override public GObject RemoveChildAt(int index, bool dispose)
		{
			GObject child = base.RemoveChildAt(index, dispose);
			child.onTouchBegin.Remove(__itemTouchBegin);
			child.onClick.Remove(__clickItem);

			return child;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		public void RemoveChildToPoolAt(int index)
		{
			GObject child = base.RemoveChildAt(index);
			ReturnToPool(child);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		public void RemoveChildToPool(GObject child)
		{
			base.RemoveChild(child);
			ReturnToPool(child);
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveChildrenToPool()
		{
			RemoveChildrenToPool(0, -1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="beginIndex"></param>
		/// <param name="endIndex"></param>
		public void RemoveChildrenToPool(int beginIndex, int endIndex)
		{
			if (endIndex < 0 || endIndex >= _children.Count)
				endIndex = _children.Count - 1;

			for (int i = beginIndex; i <= endIndex; ++i)
				RemoveChildToPoolAt(beginIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		public int selectedIndex
		{
			get
			{
				int cnt = _children.Count;
				int j;
				for (int i = 0; i < cnt; i++)
				{
					GButton obj = _children[i].asButton;
					if (obj != null && obj.selected)
					{
						j = _firstIndex + i;
						if (_loop && _numItems > 0)
							j = j % _numItems;
						return j;
					}
				}
				return -1;
			}

			set
			{
				ClearSelection();
				if (value >= 0 && value < this.numItems)
					AddSelection(value, false);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public List<int> GetSelection()
		{
			List<int> ret = new List<int>();
			int cnt = _children.Count;
			int j;
			for (int i = 0; i < cnt; i++)
			{
				GButton obj = _children[i].asButton;
				if (obj != null && obj.selected)
				{
					j = _firstIndex + i;
					if (_loop && _numItems > 0)
						j = j % _numItems;
					ret.Add(j);
				}
			}
			return ret;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="scrollItToView"></param>
		public void AddSelection(int index, bool scrollItToView)
		{
			if (selectionMode == ListSelectionMode.None)
				return;

			if (selectionMode == ListSelectionMode.Single)
				ClearSelection();

			if (scrollItToView)
				ScrollToView(index);

			if (_loop && _numItems > 0)
			{
				int j = _firstIndex % _numItems;
				if (index >= j)
					index = _firstIndex + (index - j);
				else
					index = _firstIndex + _numItems + (j - index);
			}
			else
				index -= _firstIndex;
			if (index < 0 || index >= _children.Count)
				return;

			GButton obj = GetChildAt(index).asButton;
			if (obj != null && !obj.selected)
				obj.selected = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		public void RemoveSelection(int index)
		{
			if (selectionMode == ListSelectionMode.None)
				return;

			if (_loop && _numItems > 0)
			{
				int j = _firstIndex % _numItems;
				if (index >= j)
					index = _firstIndex + (index - j);
				else
					index = _firstIndex + _numItems + (j - index);
			}
			else
				index -= _firstIndex;
			if (index < 0 || index >= _children.Count)
				return;

			GButton obj = GetChildAt(index).asButton;
			if (obj != null && obj.selected)
				obj.selected = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearSelection()
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				GButton obj = _children[i].asButton;
				if (obj != null)
					obj.selected = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SelectAll()
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				GButton obj = _children[i].asButton;
				if (obj != null)
					obj.selected = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SelectNone()
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				GButton obj = _children[i].asButton;
				if (obj != null)
					obj.selected = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SelectReverse()
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				GButton obj = _children[i].asButton;
				if (obj != null)
					obj.selected = !obj.selected;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dir"></param>
		public void HandleArrowKey(int dir)
		{
			int index = this.selectedIndex;
			if (index == -1)
				return;

			switch (dir)
			{
				case 1://up
					if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowVertical)
					{
						index--;
						if (index >= 0)
						{
							ClearSelection();
							AddSelection(index, true);
						}
					}
					else if (_layout == ListLayoutType.FlowHorizontal)
					{
						GObject current = _children[index];
						int k = 0;
						int i;
						for (i = index - 1; i >= 0; i--)
						{
							GObject obj = _children[i];
							if (obj.y != current.y)
							{
								current = obj;
								break;
							}
							k++;
						}
						for (; i >= 0; i--)
						{
							GObject obj = _children[i];
							if (obj.y != current.y)
							{
								ClearSelection();
								AddSelection(i + k + 1, true);
								break;
							}
						}
					}
					break;

				case 3://right
					if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowHorizontal)
					{
						index++;
						if (index < _children.Count)
						{
							ClearSelection();
							AddSelection(index, true);
						}
					}
					else if (_layout == ListLayoutType.FlowVertical)
					{
						GObject current = _children[index];
						int k = 0;
						int cnt = _children.Count;
						int i;
						for (i = index + 1; i < cnt; i++)
						{
							GObject obj = _children[i];
							if (obj.x != current.x)
							{
								current = obj;
								break;
							}
							k++;
						}
						for (; i < cnt; i++)
						{
							GObject obj = _children[i];
							if (obj.x != current.x)
							{
								ClearSelection();
								AddSelection(i - k - 1, true);
								break;
							}
						}
					}
					break;

				case 5://down
					if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowVertical)
					{
						index++;
						if (index < _children.Count)
						{
							ClearSelection();
							AddSelection(index, true);
						}
					}
					else if (_layout == ListLayoutType.FlowHorizontal)
					{
						GObject current = _children[index];
						int k = 0;
						int cnt = _children.Count;
						int i;
						for (i = index + 1; i < cnt; i++)
						{
							GObject obj = _children[i];
							if (obj.y != current.y)
							{
								current = obj;
								break;
							}
							k++;
						}
						for (; i < cnt; i++)
						{
							GObject obj = _children[i];
							if (obj.y != current.y)
							{
								ClearSelection();
								AddSelection(i - k - 1, true);
								break;
							}
						}
					}
					break;

				case 7://left
					if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowHorizontal)
					{
						index--;
						if (index >= 0)
						{
							ClearSelection();
							AddSelection(index, true);
						}
					}
					else if (_layout == ListLayoutType.FlowVertical)
					{
						GObject current = _children[index];
						int k = 0;
						int i;
						for (i = index - 1; i >= 0; i--)
						{
							GObject obj = _children[i];
							if (obj.x != current.x)
							{
								current = obj;
								break;
							}
							k++;
						}
						for (; i >= 0; i--)
						{
							GObject obj = _children[i];
							if (obj.x != current.x)
							{
								ClearSelection();
								AddSelection(i + k + 1, true);
								break;
							}
						}
					}
					break;
			}
		}

		void __itemTouchBegin(EventContext context)
		{
			GButton item = context.sender as GButton;
			if (item == null || selectionMode == ListSelectionMode.None)
				return;

			_selectionHandled = false;

			if (UIConfig.defaultScrollTouchEffect
				&& (this.scrollPane != null || this.parent != null && this.parent.scrollPane != null))
				return;

			if (selectionMode == ListSelectionMode.Single)
			{
				SetSelectionOnEvent(item, (InputEvent)context.data);
			}
			else
			{
				if (!item.selected)
					SetSelectionOnEvent(item, (InputEvent)context.data);
				//如果item.selected，这里不处理selection，因为可能用户在拖动
			}
		}

		void __clickItem(EventContext context)
		{
			GObject item = context.sender as GObject;
			if (!_selectionHandled)
				SetSelectionOnEvent(item, (InputEvent)context.data);
			_selectionHandled = false;

			if (scrollPane != null)
				scrollPane.ScrollToView(item, true);

			onClickItem.Call(item);
		}

		void SetSelectionOnEvent(GObject item, InputEvent evt)
		{
			if (!(item is GButton) || selectionMode == ListSelectionMode.None)
				return;

			_selectionHandled = true;
			bool dontChangeLastIndex = false;
			GButton button = (GButton)item;
			int index = GetChildIndex(item);

			if (selectionMode == ListSelectionMode.Single)
			{
				if (!button.selected)
				{
					ClearSelectionExcept(button);
					button.selected = true;
				}
			}
			else
			{
				if (evt.shift)
				{
					if (!button.selected)
					{
						if (_lastSelectedIndex != -1)
						{
							int min = Math.Min(_lastSelectedIndex, index);
							int max = Math.Max(_lastSelectedIndex, index);
							max = Math.Min(max, _children.Count - 1);
							for (int i = min; i <= max; i++)
							{
								GButton obj = GetChildAt(i).asButton;
								if (obj != null && !obj.selected)
									obj.selected = true;
							}

							dontChangeLastIndex = true;
						}
						else
						{
							button.selected = true;
						}
					}
				}
				else if (evt.ctrl || selectionMode == ListSelectionMode.Multiple_SingleClick)
				{
					button.selected = !button.selected;
				}
				else
				{
					if (!button.selected)
					{
						ClearSelectionExcept(button);
						button.selected = true;
					}
					else
						ClearSelectionExcept(button);
				}
			}

			if (!dontChangeLastIndex)
				_lastSelectedIndex = index;
		}

		void ClearSelectionExcept(GObject obj)
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				GButton button = _children[i].asButton;
				if (button != null && button != obj && button.selected)
					button.selected = false;
			}
		}

		/// <summary>
		/// Resize to list size to fit specified item count. 
		/// If list layout is single column or flow horizontally, the height will change to fit. 
		/// If list layout is single row or flow vertically, the width will change to fit.
		/// </summary>
		/// <param name="itemCount">Item count</param>
		public void ResizeToFit(int itemCount)
		{
			ResizeToFit(itemCount, 0);
		}

		/// <summary>
		/// Resize to list size to fit specified item count. 
		/// If list layout is single column or flow horizontally, the height will change to fit. 
		/// If list layout is single row or flow vertically, the width will change to fit.
		/// </summary>
		/// <param name="itemCount">>Item count</param>
		/// <param name="minSize">If the result size if smaller than minSize, then use minSize.</param>
		public void ResizeToFit(int itemCount, int minSize)
		{
			EnsureBoundsCorrect();

			int curCount = this.numItems;
			if (itemCount > curCount)
				itemCount = curCount;

			if (_virtual)
			{
				int lineCount = Mathf.CeilToInt((float)itemCount / _curLineItemCount);
				if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
					this.viewHeight = lineCount * _itemSize.y + Math.Max(0, lineCount - 1) * _lineGap;
				else
					this.viewWidth = lineCount * _itemSize.x + Math.Max(0, lineCount - 1) * _columnGap;
			}
			else if (itemCount == 0)
			{
				if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
					this.viewHeight = minSize;
				else
					this.viewWidth = minSize;
			}
			else
			{
				int i = itemCount - 1;
				GObject obj = null;
				while (i >= 0)
				{
					obj = this.GetChildAt(i);
					if (obj.visible)
						break;
					i--;
				}
				if (i < 0)
				{
					if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
						this.viewHeight = minSize;
					else
						this.viewWidth = minSize;
				}
				else
				{
					float size;
					if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
					{
						size = obj.y + obj.height;
						if (size < minSize)
							size = minSize;
						this.viewHeight = size;
					}
					else
					{
						size = obj.x + obj.width;
						if (size < minSize)
							size = minSize;
						this.viewWidth = size;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if (autoResizeItem)
				AdjustItemsSize();

			if (_layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.FlowVertical)
			{
				SetBoundsChangedFlag();
				if (_virtual)
					SetVirtualListChangedFlag(true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void AdjustItemsSize()
		{
			if (_layout == ListLayoutType.SingleColumn)
			{
				int cnt = _children.Count;
				float cw = this.viewWidth;
				for (int i = 0; i < cnt; i++)
				{
					GObject child = GetChildAt(i);
					child.width = cw;
				}
			}
			else if (_layout == ListLayoutType.SingleRow)
			{
				int cnt = _children.Count;
				float ch = this.viewHeight;
				for (int i = 0; i < cnt; i++)
				{
					GObject child = GetChildAt(i);
					child.height = ch;
				}
			}
		}

		/// <summary>
		/// Scroll the list to make an item with certain index visible.
		/// </summary>
		/// <param name="index">Item index</param>
		public void ScrollToView(int index)
		{
			ScrollToView(index, false);
		}

		/// <summary>
		///  Scroll the list to make an item with certain index visible.
		/// </summary>
		/// <param name="index">Item index</param>
		/// <param name="ani">True to scroll smoothly, othewise immdediately.</param>
		public void ScrollToView(int index, bool ani)
		{
			ScrollToView(index, false, false);
		}

		/// <summary>
		///  Scroll the list to make an item with certain index visible.
		/// </summary>
		/// <param name="index">Item index</param>
		/// <param name="ani">True to scroll smoothly, othewise immdediately.</param>
		/// <param name="setFirst">If true, scroll to make the target on the top/left; If false, scroll to make the target any position in view.</param>
		public void ScrollToView(int index, bool ani, bool setFirst)
		{
			if (_virtual)
			{
				if (this.scrollPane != null)
					scrollPane.ScrollToView(GetItemRect(index), ani, setFirst);
				else if (parent != null && parent.scrollPane != null)
					parent.scrollPane.ScrollToView(GetItemRect(index), ani, setFirst);
			}
			else
			{
				GObject obj = GetChildAt(index);
				if (this.scrollPane != null)
					scrollPane.ScrollToView(obj, ani, setFirst);
				else if (parent != null && parent.scrollPane != null)
					parent.scrollPane.ScrollToView(obj, ani, setFirst);
			}
		}

		/// <summary>
		/// Get first child in view.
		/// </summary>
		/// <returns></returns>
		public override int GetFirstChildInView()
		{
			int ret = base.GetFirstChildInView();
			if (ret != -1)
			{
				ret += _firstIndex;
				if (_loop && _numItems > 0)
					ret = ret % _numItems;
				return ret;
			}
			else
				return -1;
		}

		/// <summary>
		/// Set the list to be virtual list.
		/// </summary>
		public void SetVirtual()
		{
			SetVirtual(false);
		}

		/// <summary>
		/// Set the list to be virtual list, and has loop behavior.
		/// </summary>
		public void SetVirtualAndLoop()
		{
			SetVirtual(true);
		}

		void SetVirtual(bool loop)
		{
			if (!_virtual)
			{
				if (this.scrollPane == null)
					Debug.LogError("FairyGUI: Virtual list must be scrollable!");

				if (loop)
				{
					if (_layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.FlowVertical)
						Debug.LogError("FairyGUI: Only single row or single column layout type is supported for loop list!");

					this.scrollPane.bouncebackEffect = false;
				}

				_virtual = true;
				_loop = loop;
				RemoveChildrenToPool();

				if (_itemSize.x == 0 || _itemSize.y == 0)
				{
					GObject obj = GetFromPool(null);
					if (obj == null)
						Debug.LogError("FairyGUI: Virtual List must have a default list item resource.");
					_itemSize = obj.size;
					ReturnToPool(obj);
				}

				if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
					this.scrollPane.scrollSpeed = _itemSize.y;
				else
					this.scrollPane.scrollSpeed = _itemSize.x;

				this.scrollPane.onScroll.AddCapture(__scrolled);
				SetVirtualListChangedFlag(true);
			}
		}

		/// <summary>
		/// Set the list item count. 
		/// If the list is not virtual, specified number of items will be created. 
		/// If the list is virtual, only items in view will be created.
		/// </summary>
		public int numItems
		{
			get
			{
				if (_virtual)
					return _numItems;
				else
					return _children.Count;
			}
			set
			{
				if (_virtual)
				{
					_numItems = value;
					SetVirtualListChangedFlag(false);
				}
				else
				{
					int cnt = _children.Count;
					if (value > cnt)
					{
						for (int i = cnt; i < value; i++)
							AddItemFromPool();
					}
					else
					{
						RemoveChildrenToPool(value, cnt);
					}

					if (itemRenderer != null)
					{
						for (int i = 0; i < value; i++)
							itemRenderer(i, GetChildAt(i));
					}
				}
			}
		}

		void __parentSizeChanged()
		{
			SetVirtualListChangedFlag(true);
		}

		void SetVirtualListChangedFlag(bool layoutChanged)
		{
			if (layoutChanged)
				_virtualListChanged = 2;
			else if (_virtualListChanged == 0)
				_virtualListChanged = 1;

			Timers.inst.CallLater(RefreshVirtualList);
		}

		void RefreshVirtualList(object param)
		{
			if (_virtualListChanged == 0)
				return;

			bool layoutChanged = _virtualListChanged == 2;
			_virtualListChanged = 0;
			_eventLocked = true;

			if (layoutChanged)
			{
				if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
				{
					if (_layout == ListLayoutType.SingleColumn)
						_curLineItemCount = 1;
					else if (_lineItemCount != 0)
						_curLineItemCount = _lineItemCount;
					else
						_curLineItemCount = Mathf.FloorToInt((this.scrollPane.viewWidth + _columnGap) / (_itemSize.x + _columnGap));
					_viewCount = (Mathf.CeilToInt((this.scrollPane.viewHeight + _lineGap) / (_itemSize.y + _lineGap)) + 1) * _curLineItemCount;
					int numChildren = _children.Count;
					if (numChildren < _viewCount)
					{
						for (int i = numChildren; i < _viewCount; i++)
							this.AddItemFromPool();
					}
					else if (numChildren > _viewCount)
						this.RemoveChildrenToPool(_viewCount, numChildren);
				}
				else
				{
					if (_layout == ListLayoutType.SingleRow)
						_curLineItemCount = 1;
					else if (_lineItemCount != 0)
						_curLineItemCount = _lineItemCount;
					else
						_curLineItemCount = Mathf.FloorToInt((this.scrollPane.viewHeight + _lineGap) / (_itemSize.y + _lineGap));
					_viewCount = (Mathf.CeilToInt((this.scrollPane.viewWidth + _columnGap) / (_itemSize.x + _columnGap)) + 1) * _curLineItemCount;
					int numChildren = _children.Count;
					if (numChildren < _viewCount)
					{
						for (int i = numChildren; i < _viewCount; i++)
							this.AddItemFromPool();
					}
					else if (numChildren > _viewCount)
						this.RemoveChildrenToPool(_viewCount, numChildren);
				}
			}

			EnsureBoundsCorrect();

			if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
			{
				float ch;
				if (_layout == ListLayoutType.SingleColumn)
				{
					ch = _numItems * _itemSize.y + Math.Max(0, _numItems - 1) * _lineGap;
					if (_loop && ch > 0)
					{
						int loopCount = 5;// Mathf.CeilToInt((this.viewHeight * 2 + 1) / ch);
						ch = ch * loopCount + _lineGap * (loopCount - 1);
					}
				}
				else
				{
					int lineCount = Mathf.CeilToInt((float)_numItems / _curLineItemCount);
					ch = lineCount * _itemSize.y + Math.Max(0, lineCount - 1) * _lineGap;
				}

				this.scrollPane.SetContentSize(this.scrollPane.contentWidth, ch);
			}
			else
			{
				float cw;
				if (_layout == ListLayoutType.SingleRow)
				{
					cw = _numItems * _itemSize.x + Math.Max(0, _numItems - 1) * _columnGap;
					if (_loop && cw > 0)
					{
						int loopCount = 5; // Mathf.CeilToInt((this.viewWidth * 2 + 1) / cw);
						cw = cw * loopCount + _columnGap * (loopCount - 1);
					}
				}
				else
				{
					int lineCount = Mathf.CeilToInt((float)_numItems / _curLineItemCount);
					cw = lineCount * _itemSize.x + Math.Max(0, lineCount - 1) * _columnGap;
				}

				this.scrollPane.SetContentSize(cw, this.scrollPane.contentHeight);
			}

			_eventLocked = false;
			__scrolled(null);
		}

		void RenderItems(int beginIndex, int endIndex)
		{
			for (int i = 0; i < _viewCount; i++)
			{
				GObject obj = GetChildAt(i);
				int j = _firstIndex + i;
				if (_loop && _numItems > 0)
					j = j % _numItems;

				if (j < _numItems)
				{
					obj.visible = true;
					if (i >= beginIndex && i < endIndex)
						itemRenderer(j, obj);
				}
				else
					obj.visible = false;
			}
		}

		Rect GetItemRect(int index)
		{
			Rect rect = new Rect();
			int index1 = index / _curLineItemCount;
			int index2 = index % _curLineItemCount;
			switch (_layout)
			{
				case ListLayoutType.SingleColumn:
					rect = new Rect(0, index1 * _itemSize.y + Math.Max(0, index1 - 1) * _lineGap,
						this.viewWidth, _itemSize.y);
					break;

				case ListLayoutType.FlowHorizontal:
					rect = new Rect(index2 * _itemSize.x + Math.Max(0, index2 - 1) * _columnGap,
						index1 * _itemSize.y + Math.Max(0, index1 - 1) * _lineGap,
						_itemSize.x, _itemSize.y);
					break;

				case ListLayoutType.SingleRow:
					rect = new Rect(index1 * _itemSize.x + Math.Max(0, index1 - 1) * _columnGap, 0,
						_itemSize.x, this.viewHeight);
					break;

				case ListLayoutType.FlowVertical:
					rect = new Rect(index1 * _itemSize.x + Math.Max(0, index1 - 1) * _columnGap,
						index2 * _itemSize.y + Math.Max(0, index2 - 1) * _lineGap,
						_itemSize.x, _itemSize.y);
					break;
			}
			return rect;
		}

		void __scrolled(EventContext context)
		{
			if (_eventLocked)
				return;

			if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
			{
				if (_loop)
				{
					if (scrollPane.percY == 0)
						scrollPane.posY = _numItems * (_itemSize.y + _lineGap);
					else if (scrollPane.percY == 1)
						scrollPane.posY = scrollPane.contentHeight - _numItems * (_itemSize.y + _lineGap) - this.viewHeight;
				}

				int firstLine = Mathf.FloorToInt((scrollPane.posY + _lineGap) / (_itemSize.y + _lineGap));
				int newFirstIndex = firstLine * _curLineItemCount;
				for (int i = 0; i < _viewCount; i++)
				{
					GObject obj = GetChildAt(i);
					obj.y = (firstLine + (i / _curLineItemCount)) * (_itemSize.y + _lineGap);
				}
				if (newFirstIndex >= _numItems)
					newFirstIndex -= _numItems;

				if (newFirstIndex != _firstIndex || context == null)
				{
					int oldFirstIndex = _firstIndex;
					_firstIndex = newFirstIndex;

					if (context == null || oldFirstIndex + _viewCount < newFirstIndex || oldFirstIndex > newFirstIndex + _viewCount)
					{
						//no intersection, render all
						for (int i = 0; i < _viewCount; i++)
						{
							GObject obj = GetChildAt(i);
							if (obj is GButton)
								((GButton)obj).selected = false;
						}
						RenderItems(0, _viewCount);
					}
					else if (oldFirstIndex > newFirstIndex)
					{
						int j1 = oldFirstIndex - newFirstIndex;
						int j2 = _viewCount - j1;
						for (int i = j2 - 1; i >= 0; i--)
						{
							GObject obj1 = GetChildAt(i);
							GObject obj2 = GetChildAt(i + j1);
							if (obj2 is GButton)
								((GButton)obj2).selected = false;
							float tmp = obj1.y;
							obj1.y = obj2.y;
							obj2.y = tmp;
							SwapChildrenAt(i + j1, i);
						}
						RenderItems(0, j1);
					}
					else
					{
						int j1 = newFirstIndex - oldFirstIndex;
						int j2 = _viewCount - j1;
						for (int i = 0; i < j2; i++)
						{
							GObject obj1 = GetChildAt(i);
							GObject obj2 = GetChildAt(i + j1);
							if (obj1 is GButton)
								((GButton)obj1).selected = false;
							float tmp = obj1.y;
							obj1.y = obj2.y;
							obj2.y = tmp;
							SwapChildrenAt(i + j1, i);
						}
						RenderItems(j2, _viewCount);
					}
				}

				if (this.childrenRenderOrder == ChildrenRenderOrder.Arch)
				{
					float mid = this.scrollPane.posY + this.viewHeight / 2;
					float minDist = int.MaxValue, dist;
					int apexIndex = 0;
					for (int i = 0; i < _viewCount; i++)
					{
						GObject obj = GetChildAt(i);
						if (obj.visible)
						{
							dist = Mathf.Abs(mid - obj.y - obj.height / 2);
							if (dist < minDist)
							{
								minDist = dist;
								apexIndex = i;
							}
						}
					}
					this.apexIndex = apexIndex;
				}
			}
			else
			{
				if (_loop)
				{
					if (scrollPane.percX == 0)
						scrollPane.posX = _numItems * (_itemSize.x + _columnGap);
					else if (scrollPane.percX == 1)
						scrollPane.posX = scrollPane.contentWidth - _numItems * (_itemSize.x + _columnGap) - this.viewWidth;
				}
				int firstLine = Mathf.FloorToInt((scrollPane.posX + _columnGap) / (_itemSize.x + _columnGap));
				int newFirstIndex = firstLine * _curLineItemCount;
				for (int i = 0; i < _viewCount; i++)
				{
					GObject obj = GetChildAt(i);
					obj.x = (firstLine + (i / _curLineItemCount)) * (_itemSize.x + _columnGap);
				}

				if (newFirstIndex >= _numItems)
					newFirstIndex -= _numItems;

				if (newFirstIndex != _firstIndex || context == null)
				{
					int oldFirstIndex = _firstIndex;
					_firstIndex = newFirstIndex;
					if (context == null || oldFirstIndex + _viewCount < newFirstIndex || oldFirstIndex > newFirstIndex + _viewCount)
					{
						//no intersection, render all
						for (int i = 0; i < _viewCount; i++)
						{
							GObject obj = GetChildAt(i);
							if (obj is GButton)
								((GButton)obj).selected = false;
						}

						RenderItems(0, _viewCount);
					}
					else if (oldFirstIndex > newFirstIndex)
					{
						int j1 = oldFirstIndex - newFirstIndex;
						int j2 = _viewCount - j1;
						for (int i = j2 - 1; i >= 0; i--)
						{
							GObject obj1 = GetChildAt(i);
							GObject obj2 = GetChildAt(i + j1);
							if (obj2 is GButton)
								((GButton)obj2).selected = false;
							float tmp = obj1.x;
							obj1.x = obj2.x;
							obj2.x = tmp;
							SwapChildrenAt(i + j1, i);
						}

						RenderItems(0, j1);
					}
					else
					{
						int j1 = newFirstIndex - oldFirstIndex;
						int j2 = _viewCount - j1;
						for (int i = 0; i < j2; i++)
						{
							GObject obj1 = GetChildAt(i);
							GObject obj2 = GetChildAt(i + j1);
							if (obj1 is GButton)
								((GButton)obj1).selected = false;
							float tmp = obj1.x;
							obj1.x = obj2.x;
							obj2.x = tmp;
							SwapChildrenAt(i + j1, i);
						}

						RenderItems(j2, _viewCount);
					}
				}

				if (this.childrenRenderOrder == ChildrenRenderOrder.Arch)
				{
					float mid = this.scrollPane.posX + this.viewWidth / 2;
					float minDist = int.MaxValue, dist;
					int apexIndex = 0;
					for (int i = 0; i < _viewCount; i++)
					{
						GObject obj = GetChildAt(i);
						if (obj.visible)
						{
							dist = Mathf.Abs(mid - obj.x - obj.width / 2);
							if (dist < minDist)
							{
								minDist = dist;
								apexIndex = i;
							}
						}
					}
					this.apexIndex = apexIndex;
				}
			}

			_boundsChanged = false;
		}

		override protected internal void GetSnappingPosition(ref float xValue, ref float yValue)
		{
			if (_virtual)
			{
				if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
				{
					int i = Mathf.FloorToInt(yValue / (_itemSize.y + _lineGap));
					if (yValue > i * (_itemSize.y + _lineGap) + _itemSize.y / 2)
						i++;

					yValue = i * (_itemSize.y + _lineGap);
				}
				else
				{
					int i = Mathf.FloorToInt(xValue / (_itemSize.x + _columnGap));
					if (xValue > i * (_itemSize.x + _columnGap) + _itemSize.x / 2)
						i++;

					xValue = i * (_itemSize.x + _columnGap);
				}
			}
			else
				base.GetSnappingPosition(ref xValue, ref yValue);
		}

		override protected void UpdateBounds()
		{
			int cnt = _children.Count;
			int i;
			GObject child;
			float curX = 0;
			float curY = 0;
			float cw, ch;
			float maxWidth = 0;
			float maxHeight = 0;

			for (i = 0; i < cnt; i++)
			{
				child = GetChildAt(i);
				child.EnsureSizeCorrect();
			}

			if (_layout == ListLayoutType.SingleColumn)
			{
				for (i = 0; i < cnt; i++)
				{
					child = GetChildAt(i);
					if (!child.visible)
						continue;

					if (curY != 0)
						curY += _lineGap;
					child.y = curY;
					curY += child.height;
					if (child.width > maxWidth)
						maxWidth = child.width;
				}
				cw = curX + maxWidth;
				ch = curY;
			}
			else if (_layout == ListLayoutType.SingleRow)
			{
				for (i = 0; i < cnt; i++)
				{
					child = GetChildAt(i);
					if (!child.visible)
						continue;

					if (curX != 0)
						curX += _columnGap;
					child.x = curX;
					curX += child.width;
					if (child.height > maxHeight)
						maxHeight = child.height;
				}
				cw = curX;
				ch = curY + maxHeight;
			}
			else if (_layout == ListLayoutType.FlowHorizontal)
			{
				int j = 0;
				float viewWidth = this.viewWidth;
				for (i = 0; i < cnt; i++)
				{
					child = GetChildAt(i);
					if (!child.visible)
						continue;

					if (curX != 0)
						curX += _columnGap;

					if (_lineItemCount != 0 && j >= _lineItemCount
						|| _lineItemCount == 0 && curX + child.width > viewWidth && maxHeight != 0)
					{
						//new line
						curX -= _columnGap;
						if (curX > maxWidth)
							maxWidth = curX;
						curX = 0;
						curY += maxHeight + _lineGap;
						maxHeight = 0;
						j = 0;
					}
					child.SetXY(curX, curY);
					curX += child.width;
					if (child.height > maxHeight)
						maxHeight = child.height;
					j++;
				}
				ch = curY + maxHeight;
				cw = maxWidth;
			}
			else
			{
				int j = 0;
				float viewHeight = this.viewHeight;
				for (i = 0; i < cnt; i++)
				{
					child = GetChildAt(i);
					if (!child.visible)
						continue;

					if (curY != 0)
						curY += _lineGap;

					if (_lineItemCount != 0 && j >= _lineItemCount
						|| _lineItemCount == 0 && curY + child.height > viewHeight && maxWidth != 0)
					{
						curY -= _lineGap;
						if (curY > maxHeight)
							maxHeight = curY;
						curY = 0;
						curX += maxWidth + _columnGap;
						maxWidth = 0;
						j = 0;
					}
					child.SetXY(curX, curY);
					curY += child.height;
					if (child.width > maxWidth)
						maxWidth = child.width;
					j++;
				}
				cw = curX + maxWidth;
				ch = maxHeight;
			}

			SetBounds(0, 0, cw, ch);

			this.InvalidateBatchingState();
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			string str;
			string[] arr;

			str = xml.GetAttribute("layout");
			if (str != null)
				_layout = FieldTypes.ParseListLayoutType(str);
			else
				_layout = ListLayoutType.SingleColumn;

			str = xml.GetAttribute("selectionMode");
			if (str != null)
				selectionMode = FieldTypes.ParseListSelectionMode(str);
			else
				selectionMode = ListSelectionMode.Single;

			OverflowType overflow;
			str = xml.GetAttribute("overflow");
			if (str != null)
				overflow = FieldTypes.ParseOverflowType(str);
			else
				overflow = OverflowType.Visible;

			if (overflow == OverflowType.Scroll)
			{
				ScrollType scroll;
				str = xml.GetAttribute("scroll");
				if (str != null)
					scroll = FieldTypes.ParseScrollType(str);
				else
					scroll = ScrollType.Vertical;

				ScrollBarDisplayType scrollBarDisplay;
				str = xml.GetAttribute("scrollBar");
				if (str != null)
					scrollBarDisplay = FieldTypes.ParseScrollBarDisplayType(str);
				else
					scrollBarDisplay = ScrollBarDisplayType.Default;

				int scrollBarFlags = xml.GetAttributeInt("scrollBarFlags");

				Margin scrollBarMargin = new Margin();
				str = xml.GetAttribute("scrollBarMargin");
				if (str != null)
					scrollBarMargin.Parse(str);

				str = xml.GetAttribute("margin");
				if (str != null)
					_margin.Parse(str);

				string vtScrollBarRes = null;
				string hzScrollBarRes = null;
				arr = xml.GetAttributeArray("scrollBarRes");
				if (arr != null)
				{
					vtScrollBarRes = arr[0];
					hzScrollBarRes = arr[1];
				}

				SetupScroll(scrollBarMargin, scroll, scrollBarDisplay, scrollBarFlags, vtScrollBarRes, hzScrollBarRes);
			}
			else
			{
				SetupOverflow(overflow);
			}

			arr = xml.GetAttributeArray("clipSoftness");
			if (arr != null)
				this.clipSoftness = new Vector2(int.Parse(arr[0]), int.Parse(arr[1]));

			_lineGap = xml.GetAttributeInt("lineGap");
			_columnGap = xml.GetAttributeInt("colGap");
			_lineItemCount = xml.GetAttributeInt("lineItemCount");
			defaultItem = xml.GetAttribute("defaultItem");

			autoResizeItem = xml.GetAttributeBool("autoItemSize", true);

			XMLList col = xml.Elements("item");
			foreach (XML ix in col)
			{
				string url = ix.GetAttribute("url");
				if (string.IsNullOrEmpty(url))
					url = defaultItem;
				if (string.IsNullOrEmpty(url))
					continue;

				GObject obj = GetFromPool(url);
				if (obj != null)
				{
					AddChild(obj);
					if (obj is GButton)
					{
						((GButton)obj).title = ix.GetAttribute("title");
						((GButton)obj).icon = ix.GetAttribute("icon");
					}
					else if (obj is GLabel)
					{
						((GLabel)obj).title = ix.GetAttribute("title");
						((GLabel)obj).icon = ix.GetAttribute("icon");
					}
				}
			}
		}
	}
}

using System.Collections.Generic;
using DG.Tweening;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	class GearSizeValue
	{
		public float width;
		public float height;
		public float scaleX;
		public float scaleY;

		public GearSizeValue(float width, float height, float scaleX, float scaleY)
		{
			this.width = width;
			this.height = height;
			this.scaleX = scaleX;
			this.scaleY = scaleY;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearSize : GearBase
	{
		Dictionary<string, GearSizeValue> _storage;
		GearSizeValue _default;
		Tweener _tweener;

		public GearSize(GObject owner)
			: base(owner)
		{

		}

		protected override void Init()
		{
			_default = new GearSizeValue(_owner.width, _owner.height, _owner.scaleX, _owner.scaleY);
			_storage = new Dictionary<string, GearSizeValue>();
		}

		override protected void AddStatus(string pageId, string value)
		{
			string[] arr = value.Split(jointChar0);
			GearSizeValue gv;
			if (pageId == null)
				gv = _default;
			else
			{
				gv = new GearSizeValue(0, 0, 1, 1);
				_storage[pageId] = gv;
			}
			gv.width = int.Parse(arr[0]);
			gv.height = int.Parse(arr[1]);
			if (arr.Length > 2)
			{
				gv.scaleX = float.Parse(arr[2]);
				gv.scaleY = float.Parse(arr[3]);
			}
		}

		override public void Apply()
		{
			GearSizeValue gv;
			bool ct = this.connected;
			if (ct)
			{
				if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
					gv = _default;
			}
			else
				gv = _default;

			if (_tweener != null)
				_tweener.Kill(true);

			if (tween && UIPackage._constructing == 0 && !disableAllTweenEffect
				&& ct && pageSet.ContainsId(_controller.previousPageId))
			{
				bool a = gv.width != _owner.width || gv.height != _owner.height;
				bool b = gv.scaleX != _owner.scaleX || gv.scaleY != _owner.scaleY;
				if (a || b)
				{
					_owner.internalVisible++;
					_tweener = DOTween.To(() => new Vector4(_owner.width, _owner.height, _owner.scaleX, _owner.scaleY), v =>
					{
						_owner._gearLocked = true;
						if (a)
							_owner.SetSize(v.x, v.y, _owner.gearXY.controller == _controller);
						if (b)
							_owner.SetScale(v.z, v.w);
						_owner._gearLocked = false;
					}, new Vector4(gv.width, gv.height, gv.scaleX, gv.scaleY), tweenTime)
					.SetEase(easeType)
					.SetUpdate(true)
					.OnComplete(() =>
					{
						_tweener = null;
						_owner.internalVisible--;
						_owner.InvalidateBatchingState();
					});

					if (delay > 0)
						_tweener.SetDelay(delay);
				}
			}
			else
			{
				_owner._gearLocked = true;
				_owner.SetSize(gv.width, gv.height, _owner.gearXY.controller == _controller);
				_owner.SetScale(gv.scaleX, gv.scaleY);
				_owner._gearLocked = false;
			}
		}

		override public void UpdateState()
		{
			if (_owner._gearLocked)
				return;

			if (connected)
			{
				GearSizeValue gv;
				if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
					_storage[_controller.selectedPageId] = new GearSizeValue(_owner.width, _owner.height, _owner.scaleX, _owner.scaleY);
				else
				{
					gv.width = _owner.width;
					gv.height = _owner.height;
					gv.scaleX = _owner.scaleX;
					gv.scaleY = _owner.scaleY;
				}
			}
			else
			{
				_default.width = _owner.width;
				_default.height = _owner.height;
				_default.scaleX = _owner.scaleX;
				_default.scaleY = _owner.scaleY;
			}
		}

		internal void UpdateFromRelations(float dx, float dy)
		{
			if (_storage != null)
			{
				foreach (GearSizeValue gv in _storage.Values)
				{
					gv.width += dx;
					gv.height += dy;
				}
				_default.width += dx;
				_default.height += dy;

				UpdateState();
			}
		}
	}
}

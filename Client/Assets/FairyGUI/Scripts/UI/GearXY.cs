using System.Collections.Generic;
using DG.Tweening;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	class GearXYValue
	{
		public float x;
		public float y;

		public GearXYValue(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearXY : GearBase
	{
		Dictionary<string, GearXYValue> _storage;
		GearXYValue _default;
		Tweener _tweener;

		public GearXY(GObject owner)
			: base(owner)
		{
		}

		protected override void Init()
		{
			_default = new GearXYValue(_owner.x, _owner.y);
			_storage = new Dictionary<string, GearXYValue>();
		}

		override protected void AddStatus(string pageId, string value)
		{
			string[] arr = value.Split(jointChar0);
			if (pageId == null)
			{
				_default.x = int.Parse(arr[0]);
				_default.y = int.Parse(arr[1]);
			}
			else
				_storage[pageId] = new GearXYValue(int.Parse(arr[0]), int.Parse(arr[1]));
		}

		override public void Apply()
		{
			GearXYValue gv;
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
				if (_owner.x != gv.x || _owner.y != gv.y)
				{
					_owner.internalVisible++;
					_tweener = DOTween.To(() => new Vector2(_owner.x, _owner.y), v =>
					{
						_owner._gearLocked = true;
						_owner.SetXY(v.x, v.y);
						_owner._gearLocked = false;
					}, new Vector2(gv.x, gv.y), tweenTime)
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
				_owner.SetXY(gv.x, gv.y);
				_owner._gearLocked = false;
			}
		}

		override public void UpdateState()
		{
			if (_owner._gearLocked)
				return;

			if (connected)
			{
				GearXYValue gv;
				if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
					_storage[_controller.selectedPageId] = new GearXYValue(_owner.x, _owner.y);
				else
				{
					gv.x = _owner.x;
					gv.y = _owner.y;
				}
			}
			else
			{
				_default.x = _owner.x;
				_default.y = _owner.y;
			}
		}

		internal void UpdateFromRelations(float dx, float dy)
		{
			if (_storage != null)
			{
				foreach (GearXYValue gv in _storage.Values)
				{
					gv.x += dx;
					gv.y += dy;
				}
				_default.x += dx;
				_default.y += dy;

				UpdateState();
			}
		}
	}
}

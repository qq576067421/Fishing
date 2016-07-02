using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearDisplay : GearBase
	{
		public GearDisplay(GObject owner)
			: base(owner)
		{
		}

		override protected bool connected
		{
			get
			{
				if (_controller != null && !pageSet.isEmpty)
					return pageSet.ContainsId(_controller.selectedPageId);
				else
					return true;
			}
		}

		override protected void AddStatus(string pageId, string value)
		{
		}

		override protected void Init()
		{
		}
		override public void Apply()
		{
			if (connected)
				_owner.internalVisible++;
			else
				_owner.internalVisible = 0;
		}

		override public void UpdateState()
		{
		}
	}
}

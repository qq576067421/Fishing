using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class InputEvent
	{
		/// <summary>
		/// x position in stage coordinates.
		/// </summary>
		public float x { get; internal set; }

		/// <summary>
		/// y position in stage coordinates.
		/// </summary>
		public float y { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public KeyCode keyCode { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public EventModifiers modifiers { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public int mouseWheelDelta { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public int touchId { get; internal set; }

		internal int clickCount;

		internal void Reset()
		{
			touchId = -1;
			x = 0;
			y = 0;
			clickCount = 0;
			keyCode = KeyCode.None;
			modifiers = 0;
			mouseWheelDelta = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 position
		{
			get { return new Vector2(x, y); }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDoubleClick
		{
			get { return clickCount > 1; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool ctrl
		{
			get
			{
				RuntimePlatform rp = Application.platform;
				bool isMac = (
					rp == RuntimePlatform.OSXEditor ||
					rp == RuntimePlatform.OSXPlayer ||
					rp == RuntimePlatform.OSXWebPlayer);

				return isMac ?
					((modifiers & EventModifiers.Command) != 0) :
					((modifiers & EventModifiers.Control) != 0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool shift
		{
			get
			{
				//return (modifiers & EventModifiers.Shift) != 0;
				return Stage.shiftDown;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool alt
		{
			get
			{
				return (modifiers & EventModifiers.Alt) != 0;
			}
		}
	}
}

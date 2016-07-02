using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class EventContext
	{
		public EventDispatcher sender { get; internal set; }
		public object initiator { get; internal set; }
		public string type;
		public object data;

		internal bool _defaultPrevented;
		internal bool _stopsPropagation;

		internal List<EventBridge> callChain = new List<EventBridge>();

		public void StopPropagation()
		{
			_stopsPropagation = true;
		}

		public void PreventDefault()
		{
			_defaultPrevented = true;
		}

		public bool isDefaultPrevented
		{
			get { return _defaultPrevented; }
		}

		public InputEvent inputEvent
		{
			get { return (InputEvent)data; }
		}

		static Stack<EventContext> pool = new Stack<EventContext>();
		internal static EventContext Get()
		{
			if (pool.Count > 0)
				return pool.Pop();
			else
				return new EventContext();
		}

		internal static void Return(EventContext value)
		{
			pool.Push(value);
		}
	}

}

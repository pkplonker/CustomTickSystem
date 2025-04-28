using System;

namespace CustomTick
{
	public static partial class TickManager
	{
		private class TickAction
		{
			public int Id;
			public Action Callback;
			public float Interval;
			public float Timer;
			public float DelayRemaining;

			public TickAction(int id, Action callback, float interval, float delay = 0f)
			{
				Id = id;
				Callback = callback;
				Interval = interval;
				Timer = interval;
				DelayRemaining = delay;
			}
		}
	}
}
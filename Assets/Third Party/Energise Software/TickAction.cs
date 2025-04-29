using System;

namespace CustomTick
{
	internal class TickAction : ITickItem
	{
		public int Id;
		public Action Callback;
		public float Interval;
		public float DelayRemaining;
		public float Timer;
		public bool OneShot;
		private bool paused;

		public TickAction(int id, Action callback, float interval, float delay, bool oneShot, bool paused)
		{
			Id = id;
			Callback = callback;
			Interval = interval;
			Timer = interval;
			DelayRemaining = delay;
			OneShot = oneShot;
			this.paused = paused;
		}
		
		public bool IsValid() => Callback != null;
		public int GetId() => Id;
		public bool IsPaused() => paused;
		public void SetPaused(bool p) => paused = p;
		public bool IsOneShot() => OneShot;

		public bool ShouldTick(float deltaTime)
		{
			if (paused || !IsValid()) return false;

			if (DelayRemaining > 0f)
			{
				DelayRemaining -= deltaTime;
				return false;
			}

			Timer -= deltaTime;
			if (Timer <= 0f)
			{
				Timer = Interval;
				return true;
			}

			return false;
		}

		public void Execute()
		{
			Callback?.Invoke();
		}
	}
}
using System.Reflection;
using UnityEngine;

namespace CustomTick
{
	internal class TickMethodWithParams : ITickItem
	{
		public int Id;
		public MonoBehaviour Target;
		public MethodInfo Method;
		public float Interval;
		public float Timer;
		public float DelayRemaining;
		public object[] Parameters;
		public bool OneShot;
		private bool paused;

		public TickMethodWithParams(int id, MonoBehaviour target, MethodInfo method, float interval,
			object[] parameters, float delay, bool oneShot, bool paused)
		{
			Id = id;
			Target = target;
			Method = method;
			Parameters = parameters;
			Interval = interval;
			Timer = interval;
			DelayRemaining = delay;
			OneShot = oneShot;
			this.paused = paused;
		}

		public bool IsValid() => Target != null && Method != null;
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
			Method?.Invoke(Target, Parameters);
		}
	}
}
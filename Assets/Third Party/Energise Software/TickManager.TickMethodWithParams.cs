using System.Reflection;
using UnityEngine;

namespace CustomTick
{
	public static partial class TickManager
	{
		private class TickMethodWithParams
		{
			public int Id;
			public MonoBehaviour Target;
			public MethodInfo Method;
			public float Interval;
			public float Timer;
			public float DelayRemaining;
			public object[] Parameters;

			public TickMethodWithParams(int id, MonoBehaviour target, MethodInfo method, float interval, object[] parameters, float delay = 0f)
			{
				Id = id;
				Target = target;
				Method = method;
				Interval = interval;
				Timer = interval;
				DelayRemaining = delay;
				Parameters = parameters;
			}
		}
	}
}
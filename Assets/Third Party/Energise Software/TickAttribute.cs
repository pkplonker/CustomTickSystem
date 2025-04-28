using UnityEngine;
using System;

namespace CustomTick
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TickAttribute : Attribute
	{
		public float Interval { get; private set; }
		public float Delay { get; private set; }

		public TickAttribute(float interval, float delay = 0f)
		{
			Interval = Mathf.Max(interval, 0f);
			Delay = Mathf.Max(delay, 0f);
		}
	}
}
using UnityEngine;
using System;
using PlasticPipe.PlasticProtocol.Client;

namespace CustomTick
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TickAttribute : Attribute
	{
		public float Interval { get; private set; }

		public TickAttribute(float interval)
		{
			Interval = Mathf.Max(interval, 0f);
		}
	}
}
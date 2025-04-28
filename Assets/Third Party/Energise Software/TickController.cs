using System;
using UnityEngine;

namespace CustomTick
{
	public static class TickController
	{
		static TickController()
		{
			Debug.Log("Static constructor called");
		}

		public static CustomTickHandle RegisterAction(float interval, Action action)
		{
			return null;
		}
	}
}
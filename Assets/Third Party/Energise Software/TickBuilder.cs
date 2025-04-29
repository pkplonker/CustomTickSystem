using System;
using UnityEngine;

namespace CustomTick
{
	public class TickBuilder
	{
		// State
		private Action action;
		private MonoBehaviour target;
		private string methodName;
		private object[] parameters;

		private float interval = 1f;
		private float delay = 0f;
		private bool oneShot = false;
		private bool paused = false;

		private bool isAction;
		private bool isMethodWithParams;

		private TickBuilder() { }

		// Entry point for Action
		public static TickBuilder Create(Action action)
		{
			return new TickBuilder
			{
				action = action,
				isAction = true
			};
		}

		// Entry point for method + params
		public static TickBuilder Create(MonoBehaviour target, string methodName, object[] parameters = null)
		{
			return new TickBuilder
			{
				target = target,
				methodName = methodName,
				parameters = parameters,
				isMethodWithParams = true
			};
		}

		// Fluent settings
		public TickBuilder SetInterval(float seconds)
		{
			interval = Mathf.Max(seconds, 0.001f);
			return this;
		}

		public TickBuilder SetDelay(float seconds)
		{
			delay = Mathf.Max(seconds, 0f);
			return this;
		}

		public TickBuilder SetOneShot(bool value = true)
		{
			oneShot = value;
			return this;
		}

		public TickBuilder SetPaused(bool value = true)
		{
			paused = value;
			return this;
		}

		public TickHandle Register()
		{
			if (isAction)
			{
				return TickManager.Register(action, interval, delay, oneShot, paused);
			}

			if (isMethodWithParams)
			{
				return TickManager.Register(target, methodName, parameters, interval, delay, oneShot, paused);
			}

			Debug.LogWarning("TickBuilder: No valid registration target.");
			return default;
		}
	}
}
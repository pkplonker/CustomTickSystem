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
		private float delay;
		private bool oneShot;
		private bool paused;

		private bool isAction;
		private bool isMethodWithParams;
#if UNITY_EDITOR
		private string description;
#endif

		private TickBuilder() { }

		// Entry point for Action
		public static TickBuilder Create(Action action) =>
			new()
			{
				action = action,
				isAction = true
			};

		// Entry point for method + params
		public static TickBuilder Create(MonoBehaviour target, string methodName, object[] parameters = null) =>
			new()
			{
				target = target,
				methodName = methodName,
				parameters = parameters,
				isMethodWithParams = true
			};

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
#if UNITY_EDITOR
			string finalDescription = description;
#endif

			if (isAction)
			{
#if UNITY_EDITOR
				if (string.IsNullOrEmpty(finalDescription) && action != null)
				{
					var method = action.Method;
					var targetName = method.DeclaringType?.Name ?? "Anon";
					finalDescription = $"{targetName}.{method.Name}";
				}
#endif
				return TickManager.Register(action, interval, delay, oneShot, paused
#if UNITY_EDITOR
					, finalDescription
#endif
				);
			}

			if (isMethodWithParams)
			{
#if UNITY_EDITOR
				if (string.IsNullOrEmpty(finalDescription) && target != null && !string.IsNullOrEmpty(methodName))
				{
					finalDescription = $"{target.GetType().Name}.{methodName}";
				}
#endif
				return TickManager.Register(target, methodName, parameters, interval, delay, oneShot, paused
#if UNITY_EDITOR
					, finalDescription
#endif
				);
			}

			Debug.LogWarning("TickBuilder: Invalid builder usage.");
			return default;
		}

		public TickBuilder SetDescription(string desc)
		{
#if UNITY_EDITOR
			description = desc;
#endif
			return this;
		}
	}
}
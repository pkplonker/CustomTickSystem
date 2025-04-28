using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;

namespace CustomTick
{
	[DefaultExecutionOrder(-10000)]
	public static class TickManager
	{
		private class TickMethod
		{
			public MonoBehaviour Target;
			public MethodInfo Method;
			public float Interval;
			public float Timer;

			public TickMethod(MonoBehaviour target, MethodInfo method, float interval)
			{
				Target = target;
				Method = method;
				Interval = interval;
				Timer = interval;
			}
		}

		private static List<TickMethod> tickMethods = new ();
		private static bool initialized = false;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			if (initialized) return;
			initialized = true;

			ScanScene();

			var loop = PlayerLoop.GetCurrentPlayerLoop();
			InsertCustomUpdate(ref loop);
			PlayerLoop.SetPlayerLoop(loop);
		}

		private static void InsertCustomUpdate(ref PlayerLoopSystem loop)
		{
			var updateSystem = new PlayerLoopSystem
			{
				type = typeof(TickManager),
				updateDelegate = Update
			};

			var subsystems = new List<PlayerLoopSystem>(loop.subSystemList);
			subsystems.Insert(0, updateSystem);
			loop.subSystemList = subsystems.ToArray();
		}

		private static void Update()
		{
			float deltaTime = Time.deltaTime;

			for (int i = 0; i < tickMethods.Count; i++)
			{
				var tickMethod = tickMethods[i];
				if (tickMethod.Target == null) continue;

				tickMethod.Timer -= deltaTime;
				if (tickMethod.Timer <= 0f)
				{
					try
					{
						tickMethod.Method.Invoke(tickMethod.Target, null);
						tickMethod.Timer = tickMethod.Interval;
					}
					catch (Exception e)
					{
						Debug.LogException(e);
					}
					
				}
			}
		}

		private static void ScanScene()
		{
			MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);

			foreach (var behaviour in behaviours)
			{
				if (behaviour == null) continue;

				var methods = behaviour.GetType()
					.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				foreach (var method in methods)
				{
					var tickAttr = method.GetCustomAttribute<TickAttribute>();
					if (tickAttr != null)
					{
						if (method.GetParameters().Length == 0)
						{
							tickMethods.Add(new TickMethod(behaviour, method, tickAttr.Interval));
						}
						else
						{
							Debug.LogWarning(
								$"[Tick] method '{method.Name}' on '{behaviour.name}' must have no parameters.");
						}
					}
				}
			}
		}
	}
}
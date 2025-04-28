using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

namespace CustomTick
{
	[DefaultExecutionOrder(-10000)]
	public static partial class TickManager
	{
		private static List<TickMethod> tickMethods = new();
		private static List<TickAction> tickActions = new();
		private static List<TickMethodWithParams> tickMethodsWithParams = new();

		private static bool initialized = false;
		private static int nextId = 1;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			if (initialized) return;
			initialized = true;

			ScanScene();
			HookPlayerLoop();
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			RescanAll();
		}

		private static void RescanAll()
		{
			tickMethods.Clear();
			ScanScene();
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
							int id = nextId++; // 🔥 Assign new unique ID!
							tickMethods.Add(new TickMethod(id, behaviour, method, tickAttr.Interval, tickAttr.Delay));
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

		private static void HookPlayerLoop()
		{
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

			// Clean up destroyed objects
			for (int i = tickMethods.Count - 1; i >= 0; i--)
			{
				if (tickMethods[i].Target == null)
				{
					tickMethods.RemoveAt(i);
				}
			}

			// Tick attribute methods
			for (int i = 0; i < tickMethods.Count; i++)
			{
				var tickMethod = tickMethods[i];

				if (tickMethod.DelayRemaining > 0f)
				{
					tickMethod.DelayRemaining -= deltaTime;
					continue;
				}

				tickMethod.Timer -= deltaTime;
				if (tickMethod.Timer <= 0f)
				{
					tickMethod.Method.Invoke(tickMethod.Target, null);
					tickMethod.Timer = tickMethod.Interval;
				}
			}

			// Tick manual actions
			for (int i = 0; i < tickActions.Count; i++)
			{
				var tickAction = tickActions[i];

				if (tickAction.DelayRemaining > 0f)
				{
					tickAction.DelayRemaining -= deltaTime;
					continue;
				}

				tickAction.Timer -= deltaTime;
				if (tickAction.Timer <= 0f)
				{
					tickAction.Callback?.Invoke();
					tickAction.Timer = tickAction.Interval;
				}
			}

			for (int i = tickMethodsWithParams.Count - 1; i >= 0; i--)
			{
				var tickMethod = tickMethodsWithParams[i];

				if (tickMethod.Target == null)
				{
					tickMethodsWithParams.RemoveAt(i);
					continue;
				}

				if (tickMethod.DelayRemaining > 0f)
				{
					tickMethod.DelayRemaining -= deltaTime;
					continue;
				}

				tickMethod.Timer -= deltaTime;
				if (tickMethod.Timer <= 0f)
				{
					tickMethod.Method.Invoke(tickMethod.Target, tickMethod.Parameters);
					tickMethod.Timer = tickMethod.Interval;
				}
			}
		}

		public static TickHandle Register(Action callback, float interval, float delay = 0f)
		{
			if (callback == null || interval <= 0f)
			{
				Debug.LogWarning("Invalid Tick registration.");
				return default;
			}

			int id = nextId++;
			tickActions.Add(new TickAction(id, callback, interval, delay));

			return new TickHandle {Id = id, Type = TickType.Action};
		}

		public static TickHandle Register(MonoBehaviour target, string methodName, object[] parameters, float interval,
			float delay = 0f)
		{
			if (target == null || string.IsNullOrEmpty(methodName) || interval <= 0f)
			{
				Debug.LogWarning("Invalid Tick registration with parameters.");
				return default;
			}

			var method = target.GetType().GetMethod(methodName,
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (method == null)
			{
				Debug.LogWarning($"Method '{methodName}' not found on {target.name}.");
				return default;
			}

			int id = nextId++;
			tickMethodsWithParams.Add(new TickMethodWithParams(id, target, method, interval, parameters, delay));

			return new TickHandle {Id = id, Type = TickType.MethodWithParams};
		}

		public static void Unregister(TickHandle handle)
		{
			if (!handle.IsValid)
				return;

			switch (handle.Type)
			{
				case TickType.Action:
					for (int i = tickActions.Count - 1; i >= 0; i--)
					{
						if (tickActions[i].Id == handle.Id)
						{
							tickActions.RemoveAt(i);
							return;
						}
					}

					break;
				case TickType.Method:
					for (int i = tickMethods.Count - 1; i >= 0; i--)
					{
						if (tickMethods[i].Id == handle.Id)
						{
							tickMethods.RemoveAt(i);
							return;
						}
					}

					break;
				case TickType.MethodWithParams:
					for (int i = tickMethodsWithParams.Count - 1; i >= 0; i--)
					{
						if (tickMethodsWithParams[i].Id == handle.Id)
						{
							tickMethodsWithParams.RemoveAt(i);
							return;
						}
					}

					break;
			}
		}

		public struct TickHandle
		{
			internal int Id;
			internal TickType Type;

			public bool IsValid => Id != 0;
		}

		internal enum TickType
		{
			Action,
			Method,
			MethodWithParams
		}
	}
}
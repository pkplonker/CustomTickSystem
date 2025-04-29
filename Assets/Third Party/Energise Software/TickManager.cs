using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

namespace CustomTick
{
	[DefaultExecutionOrder(-10000)]
	internal static partial class TickManager
	{
		private static List<TickMethod> tickMethods = new();
		private static List<TickAction> tickActions = new();
		private static List<TickMethodWithParams> tickMethodsWithParams = new();
		private static Dictionary<float, TickGroup> tickGroups = new();

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
							int id = nextId++;
							var tickItem = new TickMethod(
								id,
								behaviour,
								method,
								tickAttr.Interval,
								tickAttr.Delay,
								oneShot: false,
								paused: false
							);

							if (!tickGroups.TryGetValue(tickAttr.Interval, out var group))
							{
								group = new TickGroup();
								tickGroups.Add(tickAttr.Interval, group);
							}

							group.Items.Add(tickItem);
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

			foreach (var group in tickGroups.Values)
			{
				var items = group.Items;

				for (int i = items.Count - 1; i >= 0; i--)
				{
					var item = items[i];

					if (item.ShouldTick(deltaTime))
					{
						item.Execute();

						if (item.IsOneShot())
						{
							items.RemoveAt(i);
						}
					}
					else if (!item.IsValid())
					{
						items.RemoveAt(i);
					}
				}
			}
		}

		public static TickHandle Register(Action callback, float interval, float delay = 0f, bool oneShot = false,
			bool paused = false)
		{
			if (callback == null || interval <= 0f)
			{
				Debug.LogWarning("Invalid Tick registration.");
				return default;
			}

			int id = nextId++;
			var tickItem = new TickAction(id, callback, interval, delay, oneShot, paused);

			if (!tickGroups.TryGetValue(interval, out var group))
			{
				group = new TickGroup();
				tickGroups.Add(interval, group);
			}

			group.Items.Add(tickItem);

			var handle = new TickHandle {Id = id};
#if UNITY_EDITOR
			handle.Type = TickType.Action;
#endif
			return handle;
		}

		public static TickHandle Register(MonoBehaviour target, string methodName, object[] parameters, float interval,
			float delay = 0f, bool oneShot = false, bool paused = false)
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
			var tickItem = new TickMethodWithParams(id, target, method, interval, parameters, delay, oneShot, paused);

			if (!tickGroups.TryGetValue(interval, out var group))
			{
				group = new TickGroup();
				tickGroups.Add(interval, group);
			}

			group.Items.Add(tickItem);

			var handle = new TickHandle {Id = id};
#if UNITY_EDITOR
			handle.Type = TickType.Action;
#endif
			return handle;
		}

		public static void Unregister(TickHandle handle)
		{
			if (!handle.IsValid)
				return;

			foreach (var group in tickGroups.Values)
			{
				var items = group.Items;
				for (int i = items.Count - 1; i >= 0; i--)
				{
					if (items[i].GetId() == handle.Id)
					{
						items.RemoveAt(i);
						return;
					}
				}
			}
		}
	}
}
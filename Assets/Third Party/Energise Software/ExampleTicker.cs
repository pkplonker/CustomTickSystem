using System;
using UnityEngine;

namespace CustomTick
{
	public class ExampleTicker : MonoBehaviour
	{
		private void Start()
		{
			Debug.Log("Start");
		}

		[Tick(1.0f)] // called every 1 second
		private void TickEverySecond()
		{
			Debug.Log($"Tick every second at {Time.time}");
		}

		[Tick(0.5f)] // called every 0.5 seconds
		private void TickTwiceASecond()
		{
			Debug.Log($"Tick twice a second at {Time.time}");
		}
	}
}
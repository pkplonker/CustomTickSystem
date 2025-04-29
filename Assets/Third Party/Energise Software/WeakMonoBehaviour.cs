using UnityEngine;

namespace CustomTick
{
	internal struct WeakMonoBehaviour
	{
		private Object target;
		private int cachedInstanceId;

		public WeakMonoBehaviour(Object target)
		{
			this.target = target;
			cachedInstanceId = target ? target.GetInstanceID() : 0;
		}

		public bool IsAlive()
		{
			if (target == null) return false;
			return target.GetInstanceID() == cachedInstanceId;
		}

		public MonoBehaviour AsMonoBehaviour()
		{
			return target as MonoBehaviour;
		}
	}
}
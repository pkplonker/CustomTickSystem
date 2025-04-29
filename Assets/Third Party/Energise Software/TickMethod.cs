using System.Reflection;
using UnityEngine;

namespace CustomTick
{
	internal class TickMethod : TickItemBase
	{
		private WeakMonoBehaviour weakTarget;
		private MethodInfo method;

		public TickMethod(int id, MonoBehaviour target, MethodInfo method, float interval, float delay, bool oneShot, bool paused)
			: base(id, interval, delay, oneShot, paused)
		{
			this.weakTarget = new WeakMonoBehaviour(target);
			this.method = method;
		}

		protected override bool ValidateTarget()
		{
			return weakTarget.IsAlive();
		}

		public override void Execute()
		{
			var target = weakTarget.AsMonoBehaviour();
			if (target != null)
			{
				method?.Invoke(target, null);
			}
		}
	}
}
using System.Reflection;
using UnityEngine;

namespace CustomTick
{
	internal class TickMethodWithParams : TickItemBase
	{
		private WeakMonoBehaviour weakTarget;
		private MethodInfo method;
		private object[] parameters;

		public TickMethodWithParams(int id, MonoBehaviour target, MethodInfo method, float interval, object[] parameters, float delay, bool oneShot, bool paused)
			: base(id, interval, delay, oneShot, paused)
		{
			this.weakTarget = new WeakMonoBehaviour(target);
			this.method = method;
			this.parameters = parameters;
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
				method?.Invoke(target, parameters);
			}
		}
	}

}
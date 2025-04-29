namespace CustomTick
{
	public struct TickHandle
	{
		internal int Id;

#if UNITY_EDITOR
		internal TickType Type;
#endif

		public bool IsValid => Id != 0;
	}
}
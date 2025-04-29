namespace CustomTick.Editor
{
	using UnityEngine;
	using UnityEditor;

	namespace CustomTick
	{
		public class TickManagerWindow : EditorWindow
		{
			private Vector2 scroll;

			[MenuItem("Window/Energise Software/Custom Tick")]
			public static void Open()
			{
				GetWindow<TickManagerWindow>("Tick Manager");
			}

			private void OnGUI()
			{
				if (!Application.isPlaying)
				{
					EditorGUILayout.HelpBox("Enter Play Mode to view active ticks.", MessageType.Info);
					return;
				}

				scroll = EditorGUILayout.BeginScrollView(scroll);

				var groups = TickManager.EditorGetGroups();

				if (groups == null || groups.Count == 0)
				{
					EditorGUILayout.LabelField("No active ticks.");
					EditorGUILayout.EndScrollView();
					return;
				}

				foreach (var pair in groups)
				{
					float interval = pair.Key;
					var group = pair.Value;
					var items = group.Items;

					EditorGUILayout.LabelField($"Interval: {interval:0.000}s | Count: {items.Count}",
						EditorStyles.boldLabel);
					EditorGUI.indentLevel++;

					foreach (var item in items)
					{
						if (item == null) continue;

						var id = item.GetId().ToString();

						var status = item.IsValid() ? "Valid" : "Invalid";
						var paused = item.IsPaused() ? "Paused" : "Active";
						var once = item.IsOneShot() ? "OneShot" : "Loop";

						var label = $"[ID: {id}] [{paused}] [{once}] [{status}]";

#if UNITY_EDITOR
						if (TickManager.EditorTryGetType(item.GetId(), out var type))
						{
							label += $" ({type})";
						}
#endif
						EditorGUILayout.LabelField(label);
					}

					EditorGUI.indentLevel--;
					EditorGUILayout.Space(4);
				}

				EditorGUILayout.EndScrollView();
			}
		}
	}
}
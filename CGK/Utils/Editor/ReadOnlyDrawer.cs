using UnityEditor;
using UnityEngine;

namespace CGK.Utils.Editor
{
	[CustomPropertyDrawer(typeof(CGK.Utils.ReadOnly))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
		{
			bool guiEnabled = GUI.enabled;
			GUI.enabled = false;

			EditorGUI.PropertyField(position, property, label);

			GUI.enabled = guiEnabled;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, true);
		}
	}
}
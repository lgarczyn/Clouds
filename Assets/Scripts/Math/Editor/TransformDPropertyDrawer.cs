using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TransformD))]
public class TransformDDrawer : PropertyDrawer
{
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return EditorGUI.GetPropertyHeight(property);
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginDisabledGroup(true);
    EditorGUI.PropertyField(position, property, label, true);
    EditorGUI.EndDisabledGroup();
  }
}
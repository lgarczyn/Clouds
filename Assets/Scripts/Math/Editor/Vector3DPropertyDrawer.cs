using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Vector3D))]
public class Vector3DDrawer : PropertyDrawer
{
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return EditorGUIUtility.singleLineHeight * (EditorGUIUtility.wideMode ? 1 : 2);
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    // Find the SerializedProperties by name
    var xProp = property.FindPropertyRelative(nameof(Vector3D.x));
    var yProp = property.FindPropertyRelative(nameof(Vector3D.y));
    var zProp = property.FindPropertyRelative(nameof(Vector3D.z));

    // Using BeginProperty / EndProperty on the parent property means that
    // prefab override logic works on the entire property.
    EditorGUI.BeginProperty(position, label, property);
    {
      // // Makes the fields disabled / grayed out
      // EditorGUI.BeginDisabledGroup(true);
      // {
      EditorGUI.Vector3Field(position, label, new Vector3(
        xProp.floatValue, yProp.floatValue, zProp.floatValue));
      // }
      // EditorGUI.EndDisabledGroup();
    }
    EditorGUI.EndProperty();
  }
}
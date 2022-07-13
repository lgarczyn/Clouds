using UnityEngine;
using UnityEditor;

// using OneLine;

// [CustomPropertyDrawer(typeof(Vector3D))]
// public class Vector3DDrawer : OneLinePropertyDrawer
// {
// }

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
    var x = property.FindPropertyRelative(nameof(Vector3D.x));
    var y = property.FindPropertyRelative(nameof(Vector3D.y));
    var z = property.FindPropertyRelative(nameof(Vector3D.z));
    Vector3D value = new Vector3D(x.doubleValue, y.doubleValue, z.doubleValue);

    // Using BeginProperty / EndProperty on the parent property means that
    // prefab override logic works on the entire property.
    EditorGUI.BeginProperty(position, label, property);
    {
      var sublabels = new GUIContent[] {
        new GUIContent("X"),
      new GUIContent("Y"),
      new GUIContent("Z") };

      EditorGUI.MultiPropertyField(position, sublabels, x, label);
    }
    EditorGUI.EndProperty();
  }
}
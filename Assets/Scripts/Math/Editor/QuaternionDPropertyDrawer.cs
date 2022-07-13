using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(QuaternionD))]
public class QuaternionDDrawer : PropertyDrawer
{
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return EditorGUIUtility.singleLineHeight * (EditorGUIUtility.wideMode ? 1 : 2);
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    // Find the SerializedProperties by name
    var x = property.FindPropertyRelative(nameof(QuaternionD.x));
    var y = property.FindPropertyRelative(nameof(QuaternionD.y));
    var z = property.FindPropertyRelative(nameof(QuaternionD.z));
    var w = property.FindPropertyRelative(nameof(QuaternionD.w));

    QuaternionD value = new QuaternionD(x.doubleValue, y.doubleValue, z.doubleValue, w.doubleValue);

    // Using BeginProperty / EndProperty on the parent property means that
    // prefab override logic works on the entire property.
    EditorGUI.BeginProperty(position, label, property);
    {
      var sublabels = new GUIContent[] {
        new GUIContent("X"),
      new GUIContent("Y"),
      new GUIContent("Z"),
      new GUIContent("W") };

      EditorGUI.MultiPropertyField(position, sublabels, x, label);
    }
    EditorGUI.EndProperty();
  }
}
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BoundedCurveAttribute))]
public class BoundedCurveDrawer : PropertyDrawer
{
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return EditorGUIUtility.singleLineHeight * 1;
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    BoundedCurveAttribute boundedCurve = (BoundedCurveAttribute)attribute;

    property.animationCurveValue = EditorGUI.CurveField(
      position,
      label,
      property.animationCurveValue,
      Color.white,
      boundedCurve.bounds
     );
  }
}
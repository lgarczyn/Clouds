/// Source: https://forum.unity.com/threads/a-different-requirecomponent.468618/
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RequiredComponentAttribute))]
public class RequiredComponentDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginDisabledGroup(true);
    EditorGUI.PropertyField(position, property);
    EditorGUI.EndDisabledGroup();

    MonoBehaviour mono = property.serializedObject.targetObject as MonoBehaviour;
    if (typeof(Component).IsAssignableFrom(fieldInfo.FieldType))
    {
      Component comp = mono.GetComponent(fieldInfo.FieldType);

      if (property.objectReferenceValue == null)
      {
        if (comp == null)
        {
          comp = mono.gameObject.AddComponent(fieldInfo.FieldType);
        }

        property.objectReferenceValue = comp;
        //property.serializedObject.ApplyModifiedProperties(); // not sure if this is really needed
      } else {
        if (comp != property.objectReferenceValue) {
          Debug.LogError("Field <b>" + fieldInfo.Name + "</b> of " + mono.GetType() + " had the wrong component stored!", mono);
        }
      }
    }
    else
    {
      Debug.LogError("Field <b>" + fieldInfo.Name + "</b> of " + mono.GetType() + " is not a component!", mono);
    }
  }
}

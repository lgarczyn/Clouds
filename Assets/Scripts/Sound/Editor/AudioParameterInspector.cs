using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace Sound.Editor
{
  [CustomEditor(typeof(AudioParameter))]
  public class AudioParameterInspector : UnityEditor.Editor
  {
    SerializedProperty _parameterIdProp;
    SerializedProperty _emitterProp;

    void OnEnable()
    {
      _parameterIdProp = serializedObject.FindProperty("parameterId");
      _emitterProp = serializedObject.FindProperty("emitter");
    }

    static List<EditorParamRef> GetParameters(AudioEmitter emitter)
    {
      return EventManager.EventFromGUID(emitter.EventReference.Guid).Parameters;
    }

    static void DeserializeParameterID(SerializedProperty property, out ParameterId result)
    {
      result.data = property.FindPropertyRelative("data").ulongValue;
    }

    static void SerializeParameterID(SerializedProperty property, ParameterId value)
    {
      property.FindPropertyRelative("data").ulongValue = value.data;
    }

    void DrawDropdown()
    {
      // Get and check the emitter associated with the parameter
      AudioEmitter emitter;
      {
        emitter = _emitterProp.objectReferenceValue as AudioEmitter;
        if (emitter == null) return;
        if (emitter.EventReference.Guid.IsNull) return;
      }

      // Get the id property
      // Begin property, for prefab displaying
      EditorGUI.BeginProperty(GetControlRect(true, 0), new GUIContent(), _parameterIdProp);
      // Get the actual serialized id
      DeserializeParameterID(_parameterIdProp, out ParameterId paramId);
      // Get all the potential parameters
      List<EditorParamRef> options = GetParameters(emitter);
      // Find which one is currently selected
      int selectedIndex = options.FindIndex(p => p.ID.Equals(paramId));
      // Disable if no options is available
      if (options.Count == 0)
      {
        EditorGUI.BeginDisabledGroup(true);
        PropertyField(_parameterIdProp);
        EditorGUI.EndDisabledGroup();
      }
      else
      {
        // Display the dropdown with the option names
        int newSelectedIndex = Popup(
          "Parameter",
          selectedIndex,
          options.Select(o => o.Name).ToArray());
        EditorGUI.EndDisabledGroup();

        // Update the property with the selected parameter ID
        if (newSelectedIndex != selectedIndex)
        {
          SerializeParameterID(_parameterIdProp, (PARAMETER_ID)options[newSelectedIndex].ID);
          serializedObject.ApplyModifiedProperties();
        }
      }
      // End property
      EditorGUI.EndProperty();
    }

    public override void OnInspectorGUI()
    {
      // Load serializedProperties
      serializedObject.Update();
      
      // Draw script field for consistency
      EditorGUI.BeginDisabledGroup(true);
      EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
      EditorGUI.EndDisabledGroup();

      // Draw the event parameter dropdown
      DrawDropdown();

      // Check for changes in other properties
      EditorGUI.BeginChangeCheck();

      DrawPropertiesExcluding(serializedObject, "parameterId", "m_Script");

      // If changes happened, apply
      if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
  }
}
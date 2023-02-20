using System;
using System.Linq;
using System.Reflection;
using Unity.Entities.Conversion;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityAtoms.InputSystem;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

// TODO make sure to keep fileId on regen
// TODO actually clean
namespace Atoms
{
  [CreateAssetMenu]
  [ExecuteAlways]
  public class InterpreterGenerator : ScriptableObject
  {
    [SerializeField] InputActionAsset actionAsset;
    [SerializeField] bool regenOnValidate = true;

    void OnValidate()
    {
      if (!regenOnValidate) return;
      Clear();
      Generate();
    }

    public void Generate()
    {
      // PlayerInput system = PlayerInput.all[0];
      if (actionAsset)
        foreach (InputActionMap map in actionAsset.actionMaps)
          foreach (InputAction action in map.actions)
            StoreCallbackContextInterpreter($"On {map.name} {action.name}", action);

      AssetDatabase.SaveAssetIfDirty(this);

      // system.actionEvents = new ReadOnlyArray<PlayerInput.ActionEvent>();

      // ReadOnlyArray<PlayerInput.ActionEvent> oldEvents = system.actionEvents;

      // Debug.Log(res);   
    }

    // Clear all sub-assets of this instance
    public void Clear()
    {
      string path = AssetDatabase.GetAssetPath(this);

      while (true)
      {
        BaseAtom child = AssetDatabase.LoadAssetAtPath<BaseAtom>(path);
        if (child)
          AssetDatabase.RemoveObjectFromAsset(child);
        else break;
      }

      AssetDatabase.SaveAssetIfDirty(this);
    }

    // Store an object as a sub-asset of this asset
    ScriptableObject StoreObject(Object o)
    {
      if (string.IsNullOrEmpty(o.name)) throw new Exception("Can't store without name");

      string path = AssetDatabase.GetAssetPath(this);

      Object oldStoredVersion = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(a => a.name == o.name);

      if (oldStoredVersion)
      {
        // oldStoredVersion.FindProperty("m_Script").objectReferenceValue = o.FindProperty("m_Script")...
        // type checking? cancel if type is same
        // don't care at all about keeping interpreters, so I can write them every time
        // but I really want to keep events, even if they change
        // if i lose interpreters, I have to rewrite actions
        // if i have to rewrite actions, I have to ensure I keep the same PlayerInput prefab
        // how can I ensure the local event is local to the player prefab, not just the PlayerInput subprefab
      }
      AssetDatabase.AddObjectToAsset(o, this);

      Object storedVersion = AssetDatabase.LoadAllAssetsAtPath(path).First(a => a.name == o.name);

      return (ScriptableObject)storedVersion;
    }

    ScriptableObject GetEvent(string eventName, string control, bool isButton)
    {
      ScriptableObject res = isButton
        ? CreateInstance<BoolVariable>()
        : control switch
        {
          "Analog" => CreateInstance<FloatEvent>(),
          "Axis" => CreateInstance<FloatEvent>(),
          "Quaternion" => CreateInstance<QuaternionEvent>(),
          "Stick" => CreateInstance<Vector2Event>(),
          "Vector2" => CreateInstance<Vector2Event>(),
          "Vector3" => CreateInstance<Vector3Event>(),
          _ => throw new ArgumentOutOfRangeException()
        };
      res.name = eventName;
      return res;
    }

    ScriptableObject GetInterpreter(string eventName, string control)
    {
      ScriptableObject res = control switch
      {
        "Analog" => CreateInstance<FloatCallbackContextInterpreter>(),
        "Axis" => CreateInstance<FloatCallbackContextInterpreter>(),
        "Button" => CreateInstance<FloatCallbackContextInterpreter>(),
        "Quaternion" => CreateInstance<QuaternionCallbackContextInterpreter>(),
        "Stick" => CreateInstance<Vector2CallbackContextInterpreter>(),
        "Vector2" => CreateInstance<Vector2CallbackContextInterpreter>(),
        "Vector3" => CreateInstance<Vector3CallbackContextInterpreter>(),
        _ => throw new ArgumentOutOfRangeException()
      };
      res.name = eventName + " Interpreter";
      return res;
    }

    // Create a new interpreter and its associated event
    ScriptableObject StoreCallbackContextInterpreter(string eventName, InputAction action)
    {
      bool isButton = action.type == InputActionType.Button
        || action.expectedControlType == "Button";
      // Create a new event
      ScriptableObject eventObject = StoreObject(GetEvent(eventName, action.expectedControlType, isButton));

      // Create a new interpreter to update said FloatEvent
      ScriptableObject interpreter = GetInterpreter(eventName, action.expectedControlType);

      SerializedObject serializedObject = new(interpreter);

      if (isButton)
      {
        serializedObject.FindProperty("_valueAsButton").objectReferenceValue = eventObject;
      }
      else
      {
        serializedObject.FindProperty("_started").objectReferenceValue = eventObject;
        serializedObject.FindProperty("_performed").objectReferenceValue = eventObject;
        serializedObject.FindProperty("_canceled").objectReferenceValue = eventObject;
      }

      serializedObject.ApplyModifiedPropertiesWithoutUndo();

      return StoreObject(interpreter);
    }

    // Create an actionEvent to be stored in a PlayerInput
    PlayerInput.ActionEvent GetActionEvent(InputAction a, ScriptableObject interpreter)
    {
      PlayerInput.ActionEvent ae = new(a);

      MethodInfo targetInfo = UnityEventBase.GetValidMethodInfo(interpreter,
        "Interpret", new[] { typeof(InputAction.CallbackContext) });

      UnityAction<InputAction.CallbackContext> methodDelegate =
        Delegate.CreateDelegate(typeof(UnityAction<InputAction.CallbackContext>), interpreter, targetInfo) as
          UnityAction<InputAction.CallbackContext>;

      UnityEventTools.AddPersistentListener(ae, methodDelegate);

      return ae;
    }
  }
}

// Analog float
// Axis float
// Button bool
// Quaternion Quaternion
// Stick Vector2
// Vector2 Vector2
// Vector3 Vector3
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Currently
// does not set interpreter events, probably because wrong id
namespace Atoms
{
  [CustomEditor(typeof(InterpreterGenerator))]
  public class InterpreterGeneratorDrawer : Editor
  {
    public override VisualElement CreateInspectorGUI()
    {
      VisualElement root = new VisualElement();

      IMGUIContainer defaultInspector = new(() => DrawDefaultInspector());
      defaultInspector.SetEnabled(false);
      root.Add(defaultInspector);

      InterpreterGenerator gen = target as InterpreterGenerator;
      if (gen == null) return root;

      {
        Button but = new Button(() =>
        {
          Debug.Log("CLEARING");
          gen.Clear();
        })
        {
          text = "Clear"
        };
        root.Add(but);
      }
      {
        Button but = new Button(() =>
        {
          Debug.Log("GENERATING");
          gen.Generate();
        })
        {
          text = "Generate"
        };
        root.Add(but);
      }

      return root;
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
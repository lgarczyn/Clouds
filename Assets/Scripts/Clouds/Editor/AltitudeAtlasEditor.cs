using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(AltitudeAtlas))]
public class AltitudeAtlasEditor : UnityEditor.Editor
{

  AltitudeAtlas altitude;

  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Label("Error: " + altitude.meanSquareError);
    GUILayout.Label("Output:");
    // Create an empty label 30px high
    GUILayoutOption height = GUILayout.Height(30f);
    GUILayout.Label("", new GUILayoutOption[] { height });
    // Get the bounds of said label
    var rect = GUILayoutUtility.GetLastRect();
    // Draw it
    GUI.DrawTexture(rect, altitude.altitudeAtlas);

    // Draw each output curve
    foreach (AnimationCurve curve in altitude.outputCurves)
    {
      EditorGUILayout.CurveField(curve);
    }
  }

  void OnEnable()
  {
    altitude = (AltitudeAtlas)target;
  }

}
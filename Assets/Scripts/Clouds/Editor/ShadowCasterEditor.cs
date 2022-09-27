using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShadowCameraMatrix))]
public class ShadowCameraMatrixEditor : UnityEditor.Editor
{

  ShadowCameraMatrix shadowCaster;

  bool folded = true;

  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    folded = EditorGUILayout.Foldout(folded, "Preview");

    if (folded)
    {
      GUILayout.Label(shadowCaster.GetComponent<Camera>().activeTexture);
    }
  }

  void OnEnable()
  {
    shadowCaster = (ShadowCameraMatrix)target;
  }
}
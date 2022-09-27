using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FaunaSpawner))]
public class FaunaSpawnerEditor : Editor
{

  FaunaSpawner spawner;

  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    if (Application.isPlaying && GUILayout.Button("Update"))
    {
      spawner.Reset();
    }
  }

  void OnEnable()
  {
    spawner = (FaunaSpawner)target;
  }

}
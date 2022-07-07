using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AltitudeAtlas))]
public class AltitudeAtlasEditor : UnityEditor.Editor
{

  AltitudeAtlas altitude;

  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    altitude.UpdateMap();
  }

  void OnEnable()
  {
    altitude = (AltitudeAtlas)target;
  }

}
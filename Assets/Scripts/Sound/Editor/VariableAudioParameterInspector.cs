using System;
using System.Linq;
using FMODUnity;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace Sound.Editor
{
  [CustomEditor(typeof(VariableAudioParameter))]
  public class VariableAudioParameterInspector : AudioParameterInspector
  {
  }
}
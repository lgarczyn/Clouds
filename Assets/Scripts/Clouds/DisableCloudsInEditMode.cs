using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DisableCloudsInEditMode : MonoBehaviour
{
  public Material target;

  void Update()
  {
    if (!target) return;
    target.SetInt("enabled", Application.isPlaying ? 1 : 0);
  }
}

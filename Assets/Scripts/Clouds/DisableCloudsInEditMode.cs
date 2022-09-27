using UnityEngine;

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

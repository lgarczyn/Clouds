using UnityEngine;

[ExecuteInEditMode]
public class DisableCloudsInEditMode : MonoBehaviour
{
  public Material target;

  void Update()
  {
    if (!target) return;
    int newValue = Application.isPlaying ? 1 : 0;
    if (target.HasInt("enabled") == false || target.GetInt("enabled") != newValue)
      target.SetInt("enabled", newValue);
  }
}

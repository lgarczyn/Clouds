using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ToggleClouds : MonoBehaviour
{
  public Camera shadowCamera;
  public Camera cloudsCamera;
  public int cloudRendererIndex;

  public bool cloudsEnabled;

  [SerializeField][RequiredComponent] CloudMaster reqCloudMaster;

  public void SetEnabled(bool value)
  {
    cloudsEnabled = value;
    UpdateEnabled();
  }

  void UpdateEnabled()
  {
    this.shadowCamera.enabled = cloudsEnabled;
    this.cloudsCamera.GetUniversalAdditionalCameraData().SetRenderer(
      cloudsEnabled ? cloudRendererIndex : 0
    );
  }

  void Start()
  {
    SetEnabled(cloudsEnabled);
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.M))
    {
      cloudsEnabled = !cloudsEnabled;
      UpdateEnabled();
    }
  }
}

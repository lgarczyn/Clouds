using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ToggleClouds : MonoBehaviour
{
  public int cloudRendererIndex;

  public bool cloudsEnabled;

  [SerializeField][RequiredComponent] CloudMaster reqCloudMaster;
  [SerializeField][RequiredComponent] MainCameraBridge reqShadowCameraBridge;
  [SerializeField][RequiredComponent] MainCameraBridge reqMainCameraBridge;

  public void SetEnabled(bool value)
  {
    cloudsEnabled = value;
    UpdateEnabled();
  }

  void UpdateEnabled()
  {
    reqShadowCameraBridge.instance.camera.enabled = cloudsEnabled;
    reqMainCameraBridge.instance.camera.GetUniversalAdditionalCameraData().SetRenderer(
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

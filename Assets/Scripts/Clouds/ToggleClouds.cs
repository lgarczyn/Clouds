using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class ToggleClouds : MonoBehaviour
{
  public int cloudRendererIndex;

  public bool cloudsEnabled;

  [SerializeField][RequiredComponent] CloudMaster reqCloudMaster;
  [SerializeField][RequiredComponent] ShadowCameraBridge reqShadowCameraBridge;
  [SerializeField][RequiredComponent] MainCameraBridge reqMainCameraBridge;

  public void SetEnabled(bool value)
  {
    cloudsEnabled = value;
    UpdateEnabled();
  }

  void UpdateEnabled()
  {
    reqShadowCameraBridge.instance.shadowCamera.enabled = cloudsEnabled;
    reqMainCameraBridge.instance.mainCamera.GetUniversalAdditionalCameraData().SetRenderer(
      cloudsEnabled ? cloudRendererIndex : 0
    );
  }

  void Start()
  {
    SetEnabled(cloudsEnabled);
  }

  void Update()
  {
    if (Keyboard.current.mKey.wasPressedThisFrame)
    {
      cloudsEnabled = !cloudsEnabled;
      UpdateEnabled();
    }
  }
}

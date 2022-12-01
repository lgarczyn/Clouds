using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class ToggleClouds : MonoBehaviour
{
  public ScriptableRendererFeature cloudFeature;

  public bool cloudsEnabled;

  [SerializeField][RequiredComponent] CloudMaster reqCloudMaster;
  [SerializeField][RequiredComponent] ShadowCameraBridge reqShadowCameraBridge;
  [SerializeField][RequiredComponent] MainCameraBridge reqMainCameraBridge;


  void Start()
  {
    SetEnabled(cloudsEnabled);
  }

  public void OnToggleClouds(InputAction.CallbackContext context)
  {
    Debug.Log("please" + context);
    if (context.phase != InputActionPhase.Performed) return;
    Toggle();
  }

  public void SetEnabled(bool value)
  {
    cloudsEnabled = value;
    UpdateEnabled();
  }

  public void Toggle() {
    SetEnabled(!cloudsEnabled);
  }

  void UpdateEnabled()
  {
    reqShadowCameraBridge.instance.shadowCamera.enabled = cloudsEnabled;
    cloudFeature.SetActive(cloudsEnabled);
  }
}

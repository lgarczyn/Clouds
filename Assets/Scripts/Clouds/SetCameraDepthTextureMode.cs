using UnityEngine;

[ExecuteAlways]
public class SetCameraDepthTextureMode : MonoBehaviour
{

  [SerializeField] private DepthTextureMode depthTextureMode = DepthTextureMode.Depth;

  [SerializeField][RequiredComponent] Camera reqCamera;

#if UNITY_EDITOR
  private void Update()
  {
    // Makes sure the editor view is also set to depth
    if (Camera.current)
    {
      Camera.current.depthTextureMode |= depthTextureMode;
    }
  }
#else
    void OnEnable() {
        reqCamera.depthTextureMode |= depthTextureMode;
    }
#endif
}
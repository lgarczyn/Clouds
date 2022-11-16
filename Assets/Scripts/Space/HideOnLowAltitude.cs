using UnityEngine;

public class HideOnLowAltitude : MonoBehaviour
{
  public double minHeight;

  [SerializeField][RequiredComponent] Renderer reqRenderer;

  [SerializeField][RequiredComponent] LocalSpaceControllerBridge reqLocalSpaceControllerBridge;

  // Update is called once per frame
  void Update()
  {
    var localSpace = reqLocalSpaceControllerBridge.instance;
    reqRenderer.enabled = localSpace.altitude > minHeight;
  }
}

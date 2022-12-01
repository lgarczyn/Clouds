//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;

[RequireComponent(typeof(MouseFlightControllerBridge))]
[RequireComponent(typeof(MainCameraBridge))]
[RequireComponent(typeof(CanvasGroup))]
public class Hud : MonoBehaviour
{
  [Header("Components")]
  [RequiredComponent][SerializeField] MouseFlightControllerBridge reqMouseFlightControllerBridge;
  [RequiredComponent][SerializeField] MainCameraBridge reqMainCameraBridge;
  [RequiredComponent][SerializeField] CanvasGroup reqCanvasGroup;

  [Header("HUD Elements")]
  [SerializeField] private RectTransform boresight = null;
  [SerializeField] private RectTransform mousePos = null;

  private void LateUpdate()
  {
    UpdateGraphics();
  }

  private void UpdateGraphics()
  {
    MouseFlightController controller = reqMouseFlightControllerBridge.tryInstance;
    Camera playerCam = reqMainCameraBridge.instance.mainCamera;

    reqCanvasGroup.alpha = controller == null ? 0f : 1f;
    if (controller == null) return;

    if (boresight != null)
    {
      boresight.position = playerCam.WorldToScreenPoint(controller.BoresightPos);
      boresight.gameObject.SetActive(boresight.position.z > 1f);
    }

    if (mousePos != null)
    {
      mousePos.position = playerCam.WorldToScreenPoint(controller.RealMouseAimPos);
      mousePos.gameObject.SetActive(mousePos.position.z > 1f);
    }
  }
}

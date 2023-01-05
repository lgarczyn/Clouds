//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Combination of camera rig and controller for aircraft. Requires a properly set
/// up rig. I highly recommend either using or referencing the included prefab.
/// </summary>
[RequireComponent(typeof(MainCameraBridge))]
[RequireComponent(typeof(PlayerManagerBridge))]
public class MouseFlightController : Manager<MouseFlightController>
{
  [Header("Components")]
  [SerializeField]
  [Tooltip("Transform of the object the mouse rotates to generate MouseAim position")]
  private Transform mouseAim = null;
  [SerializeField]
  [Tooltip("Transform of the object on the rig which the camera is attached to")]
  private Transform cameraRig = null;

  [SerializeField][RequiredComponent] MainCameraBridge reqMainCameraBridge;
  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

  [Header("Options")]
  // TODO clean this
  // should be private and serialized
  // offset multiplier and accessor should be here too
  public Vector3 offset;

  [SerializeField]
  [Tooltip("How quickly the camera tracks the mouse aim point.")]
  private float camSmoothSpeed = 5f;

  [SerializeField]
  [Tooltip("Mouse sensitivity for the mouse flight target")]
  private float mouseSensitivity = 3f;

  [SerializeField]
  [Tooltip("How far the boresight and mouse flight are from the aircraft")]
  private float aimDistance = 500f;

  [Space]
  [SerializeField]
  [Tooltip("How far the boresight and mouse flight are from the aircraft")]
  private bool showDebugInfo = false;

  private Vector3 frozenDirection = Vector3.forward;
  private Vector2 input = Vector2.zero;
  private bool isMouseAimFrozen = false;
  private bool isCameraFrozen = false;

  public void OnFreezeCamera(InputAction.CallbackContext context)
  {
    isCameraFrozen = context.ReadValueAsButton();
  }

  public void OnFreezeMouseAim(InputAction.CallbackContext context)
  {
    isMouseAimFrozen = context.ReadValueAsButton();
    if (isMouseAimFrozen) frozenDirection = mouseAim.forward;
    else mouseAim.forward = frozenDirection;
  }

  public void OnAim(InputAction.CallbackContext context)
  {
    input = context.ReadValue<Vector2>() * mouseSensitivity;
  }

  /// <summary>
  /// Get a point along the aircraft's boresight projected out to aimDistance meters.
  /// Useful for drawing a crosshair to aim fixed forward guns with, or to indicate what
  /// direction the aircraft is pointed.
  /// </summary>
  public Vector3 BoresightPos
  {
    get
    {
      Transform aircraft = reqPlayerManagerBridge.instance.transform;
      return (aircraft.transform.forward * aimDistance) + aircraft.transform.position;
    }
  }

  /// <summary>
  /// Get the position that the mouse is indicating the aircraft should fly, projected
  /// out to aimDistance meters. Also meant to be used to draw a mouse cursor.
  /// </summary>
  public Vector3 MouseAimPos
  {
    get
    {
      if (mouseAim != null)
      {
        return isMouseAimFrozen
            ? GetFrozenMouseAimPos()
            : mouseAim.position + (mouseAim.forward * aimDistance);
      }
      else
      {
        return transform.forward * aimDistance;
      }
    }
  }

  /// <summary>
  /// Get the position that the mouse is indicating the aircraft should fly, projected
  /// out to aimDistance meters. Also meant to be used to draw a mouse cursor.
  /// </summary>
  public Vector3 RealMouseAimPos
  {
    get
    {
      if (mouseAim != null)
      {
        return mouseAim.position + (mouseAim.forward * aimDistance);
      }
      else
      {
        return transform.forward * aimDistance;
      }
    }
  }

  protected override void OnEnable()
  {
    base.OnEnable();

    if (mouseAim == null)
      Debug.LogError(name + "MouseFlightController - No mouse aim transform assigned!");
    if (cameraRig == null)
      Debug.LogError(name + "MouseFlightController - No camera rig transform assigned!");

    mouseAim.forward = reqPlayerManagerBridge.playerTransform.forward;

    // To work correctly, the entire rig must not be parented to anything.
    // When parented to something (such as an aircraft) it will inherit those
    // rotations causing unintended rotations as it gets dragged around.
    transform.parent = null;

    if (!Application.isEditor)
    {
      Cursor.lockState = CursorLockMode.Confined;
      Cursor.visible = false;
    }

    cameraRig.rotation = Quaternion.LookRotation(mouseAim.forward, cameraRig.up);
  }

  private void Update()
  {
    UpdateCameraPos();

    RotateRig();

    reqPlayerManagerBridge.playerPlane.SetTarget(MouseAimPos);
  }

  private void FixedUpdate()
  {
    UpdateCameraPos();

    reqPlayerManagerBridge.playerPlane.SetTarget(MouseAimPos);
  }

  void LateUpdate()
  {
    if (isCameraFrozen) return;

    Transform cam = reqMainCameraBridge.instance.mainCamera.transform;

    cam.position = cameraRig.position;
    cam.rotation = cameraRig.rotation;
    cam.position += cam.forward * offset.z;
    cam.position += cam.up * offset.y;
    cam.position += cam.right * offset.x;
  }

  private void RotateRig()
  {
    if (mouseAim == null || cameraRig == null)
      return;

    // Mouse input
    var mouseDelta = input;

    Transform cam = reqMainCameraBridge.instance.mainCamera.transform;

    // Rotate the aim target that the plane is meant to fly towards.
    // Use the camera's axes in world space so that mouse motion is intuitive.
    mouseAim.Rotate(cam.right, -mouseDelta.y, Space.World);
    mouseAim.Rotate(cam.up, mouseDelta.x, Space.World);

    // The up vector of the camera normally is aligned to the horizon. However, when
    // looking straight up/down this can feel a bit weird. At those extremes, the camera
    // stops aligning to the horizon and instead aligns to itself.
    Vector3 upVec = (Mathf.Abs(mouseAim.forward.y) > 0.9f) ? cameraRig.up : Vector3.up;

    // Smoothly rotate the camera to face the mouse aim.
    cameraRig.rotation = Damp(cameraRig.rotation,
                              Quaternion.LookRotation(mouseAim.forward, upVec),
                              camSmoothSpeed,
                              Time.deltaTime);

    reqPlayerManagerBridge.playerPlane.SetTarget(mouseAim.localPosition);
  }

  private Vector3 GetFrozenMouseAimPos()
  {
    if (mouseAim != null)
      return mouseAim.position + (frozenDirection * aimDistance);
    else
      return transform.forward * aimDistance;
  }

  private void UpdateCameraPos()
  {
    Transform aircraft = reqPlayerManagerBridge.instance.transform;
    // Move the whole rig to follow the aircraft.
    transform.position = aircraft.position;
  }

  // Thanks to Rory Driscoll
  // http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
  /// <summary>
  /// Creates dampened motion between a and b that is framerate independent.
  /// </summary>
  /// <param name="a">Initial parameter</param>
  /// <param name="b">Target parameter</param>
  /// <param name="lambda">Smoothing factor</param>
  /// <param name="dt">Time since last damp call</param>
  /// <returns></returns>
  private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
  {
    return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
  }

  private void OnDrawGizmos()
  {
    if (showDebugInfo == true)
    {
      Color oldColor = Gizmos.color;

      // Draw the boresight position.
      Transform aircraft = reqPlayerManagerBridge.instance.transform;
      Gizmos.color = Color.white;
      Gizmos.DrawWireSphere(BoresightPos, 10f);

      if (mouseAim != null)
      {
        // Draw the position of the mouse aim position.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(MouseAimPos, 10f);

        // Draw axes for the mouse aim transform.
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(mouseAim.position, mouseAim.forward * 50f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(mouseAim.position, mouseAim.up * 50f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(mouseAim.position, mouseAim.right * 50f);
      }

      Gizmos.color = oldColor;
    }
  }
}

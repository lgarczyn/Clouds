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
  [SerializeField] Vector3 offset;
  public float offsetDistanceMultiplier = 1f;

  [SerializeField]
  [Tooltip("How quickly the camera tracks the mouse aim point.")]
  private float camSmoothSpeed = 5f;

  [SerializeField]
  [Tooltip("Mouse sensitivity for the mouse flight target")]
  private float mouseSensitivity = 3f;

  [SerializeField]
  [Tooltip("Controller sensitivity for the mouse flight target")]
  private float controllerSensitivity = 3f;

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

  public void OnAimController(InputAction.CallbackContext context)
  {
    input = context.ReadValue<Vector2>() * controllerSensitivity;
  }

  public void OnOverrideControls(InputAction.CallbackContext context)
  {
    reqPlayerManagerBridge.playerPlane.SetOverride(context.ReadValue<Vector2>());
  }

  /// <summary>
  /// Get a point along the aircraft's boresight projected out to aimDistance meters.
  /// Useful for drawing a crosshair to aim fixed forward guns with, or to indicate what
  /// direction the aircraft is pointed.
  /// </summary>
  public Vector3 BoresightDir
  {
    get => reqPlayerManagerBridge.instance.transform.forward;
  }

  /// <summary>
  /// Get the position that the mouse is indicating the aircraft should fly, projected
  /// out to aimDistance meters. Also meant to be used to draw a mouse cursor.
  /// </summary>
  public Vector3 MouseAimDir
  {
    get
    {
      if (mouseAim != null)
      {
        return isMouseAimFrozen
            ? GetFrozenMouseAimDir()
            : mouseAim.forward;
      }
      else
      {
        return transform.forward;
      }
    }
  }

  /// <summary>
  /// Get the position that the mouse is indicating the aircraft should fly, projected
  /// out to aimDistance meters. Also meant to be used to draw a mouse cursor.
  /// </summary>
  public Vector3 RealMouseAimDir
  {
    get
    {
      if (mouseAim != null)
      {
        return mouseAim.forward;
      }
      else
      {
        return transform.forward;
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

    if (transform.parent.position != Vector3.zero
      || transform.parent.eulerAngles != Vector3.zero
      || transform.parent.lossyScale != Vector3.one)
    {
      Debug.LogError("MouseFlightController must not be parented by any moving parent", this);
    }

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

    reqPlayerManagerBridge.playerPlane.SetTarget(MouseAimDir);
  }

  private void FixedUpdate()
  {
    UpdateCameraPos();

    reqPlayerManagerBridge.playerPlane.SetTarget(MouseAimDir);
  }

  [SerializeField] float cameraBankSmoothTime = 1f;
  [SerializeField] float cameraBankDistance = 4f;
  [SerializeField] float cameraBankMaxSpeed = 4f;

  float currentBankVelocity = 0f;
  float currentBank = 0f;

  void LateUpdate()
  {
    if (isCameraFrozen) return;

    Transform cam = reqMainCameraBridge.instance.mainCamera.transform;
    
    Vector3 localMouseAimDir = cameraRig.transform
      .InverseTransformVector(MouseAimDir);

    float xStep = localMouseAimDir.x;
    // Oops
    if (!float.IsFinite(xStep)) xStep = 0;

    float sidestepTarget = xStep * cameraBankDistance;

    if (isMouseAimFrozen) sidestepTarget = 0f;

    currentBank = Mathf.SmoothDamp(
      currentBank,
      sidestepTarget,
      ref currentBankVelocity,
      cameraBankSmoothTime,
      cameraBankMaxSpeed
    );

    Vector3 totalOffset = (offset + Vector3.right * currentBank)
      * offsetDistanceMultiplier;

    cam.position = cameraRig.position;
    cam.rotation = cameraRig.rotation;
    cam.position += cameraRig.rotation * totalOffset;
  }

  private void RotateRig()
  {
    if (mouseAim == null || cameraRig == null)
      return;

    // Mouse input
    var mouseDelta = input;
    input = Vector2.zero;

    Transform cam = reqMainCameraBridge.instance.mainCamera.transform;

    // Rotate the aim target that the plane is meant to fly towards.
    // Use the camera's axes in world space so that mouse motion is intuitive.
    mouseAim.Rotate(cam.right, -mouseDelta.y, Space.World);
    mouseAim.Rotate(cam.up, mouseDelta.x, Space.World);

    // The up vector of the camera normally is aligned to the horizon. However, when
    // looking straight up/down this can feel a bit weird. At those extremes, the camera
    // stops aligning to the horizon and instead aligns to itself.
    Vector3 upVec = (Mathf.Abs(mouseAim.forward.y) > 0.6f) ? cameraRig.up : Vector3.up;

    // Smoothly rotate the camera to face the mouse aim.
    cameraRig.rotation = Damp(cameraRig.rotation,
                              Quaternion.LookRotation(mouseAim.forward, upVec),
                              camSmoothSpeed,
                              Time.deltaTime);

    reqPlayerManagerBridge.playerPlane.SetTarget(mouseAim.localPosition);
  }

  private Vector3 GetFrozenMouseAimDir()
  {
    return mouseAim != null ? frozenDirection : transform.forward;
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
      Gizmos.DrawWireSphere(transform.position + BoresightDir * 20, 10f);

      if (mouseAim != null)
      {
        // Draw the position of the mouse aim position.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + BoresightDir * 20, 10f);

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

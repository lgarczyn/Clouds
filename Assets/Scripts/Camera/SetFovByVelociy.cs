using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SetFovByVelociy : MonoBehaviour
{
  public AnimationCurve fovByVelocity;
  public float adjustTime = 1;
  public Rigidbody target;

  private float adjustSpeed = 0f;

  void Update()
  {
    float velocity = target.velocity.magnitude;
    float value = fovByVelocity.Evaluate(velocity);
    Camera camera = GetComponent<Camera>();
    float current = camera.fieldOfView;

    float dampedValue = Mathf.SmoothDamp(current, value, ref adjustSpeed, adjustTime);

    camera.fieldOfView = dampedValue;
  }
}

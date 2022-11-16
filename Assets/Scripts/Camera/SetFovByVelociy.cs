using UnityEngine;

public class SetFovByVelociy : MonoBehaviour
{
  public AnimationCurve fovByVelocity;
  public float adjustTime = 1;
  public Rigidbody target;

  private float adjustSpeed = 0f;

  [SerializeField][RequiredComponent] Camera reqCamera;

  void Update()
  {
    float velocity = target.velocity.magnitude;
    float value = fovByVelocity.Evaluate(velocity);
    float current = reqCamera.fieldOfView;

    float dampedValue = Mathf.SmoothDamp(current, value, ref adjustSpeed, adjustTime);

    reqCamera.fieldOfView = dampedValue;
  }
}

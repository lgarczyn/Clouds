using UnityEngine;

public class SetFovByVelociy : MonoBehaviour
{
  public AnimationCurve fovByVelocity;
  public float adjustTime = 1;

  private float adjustSpeed = 0f;

  [SerializeField][RequiredComponent] Camera reqCamera;
  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

  void Update()
  {
    float velocity = reqPlayerManagerBridge.playerRigidbody.velocity.magnitude;
    float value = fovByVelocity.Evaluate(velocity);
    float current = reqCamera.fieldOfView;

    float dampedValue = Mathf.SmoothDamp(current, value, ref adjustSpeed, adjustTime);

    reqCamera.fieldOfView = dampedValue;
  }
}

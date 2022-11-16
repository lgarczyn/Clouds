using UnityEngine;

public class WhaleController : FaunaController
{
  Vector3 velocity = Vector3.forward;
  public float diveCounter = 0f;

  public float dirChangeSpeed = 0.1f;
  public float speed = 0.2f;
  public float rotationSpeed = 0.1f;
  public float diveDuration = 2f;
  public float diveSpeed = 0.1f;
  public float maxHeight = 10000f;
  public float minHeight = -10000f;

  protected override void OnRepop()
  {
    velocity = Random.insideUnitSphere * speed;
    reqRigidbody.rotation = Quaternion.LookRotation(velocity, Vector3.up);
  }

  void FixedUpdate()
  {
    // Apply random change to velocity
    Vector3 velocityChange = Random.insideUnitSphere * dirChangeSpeed;
    // Add drag
    velocity *= velocity.sqrMagnitude > 1f ? 0.9f : 1.1f;
    // Calculate new velocity plus the dive bias
    velocity += (velocityChange + GetDiveVelocity()) * Time.fixedDeltaTime;

    Rigidbody r = reqRigidbody;

    // Move forward
    r.MovePosition(r.position + velocity * Time.fixedDeltaTime * speed);
    // Rotate forward
    r.MoveRotation(
        Quaternion.Slerp(
            r.rotation,
            Quaternion.LookRotation(velocity, Vector3.up),
            Time.fixedDeltaTime * rotationSpeed));
  }

  // Calculate velocity bias to stay in container
  Vector3 GetDiveVelocity()
  {
    Rigidbody r = reqRigidbody;

    // If outside of bounds, set a pool of velocity to be spread over diveDuration
    float newCounter =
        r.position.y > maxHeight ? -1f :
        r.position.y < minHeight ? 1f :
        0f;

    newCounter *= diveDuration / Time.fixedDeltaTime;

    // If the previous pool is closer to 0, replace it
    if (Mathf.Abs(newCounter) > Mathf.Abs(diveCounter))
      diveCounter = newCounter;

    Vector3 returnVelocity = Vector3.up * Mathf.Clamp(diveCounter, -1f, 1f) * diveSpeed;

    // If the pool is more than tick time, reduce it
    if (Mathf.Abs(diveCounter) > 1f)
      diveCounter -= Mathf.Sign(diveCounter);
    else
      diveCounter = 0f;

    return returnVelocity;
  }
}

using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class Missile : MonoBehaviour
{
  [Header("Components")]
  [SerializeField] private Target target = null;

  [Header("Physics")]
  [SerializeField] private PIDController3D controller;

  public float rotateSpeed = 10f;

  [Range(0f, 3f)]
  public float parameterVariation = 0.5f;

  [Range(0.1f, 1f)]
  public float velocityLimitingFactor = 0.9f;

  public float pidPositionAccuracy = 100f;
  [Range(-1, 1)]
  public float aggressivity = 0f;

  private Vector3 targetPosition;

  private float GetRandomVariation()
  {
    return Random.Range(1 / (1 + parameterVariation), 1 + parameterVariation);
  }

  private void Start()
  {
    float v = parameterVariation;
    controller.Kp *= GetRandomVariation();
    controller.Ki *= GetRandomVariation();
    controller.Kd *= GetRandomVariation();
    controller.N *= GetRandomVariation();
    transform.position += Random.insideUnitSphere * 100;
  }

  private Vector3 GetPidTarget(Vector3 deltaPos)
  {
    if (deltaPos.magnitude < pidPositionAccuracy)
      return Vector3.zero;

    return deltaPos - deltaPos.normalized * pidPositionAccuracy;
  }

  private void FixedUpdate()
  {
    Rigidbody r = GetComponent<Rigidbody>();

    if (target.IsVisible(r.position))
    {
      Vector3 idealPos = target.position - target.velocity * aggressivity;
      Vector3 force = controller.Iterate(
        GetPidTarget(idealPos - r.position),
        Vector3.zero,
        Time.deltaTime
      );

      r.AddForce(force, ForceMode.Acceleration);
    }
    else
    {
      Vector3 force = controller.Iterate(
        Vector3.zero,
        Vector3.zero,
        Time.deltaTime
      );

      r.AddForce(force, ForceMode.Acceleration);
    }

    r.velocity *= Mathf.Pow(velocityLimitingFactor, Time.fixedDeltaTime);

    if (r.velocity.sqrMagnitude > 0f)
    {
      r.rotation = Quaternion.Slerp(
        r.rotation,
        Quaternion.LookRotation(r.velocity),
        Time.fixedDeltaTime * rotateSpeed);
    }
  }
}

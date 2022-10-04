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

  [Header("Behavior")]

  public float pidPositionAccuracy = 10f;

  public float searchPositionAccuracy = 100f;

  [Range(-1, 1)]
  public float aggressivity = 0f;

  private Vector3 lastTargetPosition;

  private float angularVelocity = 0;

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

  private Vector3 GetPidTarget(Vector3 deltaPos, float accuracy)
  {
    if (deltaPos.magnitude < pidPositionAccuracy)
      return Vector3.zero;

    return deltaPos - deltaPos.normalized * accuracy;
  }

  private void FixedUpdate()
  {
    Rigidbody r = GetComponent<Rigidbody>();

    if (target.IsVisible(r.position))
    {
      lastTargetPosition = target.position;
      Vector3 idealPos = target.position - target.velocity * aggressivity;
      Vector3 force = controller.Iterate(
        GetPidTarget(idealPos - r.position, pidPositionAccuracy),
        Vector3.zero,
        Time.deltaTime
      );

      r.AddForce(force, ForceMode.Acceleration);
    }
    else
    {
      Vector3 force = controller.Iterate(
        GetPidTarget(lastTargetPosition - r.position, searchPositionAccuracy),
        Vector3.zero,
        Time.deltaTime
      );

      r.AddForce(force, ForceMode.Acceleration);
    }

    r.velocity *= Mathf.Pow(velocityLimitingFactor, Time.fixedDeltaTime);

    if (r.velocity.sqrMagnitude > 0f)
    {
      Quaternion target = Quaternion.LookRotation(r.velocity);

      r.rotation = r.rotation.SmoothDampSimple(
        target,
        ref angularVelocity,
        rotateSpeed,
        float.PositiveInfinity,
        Time.fixedDeltaTime);
    }
  }
}

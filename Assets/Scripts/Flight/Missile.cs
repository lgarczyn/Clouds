using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TargetManagerBridge))]
public class Missile : MonoBehaviour
{
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

  [Range(0, 1)]
  public float aggressivityVariation = 0f;

  private Vector3 lastTargetPosition;

  private float temporaryTargetDuration = 0f;

  private Vector3 temporaryTarget;

  public void SetTempTarget(Vector3 target, float duration)
  {
    temporaryTargetDuration = duration;
    temporaryTarget = target;
  }

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

    aggressivity += Random.Range(-aggressivityVariation, -aggressivityVariation);
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

    // Get the plane target
    ITarget target = GetComponent<TargetManagerBridge>().instance.GetTarget();

    float accuracy;
    Vector3 targetPos;

    if (temporaryTargetDuration > 0f)
    {
      temporaryTargetDuration -= Time.fixedDeltaTime;
      targetPos = temporaryTarget;
      accuracy = 10f;
    }
    else if (target.IsVisible(r.position))
    {
      lastTargetPosition = target.position;

      targetPos = lastTargetPosition - target.velocity * aggressivity;
      accuracy = pidPositionAccuracy;
    }
    else
    {
      targetPos = lastTargetPosition;
      accuracy = searchPositionAccuracy;
    }

    Vector3 force = controller.Iterate(
      GetPidTarget(targetPos - r.position, accuracy),
      Vector3.zero,
      Time.deltaTime
    );

    r.AddForce(force, ForceMode.Acceleration);

    r.velocity *= Mathf.Pow(velocityLimitingFactor, Time.fixedDeltaTime);

    if (r.velocity.sqrMagnitude > 0f)
    {
      r.MoveRotation(Quaternion.LookRotation(r.velocity, r.rotation * Vector3.up));
    }
  }
}

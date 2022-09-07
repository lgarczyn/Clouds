using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class Missile : MonoBehaviour
{
  [Header("Components")]
  [SerializeField] private Transform target = null;

  [Header("Physics")]
  [SerializeField] private PIDController3D controller;

  public float rotateSpeed = 10f;

  [Range(0f, 3f)]
  public float parameterVariation = 0.5f;

  [Range(0.1f, 1f)]
  public float velocityLimitingFactor = 0.9f;

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

  private void FixedUpdate()
  {
    // Try to ignore target.position and only care for deltaPos
    // Try to use velocity in input
    Vector3 force = controller.Iterate(
      target.position - transform.position,
      Vector3.zero,
      Time.deltaTime
    );

    Rigidbody r = GetComponent<Rigidbody>();

    r.AddForce(force, ForceMode.Acceleration);

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

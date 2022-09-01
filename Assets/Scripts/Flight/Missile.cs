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
  public float parameterVariation = 0.5f;

  public float friendPushRadius = 100f;
  public float friendPushForce = 1f;

  [Range(0.1f, 1f)]
  public float velocityLimitingFactor = 0.9f;

  // public bool reset;

  private void Start()
  {
    float v = parameterVariation;
    controller.Kp += Random.Range(-v, v) * Random.Range(-v, v);
    controller.Ki += Random.Range(-v, v) * Random.Range(-v, v);
    controller.Kd += Random.Range(-v, v) * Random.Range(-v, v);
    controller.N += Random.Range(-v, v) * Random.Range(-v, v);
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

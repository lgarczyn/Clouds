using UnityEngine;

[RequireComponent(typeof(PlaneThrustController))]
[RequireComponent(typeof(Rigidbody))]
public class PlaneCheatController : MonoBehaviour
{
  [HideInInspector]
  public Vector3 startPos = Vector3.zero;
  [HideInInspector]
  public Quaternion startRotation = Quaternion.identity;

  void Start()
  {
    Rigidbody rigidbody = GetComponent<Rigidbody>();

    startPos = rigidbody.position;
    startRotation = rigidbody.rotation;
  }

  void Update()
  {
    PlaneThrustController thrustController = GetComponent<PlaneThrustController>();
    Rigidbody rigidbody = GetComponent<Rigidbody>();

    if (Input.GetKeyDown(KeyCode.L)) rigidbody.isKinematic = !rigidbody.isKinematic;
    if (Input.GetKeyDown(KeyCode.K))
    {
      rigidbody.position = startPos;
      rigidbody.rotation = startRotation;
      if (!rigidbody.isKinematic)
      {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
      }
    }
    if (Input.GetKeyDown(KeyCode.I))
    {
      FindObjectsOfType<MissileDeathController>().Do((m) => m.Damage(1, true));
    }
  }
}

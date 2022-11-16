using UnityEngine;

public class PlaneCheatController : MonoBehaviour
{
  [HideInInspector]
  public Vector3 startPos = Vector3.zero;
  [HideInInspector]
  public Quaternion startRotation = Quaternion.identity;

  [SerializeField][RequiredComponent] PlaneThrustController reqPlaneThrustController;

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  void Start()
  {
    startPos = reqRigidbody.position;
    startRotation = reqRigidbody.rotation;
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.L)) reqRigidbody.isKinematic = !reqRigidbody.isKinematic;
    if (Input.GetKeyDown(KeyCode.K))
    {
      reqRigidbody.position = startPos;
      reqRigidbody.rotation = startRotation;
      if (!reqRigidbody.isKinematic)
      {
        reqRigidbody.velocity = Vector3.zero;
        reqRigidbody.angularVelocity = Vector3.zero;
      }
    }
    if (Input.GetKeyDown(KeyCode.I))
    {
      FindObjectsOfType<MissileDeathController>().Do((m) => m.Kill());
    }
  }
}

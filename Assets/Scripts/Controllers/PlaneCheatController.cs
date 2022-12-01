using UnityEngine;
using UnityEngine.InputSystem;

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

  public void OnLockPlane(InputAction.CallbackContext context)
  {
    if (context.phase != InputActionPhase.Performed) return;

    reqRigidbody.isKinematic = !reqRigidbody.isKinematic;
  }

  public void OnResetPlane(InputAction.CallbackContext context)
  {
    if (context.phase != InputActionPhase.Performed) return;

    reqRigidbody.position = startPos;
    reqRigidbody.rotation = startRotation;
    if (!reqRigidbody.isKinematic)
    {
      reqRigidbody.velocity = Vector3.zero;
      reqRigidbody.angularVelocity = Vector3.zero;
    }
  }

  public void OnKillEnemies(InputAction.CallbackContext context)
  {
    if (context.phase != InputActionPhase.Performed) return;

    FindObjectsOfType<MissileDeathController>().Do((m) => m.Kill());
  }
}

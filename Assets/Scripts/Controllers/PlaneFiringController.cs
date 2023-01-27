using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerManagerBridge))]
public class PlaneFiringController : MonoBehaviour
{
  public float rps = 10;
  public float spread = 0.1f;
  public PoolRef bulletPool;
  public WeaponAudio audioPlayer;

  public Transform gunportLeft;
  public Transform gunportRight;

  [SerializeField]
  [HideInInspector]
  double lastShotTimestamp = 0;
  bool nextShotLeft = true;
  bool firing = false;

  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

  public void OnFire(InputAction.CallbackContext context)
  {
    firing = context.ReadValueAsButton();

    if (!audioPlayer) return;

    if (firing && context.performed) audioPlayer.StartFire(rps);
    if (!firing) audioPlayer.EndFire();
  }

  void FixedUpdate()
  {
    if (firing && lastShotTimestamp + 1f / rps < Time.fixedTimeAsDouble)
    {
      Vector3 dir = (Camera.main.transform.forward
        + Random.insideUnitSphere * spread).normalized;

      Rigidbody rb = reqPlayerManagerBridge.playerRigidbody;

      Vector3 position = nextShotLeft ? gunportLeft.position : gunportRight.position;
      // position -= transform.position + rb.position;
      nextShotLeft = !nextShotLeft;
      bulletPool.Get<BulletController>().Init(position, dir);
      lastShotTimestamp = Time.fixedTimeAsDouble;
    }
  }
}

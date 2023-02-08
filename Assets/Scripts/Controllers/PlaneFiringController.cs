using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerManagerBridge))]
public class PlaneFiringController : MultiUpdateBodyChild
{
  public float rps = 10;
  public float spread = 0.1f;
  public float aimAnglePerSecond = 180f;
  public PoolRef bulletPool;
  public WeaponAudio audioPlayer;

  public Transform gunportLeft;
  public Transform gunportRight;

  [SerializeField]
  [HideInInspector]
  bool nextShotLeft = true;
  bool firing = false;

  Quaternion aim;

  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

  public void OnFire(InputAction.CallbackContext context)
  {
    firing = context.ReadValueAsButton();

    if (!audioPlayer) return;

    if (firing && context.performed) audioPlayer.StartFire(rps);
    if (!firing) audioPlayer.EndFire();
  }

  protected override void BeforeUpdates()
  {
    if (aim == new Quaternion()) aim = Camera.main.transform.rotation;
    base.BeforeUpdates();
  }

  override protected Wait MultiUpdate (double deltaTime)
  {
    aim = Quaternion.RotateTowards(aim, Camera.main.transform.rotation, aimAnglePerSecond * (float)deltaTime);
    // Handle more precise firing controls
    if (!firing) return Wait.ForFrame();

    Vector3 dir = (aim * Vector3.forward
        + Random.insideUnitSphere * spread).normalized;

    Rigidbody rb = reqPlayerManagerBridge.playerRigidbody;

    Vector3 localPos = nextShotLeft ? gunportLeft.localPosition : gunportRight.localPosition;
    Vector3 position = interpolatedMatrix.MultiplyPoint(localPos);

    nextShotLeft = !nextShotLeft;
    bulletPool.Get<BulletController>().Init(position, dir, (float)timeToEndOfFrame);

    return Wait.For(1f / rps);
  }
}

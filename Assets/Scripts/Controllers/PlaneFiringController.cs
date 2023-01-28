using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerManagerBridge))]
public class PlaneFiringController : MultiUpdateBodyChild
{
  public float rps = 10;
  public float spread = 0.1f;
  public PoolRef bulletPool;
  public WeaponAudio audioPlayer;

  public Transform gunportLeft;
  public Transform gunportRight;

  [SerializeField]
  [HideInInspector]
  bool nextShotLeft = true;
  bool firing = false;

  Quaternion previousCameraDirection;

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
    if (previousCameraDirection == Quaternion.identity) previousCameraDirection = Camera.main.transform.rotation;
    base.BeforeUpdates();
  }

  protected override void AfterUpdates()
  {
    previousCameraDirection = Camera.main.transform.rotation;
    base.AfterUpdates();
  }

  override protected Wait MultiUpdate (float deltaTime)
  {
    // Handle more precise firing controls
    if (!firing) return Wait.ForFrame();

    Quaternion gunRotation = Quaternion.Slerp(previousCameraDirection, Camera.main.transform.rotation, frameRatio);

    Vector3 dir = (gunRotation * Vector3.forward
        + Random.insideUnitSphere * spread).normalized;

    Rigidbody rb = reqPlayerManagerBridge.playerRigidbody;

    //HOW TO  GET SUB POSITION ????
    // GET INFO ON
    // Vector3 position = nextShotLeft ? gunportLeft.a : gunportRight.position;
    Vector3 position = interpolatedPosition; 

    nextShotLeft = !nextShotLeft;
    bulletPool.Get<BulletController>().Init(position, dir, timeToEndOfFrame);

    return Wait.For(1f / rps);
  }
}

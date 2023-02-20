using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerManagerBridge))]
public class PlaneFiringController : MultiUpdateBodyChild
{
  [SerializeField] float rps = 10;
  [SerializeField] float spread = 0.1f;
  [SerializeField] float aimAnglePerSecond = 180f;
  [SerializeField] PoolRef bulletPool;

  [SerializeField] UnityEvent<float> onShoot;

  [SerializeField] Transform gunportLeft;
  [SerializeField] Transform gunportRight;

  bool nextShotLeft = true;
  bool firing = false;

  Quaternion aim;

  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

  public void OnFire(InputAction.CallbackContext context)
  {
    firing = context.ReadValueAsButton();
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

    onShoot.Invoke((float)(currentTime - timeOfLastUpdate));

    Vector3 dir = (aim * Vector3.forward
                   + Random.insideUnitSphere * spread).normalized;

    Vector3 localPos = nextShotLeft ? gunportLeft.localPosition : gunportRight.localPosition;
    Vector3 position = interpolatedMatrix.MultiplyPoint(localPos);

    nextShotLeft = !nextShotLeft;
    bulletPool.Get<BulletController>().Init(position, dir, (float)timeToEndOfFrame);

    return Wait.For(1f / rps);
  }
}

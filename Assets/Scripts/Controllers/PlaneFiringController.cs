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

  [SerializeField] Animation leftClip;
  [SerializeField] Animation rightClip;

  bool _nextShotLeft = true;
  bool _firing = false;
  Quaternion _aim;

  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

  public void OnFire(InputAction.CallbackContext context)
  {
    _firing = context.ReadValueAsButton();
  }

  protected override void BeforeUpdates()
  {
    if (_aim == new Quaternion()) _aim = Camera.main.transform.rotation;
    base.BeforeUpdates();
  }

  override protected Wait MultiUpdate (double deltaTime)
  {
    _aim = Quaternion.RotateTowards(_aim, Camera.main.transform.rotation, aimAnglePerSecond * (float)deltaTime);
    // Handle more precise firing controls
    if (!_firing) return Wait.ForFrame();

    onShoot.Invoke((float)(currentTime - timeOfLastUpdate));

    Vector3 dir = (_aim * Vector3.forward
                   + Random.insideUnitSphere * spread).normalized;

    if (_nextShotLeft) leftClip.Play();
    else rightClip.Play();

    leftClip.transform.forward = rightClip.transform.forward = dir;

    Vector3 localPos = _nextShotLeft ? gunportLeft.localPosition : gunportRight.localPosition;
    Vector3 position = interpolatedMatrix.MultiplyPoint(localPos);
    

    _nextShotLeft = !_nextShotLeft;
    bulletPool.Get<BulletController>().Init(position, dir, (float)timeToEndOfFrame);

    return Wait.For(1f / rps);
  }
}

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TargetManagerBridge))]
public class MissileFiringController : MonoBehaviour
{
  public float rps = 10;
  public float spread = 0.1f;
  public BulletController bulletPrefab;
  public float bulletSpeed = 1000f;
  public float fireCone = 90f;
  public float range = 100f;

  public float lockingTimeForWarning = 0.1f;
  public float lockingTimeForShooting = 0.2f;

  float reloadTime = 0;
  float lockTime = 0;

  bool lockedLastFrame = false;

  public UnityEvent<bool> onLockChange;
  public UnityEvent onLock;

  void Start()
  {
    reloadTime = 0;
  }

  void FixedUpdate()
  {
    reloadTime = Mathf.Max(reloadTime - Time.fixedDeltaTime, 0f);

    bool lockSuccessful = TryLock();

    if (lockSuccessful) {
      lockTime += Time.fixedDeltaTime;
    } else {
      lockTime = 0f;
    }

    if (lockTime > lockingTimeForWarning) {
      onLock.Invoke();
    }

    if (lockedLastFrame != lockSuccessful ) {
      onLockChange.Invoke(lockSuccessful);
    }
    lockedLastFrame = lockSuccessful;
  }

  bool TryLock() {

    Rigidbody r = GetComponent<Rigidbody>();

    // Get the plane target
    ITarget target = GetComponent<TargetManagerBridge>().instance.GetTarget();

    if (target.IsVisible(r.position) == false) return false;

    float distance = Vector3.Distance(target.position, r.position);

    if (distance > range) return false;

    Vector3 assumedPos = target.position;
    Vector3 dir = (assumedPos - r.position).normalized;

    if (Vector3.Angle(dir, r.rotation * Vector3.forward) > fireCone) return false;

    // Skip this frame until firing possible, but still lock
    if (lockTime < lockingTimeForShooting) return true;

    if (reloadTime > 0f) return true;

    Vector3 actualDir = (dir + Random.insideUnitSphere * spread).normalized;

    var bulletGO = GameObject.Instantiate(bulletPrefab.gameObject,
        r.position, Quaternion.identity,
        transform.parent
        );
    bulletGO.GetComponent<BulletController>().Init(r, actualDir);

    reloadTime = 1 / rps;

    return true;
  }
}

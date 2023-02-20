using Sound;
using UnityEngine;
using UnityEngine.Events;

public class MissileFiringController : MonoBehaviour
{
  public float rps = 10;
  public float spread = 0.1f;
  public PoolRef bulletPool;
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
  public UnityEvent<float> onShoot;

  [SerializeField][RequiredComponent] Missile reqMissile;
  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;
  [SerializeField][RequiredComponent] TargetManagerBridge reqTargetManagerBridge;

  [SerializeField] AudioEmitter audioEmitter;

  void Start()
  {
    reloadTime = 0;
  }

  void FixedUpdate()
  {
    reloadTime = Mathf.Max(reloadTime - Time.fixedDeltaTime, 0f);

    bool lockSuccessful = TryLock();

    if (lockSuccessful)
    {
      lockTime += Time.fixedDeltaTime;
    }
    else
    {
      lockTime = 0f;
    }

    if (lockTime > lockingTimeForWarning)
    {
      onLock.Invoke();
    }

    if (lockedLastFrame != lockSuccessful)
    {
      onLockChange.Invoke(lockSuccessful);
    }
    lockedLastFrame = lockSuccessful;
  }

  bool TryLock()
  {

    Rigidbody r = reqRigidbody;

    // Get the plane target
    ITarget target = reqTargetManagerBridge.instance.GetTarget();

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

    bulletPool
      .Get<BulletController>()
      .Init(r.position, actualDir);

    reloadTime = 1 / rps;
    
    onShoot.Invoke(0f);

    return true;
  }
}

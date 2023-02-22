using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using VolumetricLines;
using Sound;
using UnityEngine.Events;

[RequireComponent(typeof(VolumetricLineBehavior))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PlayerManagerBridge))]
[RequireComponent(typeof(WarningManagerBridge))]
// TODO: use multiupdatebodychild to raycast multiple time per frame
public class PlaneLaserController : MonoBehaviour
{
  [SerializeField] float dps = 10;
  [SerializeField] float range = 1000f;
  [SerializeField] float width = 10f;
  [SerializeField] float speed = 10000f;
  [SerializeField] float energyPerSecond = 3;
  [SerializeField] float delay = 0.3f;
  [SerializeField] LayerMask layerMask;

  [RequiredComponent][SerializeField] VolumetricLineBehavior reqLine;
  [RequiredComponent][SerializeField] MeshRenderer reqMeshRenderer;
  [RequiredComponent][SerializeField] PlayerManagerBridge reqPlayerManagerBridge;
  [RequiredComponent][SerializeField] WarningManagerBridge reqWarningManagerBridge;

  [SerializeField] UnityEvent<bool> onLaserStartStop;

  bool _fireInput = false;
  bool _firedLastFrame = false;

  public void OnFire(InputAction.CallbackContext context)
  {
    _fireInput = context.ReadValueAsButton();
  }

  void OnPrepareShot()
  {
    reqMeshRenderer.enabled = true;
    reqLine.Width = 0.1f;
    _delayToFire = delay;
    onLaserStartStop.Invoke(true);
  }

  void OnUnprepareShot()
  {
    _delayToFire = float.PositiveInfinity;
    onLaserStartStop.Invoke(false);
  }
  void OnFiringStartStop(bool isFiring)
  {
    _firedLastFrame = isFiring;
    reqMeshRenderer.enabled = isFiring;
    reqLine.Width = 1f;

    if (isFiring) reqPlayerManagerBridge.playerPlane.torqueMult /= 10;
    else reqPlayerManagerBridge.playerPlane.torqueMult *= 10;

    // if (firing) reqPlayerManagerBridge.playerRigidbody.inertiaTensor /= 100;
    // else reqPlayerManagerBridge.playerRigidbody.inertiaTensor *= 100;

    if (isFiring) reqPlayerManagerBridge.playerRigidbody.angularDrag /= 10;
    else reqPlayerManagerBridge.playerRigidbody.angularDrag *= 10;

    if (!isFiring) OnUnprepareShot();
  }

  // Return hit distance or max distance
  float FireLaser(Vector3 dir)
  {
    bool didHit = Physics.SphereCast(
      transform.position,
      width,
      dir,
      out RaycastHit hitInfo,
      range,
      layerMask,
      QueryTriggerInteraction.Ignore);

    if (!didHit) return range;

    IDamageReceiver damageReceiver = hitInfo.collider.GetComponent<IDamageReceiver>();

    if (damageReceiver == null) return hitInfo.distance;

    DamageInfo info = new DamageInfo
    {
      damage = dps * Time.fixedDeltaTime,
      oneOff = false,
      relativeVelocity = Vector3.forward * speed,
      position = hitInfo.point,
      normal = hitInfo.normal
    };

    damageReceiver.Damage(info);

    return hitInfo.distance;
  }

  static readonly RaycastHit[] raycastHits = new RaycastHit[10];

  void WarnPotentialTargets(Vector3 dir)
  {
    int numHit = Physics.SphereCastNonAlloc(
      transform.position,
      width * 20,
      dir,
      raycastHits,
      range,
      layerMask,
      QueryTriggerInteraction.Collide);
    
    raycastHits.Take(numHit).Do((hit) => {
      IEvasionTrigger trigger = hit.collider.GetComponent<IEvasionTrigger>();
      if (trigger != null) trigger.TriggerEvasion();
    });
  }

  float _delayToFire = float.PositiveInfinity;

  void FixedUpdate()
  {
    bool reallyFiring = _fireInput &&
                        reqPlayerManagerBridge.instance.planeEntity.TrySpendEnergy(
                          energyPerSecond * Time.fixedDeltaTime);

    if (_fireInput && !reallyFiring)
    {
      reqWarningManagerBridge.WarnLowLaser();
    }

    if (reallyFiring)
    {
      if (!float.IsFinite(_delayToFire))
      {
        OnPrepareShot();
      }

      if (_delayToFire > 0)
      {
        _delayToFire -= Time.deltaTime;
        return;
      }

      if (!_firedLastFrame)
      {
        OnFiringStartStop(true);
      }

      Vector3 dir = (transform.rotation * Vector3.forward).normalized;
      WarnPotentialTargets(dir);
      float distance = FireLaser(dir);
      reqLine.EndPos = Vector3.forward * distance;
    }
    else if (_firedLastFrame)
    {
      OnFiringStartStop(false);
    } else if (float.IsFinite(_delayToFire))
    {
      OnUnprepareShot();
    }
  }
}

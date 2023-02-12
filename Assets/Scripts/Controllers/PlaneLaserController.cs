using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using VolumetricLines;

[RequireComponent(typeof(VolumetricLineBehavior))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PlayerManagerBridge))]
// TODO: use multiupdatebodychild to raycast multiple time per frame
public class PlaneLaserController : MonoBehaviour
{
  [SerializeField] float dps = 10;
  [SerializeField] float range = 1000f;
  [SerializeField] float width = 10f;
  [SerializeField] float speed = 10000f;
  [SerializeField] float energyPerSecond = 3;
  [SerializeField] LayerMask layerMask;

  [RequiredComponent][SerializeField] VolumetricLineBehavior reqLine;
  [RequiredComponent][SerializeField] MeshRenderer reqMeshRenderer;
  [RequiredComponent][SerializeField] PlayerManagerBridge reqPlayerManagerBridge;
  
  [SerializeField] ContinuousWeaponAudio weaponAudio;

  bool _fireInput = false;
  bool _firedLastFrame = false;

  public void OnFire(InputAction.CallbackContext context)
  {
    _fireInput = context.ReadValueAsButton();
  }
  void OnFiringStartStop(bool isFiring)
  {
    reqMeshRenderer.enabled = isFiring;

    if (isFiring) reqPlayerManagerBridge.playerPlane.torqueMult /= 10;
    else reqPlayerManagerBridge.playerPlane.torqueMult *= 10;

    // if (firing) reqPlayerManagerBridge.playerRigidbody.inertiaTensor /= 100;
    // else reqPlayerManagerBridge.playerRigidbody.inertiaTensor *= 100;

    if (isFiring) reqPlayerManagerBridge.playerRigidbody.angularDrag /= 10;
    else reqPlayerManagerBridge.playerRigidbody.angularDrag *= 10;

    if (!weaponAudio) return;

    if (isFiring) weaponAudio.StartFire(1);
    if (!isFiring) weaponAudio.EndFire();
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

  void FixedUpdate()
  {
    bool reallyFiring = _fireInput &&
                        reqPlayerManagerBridge.instance.planeEntity.TrySpendEnergy(
                          energyPerSecond * Time.fixedDeltaTime);
    
    if (reallyFiring == false && _firedLastFrame)
    {
      _firedLastFrame = false;
      OnFiringStartStop(false);
    }
    else if (reallyFiring)
    {
      if (!_firedLastFrame)
      {
        OnFiringStartStop(true);
        _firedLastFrame = true;
      }

      Vector3 dir = (transform.rotation * Vector3.forward).normalized;
      WarnPotentialTargets(dir);
      float distance = FireLaser(dir);
      reqLine.EndPos = Vector3.forward * distance;
    }
  }
}

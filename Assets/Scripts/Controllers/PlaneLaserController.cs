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
  [SerializeField] LayerMask layerMask;

  [RequiredComponent][SerializeField] VolumetricLineBehavior reqLine;
  [RequiredComponent][SerializeField] MeshRenderer reqMeshRenderer;
  [RequiredComponent][SerializeField] PlayerManagerBridge reqPlayerManagerBridge;
  
  [SerializeField] ContinuousWeaponAudio weaponAudio;

  bool _firing = false;

  public void OnFire(InputAction.CallbackContext context)
  {
    _firing = context.ReadValueAsButton();

    if (context.started) return;

    if (_firing) reqPlayerManagerBridge.playerPlane.torqueMult /= 10;
    else reqPlayerManagerBridge.playerPlane.torqueMult *= 10;

    // if (firing) reqPlayerManagerBridge.playerRigidbody.inertiaTensor /= 100;
    // else reqPlayerManagerBridge.playerRigidbody.inertiaTensor *= 100;

    if (_firing) reqPlayerManagerBridge.playerRigidbody.angularDrag /= 10;
    else reqPlayerManagerBridge.playerRigidbody.angularDrag *= 10;

    if (!weaponAudio) return;

    if (_firing && context.performed) weaponAudio.StartFire(1);
    if (!_firing) weaponAudio.EndFire();
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
    reqMeshRenderer.enabled = _firing;
    if (_firing)
    {
      Vector3 dir = (transform.rotation * Vector3.forward).normalized;
      WarnPotentialTargets(dir);
      float distance = FireLaser(dir);
      reqLine.EndPos = Vector3.forward * distance;
    }
  }
}

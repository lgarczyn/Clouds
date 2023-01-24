using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using VolumetricLines;

public class AnimationCurveConstantLength : AnimationCurve
{
  public void Resize(float factor)
  {
    this.keys = this.keys.Select((k) =>
    {
      k.time *= factor;
      return k;
    }).ToArray();
  }
}

[RequireComponent(typeof(VolumetricLineBehavior))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PlayerManagerBridge))]
// [RequireComponent(typeof(WeaponAudio))]
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

  bool firing = false;

  public void OnFire(InputAction.CallbackContext context)
  {
    firing = context.ReadValueAsButton();

    if (context.started) return;

    if (firing) reqPlayerManagerBridge.playerPlane.torqueMult /= 10;
    else reqPlayerManagerBridge.playerPlane.torqueMult *= 10;

    // if (firing) reqPlayerManagerBridge.playerRigidbody.inertiaTensor /= 100;
    // else reqPlayerManagerBridge.playerRigidbody.inertiaTensor *= 100;

    if (firing) reqPlayerManagerBridge.playerRigidbody.angularDrag /= 10;
    else reqPlayerManagerBridge.playerRigidbody.angularDrag *= 10;

    if (!weaponAudio) return;

    if (firing && context.performed) weaponAudio.StartFire(1);
    if (!firing) weaponAudio.EndFire();
  }

  // Return hit distance or max distance
  float FireLaser(Vector3 dir)
  {
    RaycastHit hitInfo;

    bool didHit = Physics.SphereCast(
      transform.position,
      width,
      dir,
      out hitInfo,
      range,
      layerMask,
      QueryTriggerInteraction.Ignore);

    if (!didHit) return range;

    IDamageReceiver damageReceiver = hitInfo.collider.GetComponent<IDamageReceiver>();

    if (damageReceiver == null) return hitInfo.distance;

    DamageInfo info = new DamageInfo();

    info.damage = dps * Time.fixedDeltaTime;
    info.oneOff = false;
    info.relativeVelocity = Vector3.forward * speed;
    info.position = hitInfo.point;
    info.normal = hitInfo.normal;
    damageReceiver.Damage(info);

    return hitInfo.distance;
  }

  
  static RaycastHit[] raycastHits = new RaycastHit[10];

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
      var trigger = hit.collider.GetComponent<IEvasionTrigger>();
      if (trigger != null) trigger.TriggerEvasion();
    });
  }

  void FixedUpdate()
  {
    reqMeshRenderer.enabled = firing;
    if (firing)
    {
      Vector3 dir = (transform.rotation * Vector3.forward).normalized;
      WarnPotentialTargets(dir);
      float distance = FireLaser(dir);
      reqLine.EndPos = Vector3.forward * distance;
    }
  }
}

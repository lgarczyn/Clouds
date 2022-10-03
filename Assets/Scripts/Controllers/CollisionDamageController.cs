using UnityEngine;

public class CollisionDamageController : MonoBehaviour
{
  public float bodyCollisionDamage = 50f;
  public float collisionDps = 10f;

  DamageInfo GetDamageInfo(float damage, Collision collisionInfo)
  {
    DamageInfo info = new DamageInfo();
    info.damage = damage;
    info.relativeVelocity = collisionInfo.relativeVelocity;
    info.position = collisionInfo.GetContact(0).point;
    info.normal = collisionInfo.GetContact(0).normal;

    return info;
  }

  IDamageReceiver GetTarget(Collision collision)
  {
    IDamageReceiver receiver;

    var average = collision.GetAverageContact();

    if (average.thisCollider.TryGetComponent<IDamageReceiver>(out receiver))
      return receiver;

    if (collision.gameObject.TryGetComponent<IDamageReceiver>(out receiver))
      return receiver;

    return null;
  }

  void OnCollisionStay(Collision collision)
  {
    DamageInfo info = GetDamageInfo(collisionDps * Time.fixedDeltaTime, collision);
    info.oneOff = false;
    IDamageReceiver target = GetTarget(collision);
    if (target != null) target.Damage(info);
  }

  void OnCollisionEnter(Collision collision)
  {
    DamageInfo info = GetDamageInfo(bodyCollisionDamage, collision);
    info.oneOff = true;
    IDamageReceiver target = GetTarget(collision);
    if (target != null) target.Damage(info);
  }
}

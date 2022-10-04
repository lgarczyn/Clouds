using UnityEngine;

struct DefaultCollisionDealer : IDamageDealer
{
  float hit;
  float frame;
  public DefaultCollisionDealer(float hit, float frame)
  {
    this.hit = hit;
    this.frame = frame;
  }

  public float GetDamageHit()
  {
    return hit;
  }

  public float GetDamageFrame()
  {
    return frame;
  }
}

public class CollisionDamageController : MonoBehaviour
{
  public float collisionDamage = 10f;
  public float collisionDps = 10f;

  DamageInfo GetDamageInfo(float damage, Collision collisionInfo)
  {
    DamageInfo info = new DamageInfo();
    var average = collisionInfo.GetAverageContact();
    info.damage = damage;
    info.relativeVelocity = collisionInfo.relativeVelocity;
    info.position = average.point;
    info.normal = average.normal;

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

  IDamageDealer GetSource(Collision collision)
  {
    IDamageDealer dealer;

    if (collision.gameObject.TryGetComponent<IDamageDealer>(out dealer))
      return dealer;

    return new DefaultCollisionDealer(collisionDamage, collisionDps);
  }

  void OnCollisionStay(Collision collision)
  {
    IDamageDealer dealer = GetSource(collision);
    DamageInfo info = GetDamageInfo(dealer.GetDamageFrame() * Time.fixedDeltaTime, collision);
    info.oneOff = false;
    IDamageReceiver target = GetTarget(collision);
    if (target != null) target.Damage(info);
  }

  void OnCollisionEnter(Collision collision)
  {
    IDamageDealer dealer = GetSource(collision);
    DamageInfo info = GetDamageInfo(dealer.GetDamageHit(), collision);
    info.oneOff = true;
    IDamageReceiver target = GetTarget(collision);
    if (target != null) target.Damage(info);
  }
}

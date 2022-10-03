using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class BulletController : MonoBehaviour
{
  [SerializeField] float baseVelocity = 100;
  [SerializeField] float marginOnSpawn = 3;
  [SerializeField] float explosionForce = 10;
  [SerializeField] float damage = 10;
  [SerializeField] float lifetime = 10;
  [SerializeField] GameObject collisionEffect;
  [SerializeField] float bounceDistance;

  public void Init(Rigidbody parent, Vector3 normalizedDir)
  {
    Rigidbody r = GetComponent<Rigidbody>();
    r.position = parent.position + normalizedDir * marginOnSpawn;
    r.velocity = baseVelocity * normalizedDir;
    r.rotation = Quaternion.LookRotation(normalizedDir, Vector3.forward);

    TrailRenderer trail = GetComponent<TrailRenderer>();
  }

  void FixedUpdate()
  {
    lifetime -= Time.fixedDeltaTime;
    if (lifetime < 0f) GameObject.Destroy(gameObject);
  }

  IDamageReceiver GetDamageReceiver(Collision collisionInfo)
  {
    IDamageReceiver receiver;

    if (collisionInfo.collider.gameObject.TryGetComponent<IDamageReceiver>(out receiver))
    {
      return receiver;
    }

    if (collisionInfo.gameObject.TryGetComponent<IDamageReceiver>(out receiver))
    {
      return receiver;
    }

    return null;
  }

  DamageInfo GetDamageInfo(float damage, Collision collisionInfo)
  {
    DamageInfo info = new DamageInfo();
    var average = collisionInfo.GetAverageContact();
    info.damage = damage;
    info.relativeVelocity = collisionInfo.relativeVelocity;
    info.position = average.point;
    info.normal = average.normal;
    info.oneOff = true;

    return info;
  }

  Vector3 CalculateBouncedPosition(Collision collisionInfo)
  {
    var average = collisionInfo.GetAverageContact();

    return average.point + average.normal * bounceDistance;
  }



  void OnCollisionEnter(Collision collisionInfo)
  {
    // Destroy gameobject after 5s, to leave time for the trail to disappear
    GameObject.Destroy(gameObject, 5);

    // Get the point of contact
    Vector3 contact = collisionInfo.GetContact(0).point;

    // Instantiate the contact feedback
    if (collisionEffect)
    {
      GameObject.Instantiate(
        collisionEffect,
        contact,
        Quaternion.identity);
    }

    // Apply collision force if possible
    collisionInfo.rigidbody?.AddExplosionForce(
      explosionForce,
      contact,
      0f, 0f,
      ForceMode.Impulse);

    // Find if the target can be damaged (shield or health)
    DamageInfo damageInfo = GetDamageInfo(damage, collisionInfo);
    GetDamageReceiver(collisionInfo)?.Damage(damageInfo);

    // Handle the trail
    TrailRenderer trail = GetComponent<TrailRenderer>();
    // If at least one position, move the last one to the point of contact so that it doesn't bounce
    if (trail.positionCount > 1)
    {
      trail.SetPosition(trail.positionCount - 1, contact);
      trail.AddPosition(CalculateBouncedPosition(collisionInfo));
    }
    // Prevent any more emission
    trail.emitting = false;
    // Destroy the bullet earlier if trail is cleared
    trail.autodestruct = true;
  }
}

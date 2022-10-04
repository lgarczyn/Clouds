using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class BulletController : MonoBehaviour, IDamageDealer
{
  [SerializeField] float baseVelocity = 100;
  [SerializeField] float marginOnSpawn = 3;
  [SerializeField] float explosionForce = 10;
  [SerializeField] float damage = 10;
  [SerializeField] float lifetime = 10;
  [SerializeField] GameObject collisionEffect;
  [SerializeField] float bounceDistance;

  bool destroyed = false;

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

  Vector3 CalculateBouncedPosition(AverageContactPoint average)
  {
    return average.point + average.normal * bounceDistance;
  }

  void OnCollisionEnter(Collision collisionInfo)
  {
    if (destroyed) return;

    // Destroy gameobject after 5s, to leave time for the trail to disappear
    GameObject.Destroy(gameObject, 5);

    // Get the point of contact
    AverageContactPoint contact = collisionInfo.GetAverageContact();

    // Instantiate the contact feedback
    if (collisionEffect)
    {
      GameObject.Instantiate(
        collisionEffect,
        contact.point,
        Quaternion.identity);
    }

    // Apply collision force if possible
    collisionInfo.rigidbody?.AddExplosionForce(
      explosionForce,
      contact.point,
      0f, 0f,
      ForceMode.Impulse);

    // Handle the trail
    TrailRenderer trail = GetComponent<TrailRenderer>();
    // If at least one position, move the last one to the point of contact so that it doesn't bounce
    if (trail.positionCount > 1)
    {
      trail.SetPosition(trail.positionCount - 1, contact.point);
      trail.AddPosition(CalculateBouncedPosition(contact));
    }
    // Prevent any more emission
    trail.emitting = false;
    // Destroy the bullet earlier if trail is cleared
    trail.autodestruct = true;
  }

  public float GetDamageHit()
  {
    return destroyed ? -0f : damage;
  }

  public float GetDamageFrame()
  {
    return 0f;
  }
}

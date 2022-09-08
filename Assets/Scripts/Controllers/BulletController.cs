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


  public void Init(Rigidbody parent, Vector3 normalizedDir)
  {
    Rigidbody r = GetComponent<Rigidbody>();
    r.position = parent.position + normalizedDir * marginOnSpawn;
    r.velocity = parent.velocity + baseVelocity * normalizedDir;
    r.rotation = Quaternion.LookRotation(normalizedDir, Vector3.forward);

    TrailRenderer trail = GetComponent<TrailRenderer>();
  }

  void FixedUpdate()
  {
    lifetime -= Time.fixedDeltaTime;
    if (lifetime < 0f) GameObject.Destroy(gameObject);
  }

  void OnCollisionEnter(Collision collisionInfo)
  {
    GameObject.Destroy(gameObject, 5);

    var target = collisionInfo.gameObject;
    Rigidbody r = GetComponent<Rigidbody>();

    Vector3 contact = collisionInfo.GetContact(0).point;

    if (collisionEffect)
    {
      GameObject.Instantiate(collisionEffect, r.position, Quaternion.identity);
    }

    target.GetComponent<Rigidbody>().AddExplosionForce(
      explosionForce,
      contact,
      0f, 0f,
      ForceMode.Impulse);

    var damageReceiver = target.GetComponent<IDamageReceiver>();

    if (damageReceiver != null)
    {
      damageReceiver.Damage(damage);
    }

    TrailRenderer trail = GetComponent<TrailRenderer>();
    trail.AddPosition(contact);
    trail.emitting = false;
    trail.autodestruct = true;
  }
}

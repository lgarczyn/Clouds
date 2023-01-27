using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(PoolSubject))]
public class BulletController : MonoBehaviour, IDamageDealer
{
  /// <summary>
  /// The starting velocity of the bullet
  /// </summary>
  [SerializeField] float baseVelocity = 100;
  /// <summary>
  /// How far from the original rigidbody should the bullet spawn
  /// </summary>
  [SerializeField] float marginOnSpawn = 3;
  /// <summary>
  /// How strongly should the bullet impact targets
  /// </summary>
  [SerializeField] float explosionForce = 10;
  /// <summary>
  /// How much damage does the bullet do
  /// </summary>
  [SerializeField] float damage = 10;
  /// <summary>
  /// How long does the bullet fly before being destroyed
  /// </summary>
  [SerializeField] float lifetime = 10;
  /// <summary>
  /// Effect to be instantiated on collision (particles)
  /// </summary>
  [SerializeField] GameObject collisionEffect;
  /// <summary>
  /// How far should the bullet bounce after impact (currently glitchy)
  /// </summary>
  [SerializeField] float bounceDistance;

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;
  [SerializeField][RequiredComponent] TrailRenderer reqTrailRenderer;
  [SerializeField][RequiredComponent] PoolSubject reqPoolSubject;

  /// <summary>
  /// If the bullet was destroyed
  /// </summary>
  bool destroyed = false;
  /// <summary>
  /// The remaining lifetime of the bullet
  /// </summary>
  float currentLifetime = 0f;

  /// <summary>
  /// Initialize the bullet shot from a rigidbody
  /// </summary>
  /// <param name="parent">the shooter</param>
  /// <param name="normalizedDir">the direction of the bullet</param>
  public void Init(Vector3 position, Vector3 normalizedDir)
  {
    // enable collisions
    reqRigidbody.detectCollisions = true;
    // set position of bullet
    reqRigidbody.transform.position = position + normalizedDir * marginOnSpawn;
    reqRigidbody.MovePosition(position + normalizedDir * marginOnSpawn);
    reqRigidbody.position = position + normalizedDir * marginOnSpawn;

    // Set velocity of bullet
    Vector3 velocity = baseVelocity * normalizedDir;
    reqRigidbody.velocity = velocity;
    reqRigidbody.MoveRotation(Quaternion.LookRotation(velocity, Vector3.forward));
    reqRigidbody.rotation = Quaternion.LookRotation(velocity, Vector3.forward);
    // Init trailrenderer
    reqTrailRenderer.Clear();
    reqTrailRenderer.emitting = true;
    reqTrailRenderer.AddPosition(position);
    // Reset lifetime elements
    destroyed = false;
    currentLifetime = lifetime;
  }

  void FixedUpdate()
  {
    // Update lifetime
    currentLifetime -= Time.fixedDeltaTime;
    // Update rotation using velocity
    reqRigidbody.MoveRotation(Quaternion.LookRotation(reqRigidbody.velocity, Vector3.up));
    // Check if bullet should return to pool
    if (currentLifetime < 0f) Release();
  }

  /// <summary>
  /// Obtain the potential damage target from a collision
  /// </summary>
  /// <param name="collisionInfo">the result of a collision</param>
  /// <returns></returns>
  IDamageReceiver GetDamageReceiver(Collision collisionInfo)
  {
    IDamageReceiver receiver;

    // If collider has a damage receiver, return first
    if (collisionInfo.collider.gameObject.TryGetComponent<IDamageReceiver>(out receiver))
    {
      return receiver;
    }

    // Otherwise, use root collider
    if (collisionInfo.gameObject.TryGetComponent<IDamageReceiver>(out receiver))
    {
      return receiver;
    }

    return null;
  }

  // Calculate the position after the bounce
  Vector3 CalculateBouncedPosition(AverageContactPoint average)
  {
    return average.point + average.normal * bounceDistance;
  }

  // On collision, destroy bullet and figure out if anyone should die
  void OnCollisionEnter(Collision collisionInfo)
  {
    if (destroyed) return;

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
    TrailRenderer trail = reqTrailRenderer;
    // If at least one position, move the last one to the point of contact so that it doesn't bounce
    if (trail.positionCount > 1)
    {
      trail.SetPosition(trail.positionCount - 1, contact.point);
      trail.AddPosition(CalculateBouncedPosition(contact));
    }
    Release();
  }

  /// <summary>
  /// Return bullet to pool after trail has disappeared
  /// </summary>
  public void Release() {
    reqRigidbody.detectCollisions = false;
    // Prevent any more emission
    reqTrailRenderer.emitting = false;
    // Destroy the bullet earlier if trail is cleared
    if (reqPoolSubject == null) Debug.Log(reqPoolSubject, reqTrailRenderer);
    reqPoolSubject.Release(reqTrailRenderer.time);
  }

  /// <summary>
  /// Return the damage of the bullet
  /// Should the bullet already be destroyed, return 0
  /// </summary>
  /// <returns></returns>
  public float GetDamageHit()
  {
    return destroyed ? -0f : damage;
  }

  /// <summary>
  /// The damage per frame of contact, meaningless here
  /// </summary>
  /// <returns></returns>
  public float GetDamageFrame()
  {
    return 0f;
  }
}

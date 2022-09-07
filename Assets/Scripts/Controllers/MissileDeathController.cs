using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Missile))]
[RequireComponent(typeof(Rigidbody))]
public class MissileDeathController : MonoBehaviour, IDamageReceiver
{
  public float timeToDie = 5f;

  public ParticleSystemRenderer deathParticles;

  public MonoBehaviour[] toDisable;
  public Renderer[] rendererToDisable;

  public float Damage(float damage)
  {
    enabled = false;
    deathParticles.gameObject.SetActive(true);
    toDisable.Do((t) => t.enabled = false);
    rendererToDisable.Do((t) => t.enabled = false);
    GetComponent<Rigidbody>().AddRelativeTorque(Random.insideUnitSphere * 30, ForceMode.VelocityChange);
    Destroy(gameObject, timeToDie);
    return -damage;
  }
}

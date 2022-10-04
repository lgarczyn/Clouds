using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Missile))]
[RequireComponent(typeof(Rigidbody))]
public class MissileDeathController : MonoBehaviour, IDamageReceiver
{
  public float timeToDie = 5f;

  public ParticleSystemRenderer deathParticles;

  public MonoBehaviour[] toDisable;
  public TrailRenderer trail;

  public float Damage(DamageInfo damageInfo)
  {
    if (damageInfo.damage <= 0f) return 0f;

    enabled = false;
    deathParticles.gameObject.SetActive(true);
    toDisable.Do((t) => t.enabled = false);
    trail.emitting = false;
    GetComponent<Rigidbody>().AddRelativeTorque(Random.insideUnitSphere * 30, ForceMode.VelocityChange);
    Destroy(gameObject, timeToDie);
    return -damageInfo.damage;
  }

  public void Kill()
  {
    DamageInfo info = new DamageInfo();
    info.damage = 1;
    Damage(info);
  }
}

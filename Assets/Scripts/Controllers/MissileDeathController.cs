using UnityEngine;

public class MissileDeathController : MonoBehaviour, IDamageReceiver
{
  public float timeToDie = 5f;

  public UnityEngine.Events.UnityEvent onDeath;

  [SerializeField][RequiredComponent] Missile reqMissile;

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  public float Damage(DamageInfo damageInfo)
  {
    if (damageInfo.damage <= 0f) return 0f;

    onDeath.Invoke();

    enabled = false;
    reqRigidbody.AddRelativeTorque(Random.insideUnitSphere * 30, ForceMode.VelocityChange);
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

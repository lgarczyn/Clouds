using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlaneEntity : MonoBehaviour, IDamageReceiver
{
  public bool destroyed = false;
  public float maxHealth = 100f;
  public float maxEnergy = 100f;
  public float maxShield = 100f;
  public float health = 30;
  public float energy = 30;
  public float shield = 30;

  public bool shieldFull
  {
    get
    {
      return shield >= maxShield;
    }
  }

  public UnityEvent onDeath;
  public UnityEvent<float> healthChange;
  public UnityEvent<float> energyChange;
  [FormerlySerializedAs("matterChange")]
  public UnityEvent<float> shieldChange;
  public UnityEvent<float> damageTaken;
  public UnityEvent<float> repeatingDamageTaken;
  public UnityEvent<float> shieldDamageTaken;
  public UnityEvent<float> speedChange;

  public void FixedUpdate()
  {
    this.healthChange.Invoke(this.health);
    this.energyChange.Invoke(this.energy);
    this.shieldChange.Invoke(shield);
    this.speedChange.Invoke(GetComponent<Rigidbody>().velocity.magnitude);
  }

  public float Damage(DamageInfo info)
  {
    if (info.oneOff) this.damageTaken.Invoke(info.damage);
    else repeatingDamageTaken.Invoke(info.damage);

    if (destroyed || info.damage <= 0f)
      return -info.damage;

    this.health -= info.damage;
    float returnValue = health;

    this.health = Mathf.Clamp(this.health, 0, maxHealth);

    if (this.health <= 0)
    {
      destroyed = true;
      onDeath.Invoke();
    }
    return returnValue;
  }

  public void Repair(float units)
  {
    if (destroyed)
      return;

    this.health += units;
    this.health = Mathf.Clamp(this.health, 0, maxHealth);
  }

  public void RefuelEnergy(float units)
  {
    if (destroyed)
      return;

    this.energy = Mathf.Min(energy + units, maxEnergy);
  }
  public bool TrySpendEnergy(float units)
  {
    if (destroyed)
      return false;

    if (energy < units)
      return false;

    this.energy -= units;
    return true;
  }

  public void RefuelShield(float units)
  {
    if (destroyed)
      return;

    this.shield = Mathf.Min(shield + units, maxShield);
  }

  public bool TrySpendShield(float units)
  {
    if (destroyed)
      return false;

    if (shield < units)
      return false;

    this.shield -= units;
    this.shieldDamageTaken.Invoke(units);
    return true;
  }
}

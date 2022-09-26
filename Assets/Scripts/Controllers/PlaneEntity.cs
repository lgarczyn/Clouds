using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlaneEntity : MonoBehaviour, IDamageReceiver
{
  public bool destroyed = false;
  public float maxHealth = 100f;
  public float maxEnergy = 100f;
  public float maxMatter = 100f;
  public float health = 30;
  public float energy = 30;
  public float matter = 30;

  public UnityEvent onDeath;
  public UnityEvent<float> healthChange;
  public UnityEvent<float> energyChange;
  public UnityEvent<float> matterChange;
  public UnityEvent<bool> matterFull;
  public UnityEvent<float> damageTaken;
  public UnityEvent<float> repeatingDamageTaken;

  public void FixedUpdate()
  {
    this.healthChange.Invoke(this.health);
    this.energyChange.Invoke(this.energy);
    this.matterChange.Invoke(matter);
    this.matterFull.Invoke(matter == maxMatter);
  }

  public float Damage(float damage, bool oneOff)
  {
    if (oneOff) this.damageTaken.Invoke(damage);
    else repeatingDamageTaken.Invoke(damage);

    if (destroyed || damage <= 0f)
      return damage;

    this.health -= damage;
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
  public void RefuelMatter(float units)
  {
    if (destroyed)
      return;

    this.matter = Mathf.Min(matter + units, maxMatter);
  }
  public bool TrySpendMatter(float units)
  {
    if (destroyed)
      return false;

    if (matter < units)
      return false;

    this.matter -= units;
    return true;
  }
}

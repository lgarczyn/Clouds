using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlaneEntity : MonoBehaviour
{
    public bool destroyed = false;
    public float maxHealth = 100f;
    public float maxEnergy = 100f;
    public float maxMatter = 100f;
    public float health;
    public float energy;
    public float matter;

    public float repairTimer = 3f;

    float timeSinceLastDamage = float.PositiveInfinity;

    public UnityEvent onDeath;
    public UnityEvent<float> healthChange;
    public UnityEvent<float> energyChange;
    public UnityEvent<float> matterChange;

    public void Start() {
        this.health = this.maxHealth;
        this.matter = this.maxMatter;
        this.energy = this.maxEnergy;
    }

    public void FixedUpdate() {
        this.healthChange.Invoke(this.health);
        this.timeSinceLastDamage += Time.fixedDeltaTime;
    }

    public void Damage(float damage) {
        if (destroyed)
            return;

        timeSinceLastDamage = 0;
    
        this.health -= damage;
        this.health = Mathf.Clamp(this.health, 0, maxHealth);

        this.healthChange.Invoke(this.health);

        if (this.health <= 0) {
            destroyed = true;
            onDeath.Invoke();
        }
    }

    public void Repair(float units) {
        if (destroyed)
            return;

        this.health += units;
        this.health = Mathf.Clamp(this.health, 0, maxHealth);

        this.healthChange.Invoke(this.health);
    }

    public bool ShouldRepair() {
        return health < maxHealth && timeSinceLastDamage > repairTimer;

    }
    public void RefuelEnergy(float units) {
        if (destroyed)
            return;
        
        this.energy = Mathf.Min(energy + units, maxEnergy);
        this.energyChange.Invoke(this.energy);
    }
    public bool TrySpendEnergy(float units) {
        if (destroyed)
            return false;
        
        if (energy < units)
            return false;
    
        this.energy -= units;
        this.energyChange.Invoke(this.energy);
        return true;
    }
    public void RefuelMatter(float units) {
        if (destroyed)
            return;
    
        this.matter = Mathf.Min(matter + units, maxMatter);
        this.matterChange.Invoke(this.matter);
    }
    public bool TrySpendMatter(float units) {
        if (destroyed)
            return false;
        
        if (matter < units)
            return false;
    
        this.matter -= units;
        return true;
    }
}

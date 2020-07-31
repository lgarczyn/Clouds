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
    public float health = 30;
    public float energy = 30;
    public float matter = 30;

    public UnityEvent onDeath;
    public UnityEvent<float> healthChange;
    public UnityEvent<float> energyChange;
    public UnityEvent<float> matterChange;
    public UnityEvent<bool> matterFull;

    public void FixedUpdate() {
        this.healthChange.Invoke(this.health);
    }

    public void Damage(float damage) {
        if (destroyed || damage <= 0f)
            return;
    
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
        return health < maxHealth && energy > 10f;
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
        this.matterChange.Invoke(matter);
        this.matterFull.Invoke(matter == maxMatter);
    }
    public bool TrySpendMatter(float units) {
        if (destroyed)
            return false;
        
        if (matter < units)
            return false;
    
        this.matter -= units;
        this.matterFull.Invoke(matter == maxMatter);
        return true;
    }
}

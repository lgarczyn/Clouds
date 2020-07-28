using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneDamageController : MonoBehaviour
{
    public PlaneEntity plane;

    public float bodyCollisionDamage = 100f;
    public float collisionDps = 10f;
    public float minDepth = -1000f;
    public float pressureDpsPerMeter = 0.01f;

    float timeSinceLastDamage = 0f;

    void OnCollisionStay(Collision collision) {
        plane.Damage(collisionDps * Time.fixedDeltaTime);
    }

    // void OnTriggerEnter() {
    //     plane.Damage(bodyCollisionDamage);
    // }

    void FixedUpdate() {

        timeSinceLastDamage += Time.fixedDeltaTime;

        float extraDepth = minDepth - plane.transform.position.y;

        if (extraDepth > 0f) {
            plane.Damage(extraDepth * pressureDpsPerMeter * Time.fixedDeltaTime);
            timeSinceLastDamage = 0;
        }
    }
}

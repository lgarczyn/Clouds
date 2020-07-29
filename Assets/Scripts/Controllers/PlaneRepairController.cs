using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneRepairController : MonoBehaviour
{
    public PlaneEntity plane;
    public float repairPerSecond = 1f;
    void FixedUpdate() {
        float repairs = repairPerSecond * Time.fixedDeltaTime;

        if (plane.ShouldRepair())
            if (plane.TrySpendMatter(repairs / 10f))
                if (plane.TrySpendEnergy(repairs))
                    plane.Repair(repairs);
    }
}

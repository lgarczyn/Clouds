using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneRepairController : MonoBehaviour
{
    public PlaneEntity plane;
    public float repairPerSecond = 1f;

    public KeyCode repairKey;

    private bool repairPressed;

    void Update() {
        repairPressed = Input.GetKeyDown(repairKey);
    }

    void FixedUpdate() {
        float repairs = repairPerSecond * Time.fixedDeltaTime;

        if (repairPressed)
            if (plane.ShouldRepair())
                if (plane.TrySpendMatter(repairs / 10f))
                    if (plane.TrySpendEnergy(repairs))
                        plane.Repair(repairs);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneResourceController : MonoBehaviour
{
    public PlaneEntity plane;
    public float energyUnitPerSecond = 1f;
    public float matterUnitPerSecond = 1f;
    void FixedUpdate()
    {
        plane.RefuelEnergy(energyUnitPerSecond * Time.fixedDeltaTime);
        plane.RefuelMatter(matterUnitPerSecond * Time.fixedDeltaTime);
    }
}

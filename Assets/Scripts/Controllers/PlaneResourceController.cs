using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceCalculator: MonoBehaviour  {
    public abstract float GetLight();
    public abstract float GetDensity();
}

public class PlaneResourceController : MonoBehaviour
{
    public PlaneEntity plane;
    public ResourceCalculator resourceCalculator;

    public PlaneDeathController deathController;
    public float energyMultiplier = 1f;
    public float matterMultiplier = 1f;

    public float baseEnergyPerSecond = 1f;

    public AnimationCurve matterVsDensity;
    public AnimationCurve matterVsDepth;
    public AnimationCurve energyVsLight;


    void FixedUpdate()
    {
        float light = resourceCalculator.GetLight();
        float density = resourceCalculator.GetDensity();

        if (light < 0f || density < 0f) {

            // TODO display error to client
            // If not on startup and resources still cannot be retrieved
            if (Time.time > 1) Debug.LogWarning("Resource controller cannot calculate resources");
            return ;
        }

        float depth = Mathf.Clamp01(-plane.transform.position.y / 1400f + 0.5f); 

        plane.RefuelEnergy(energyMultiplier * energyVsLight.Evaluate(light) * Time.fixedDeltaTime);
        plane.RefuelMatter(matterMultiplier * matterVsDensity.Evaluate(density) * matterVsDepth.Evaluate(depth) * Time.fixedDeltaTime);

        if (plane.TrySpendEnergy(baseEnergyPerSecond * Time.fixedDeltaTime) == false) {
            deathController.KillPlane();
        }
    }
}

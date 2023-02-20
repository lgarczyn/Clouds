using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(Volume))]
[RequireComponent(typeof(ResourceCalculatorBridge))]
public class HigherBloomRadiusInClouds : MonoBehaviour
{
  [SerializeField] AnimationCurve radiusVsDensity;
  [SerializeField] AnimationCurve intensityVsDensity;

  [SerializeField][RequiredComponent] Volume reqVolume;
  [SerializeField][RequiredComponent] ResourceCalculatorBridge reqResourceCalculatorBridge;

  void Update()
  {
    if (!reqVolume.profile) return;

    if (!reqVolume.profile.TryGet(out Bloom bloomEffect)) return;

    float density = reqResourceCalculatorBridge.GetDensity();

    bloomEffect.scatter.Override(radiusVsDensity.Evaluate(density));
    bloomEffect.intensity.Override(intensityVsDensity.Evaluate(density));
  }
}

using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(Volume))]
public class HigherBloomRadiusInClouds : MonoBehaviour
{
  [SerializeField] AnimationCurve radiusVsDensity;
  [SerializeField] AnimationCurve intensityVsDensity;

  [SerializeField] FloatReference playerDensity;

  [SerializeField][RequiredComponent] Volume reqVolume;

  void Update()
  {
    if (!reqVolume.profile) return;

    if (!reqVolume.profile.TryGet(out Bloom bloomEffect)) return;

    bloomEffect.scatter.Override(radiusVsDensity.Evaluate(playerDensity));
    bloomEffect.intensity.Override(intensityVsDensity.Evaluate(playerDensity));
  }
}

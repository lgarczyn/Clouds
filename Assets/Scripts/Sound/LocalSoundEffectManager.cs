using UnityEngine;

[RequireComponent(typeof(AudioLowPassFilter))]
public class LocalSoundEffectManager : MonoBehaviour
{
  [SerializeField] AnimationCurve lowpassVsDensity;
  [SerializeField] ResourceCalculator resources;

  void Update()
  {
    float density = resources.GetDensity();
    AudioLowPassFilter filter = GetComponent<AudioLowPassFilter>();
    filter.cutoffFrequency = lowpassVsDensity.Evaluate(density);
  }
}

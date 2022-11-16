using UnityEngine;

public class LowPassFilterInClouds : MonoBehaviour
{
  [SerializeField] AnimationCurve lowpassVsDensity;
  [SerializeField] ResourceCalculator resources;

  [SerializeField][RequiredComponent] AudioLowPassFilter reqAudioLowPassFilter;

  void Update()
  {
    float density = resources.GetDensity();
    AudioLowPassFilter filter = reqAudioLowPassFilter;
    filter.cutoffFrequency = lowpassVsDensity.Evaluate(density);
  }
}

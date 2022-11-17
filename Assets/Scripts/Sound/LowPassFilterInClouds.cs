using UnityEngine;

public class LowPassFilterInClouds : MonoBehaviour
{
  [SerializeField] AnimationCurve lowpassVsDensity;

  [SerializeField][RequiredComponent] AudioLowPassFilter reqAudioLowPassFilter;
  [SerializeField][RequiredComponent] ResourceCalculatorBridge reqResourceCalculatorBridge;

  void Update()
  {
    float density = reqResourceCalculatorBridge.GetDensity();
    AudioLowPassFilter filter = reqAudioLowPassFilter;
    filter.cutoffFrequency = lowpassVsDensity.Evaluate(density);
  }
}

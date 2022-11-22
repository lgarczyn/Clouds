using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Text = TMPro.TextMeshProUGUI;
using UnityEngine.InputSystem;

public class FPSData : MonoBehaviour
{
  public float overheadScore = 2;
  public bool reset;

  [SerializeField][RequiredComponent] Text reqText;

  [SerializeField][RequiredComponent] GForceCalculatorBridge reqGForceCalculatorBridge;

  private bool firstFrame;
  SortedDictionary<int, int> frameTimes;
  int frameNum;

  private void Awake()
  {
    firstFrame = true;

    if (FrameTimingManager.IsFeatureEnabled())
    {
      FrameTimingManager.CaptureFrameTimings();
    }
  }

  void Update()
  {
    if (reset || frameTimes == null)
    {
      frameTimes = new SortedDictionary<int, int>();
      frameNum = 0;
      reset = false;
    }
    if (firstFrame == true)
    {
      firstFrame = false;
      return;
    }

    float frameSec = Time.unscaledDeltaTime;

    int frameMillisec = Mathf.RoundToInt(frameSec * 10000);

    if (frameTimes.ContainsKey(frameMillisec))
      frameTimes[frameMillisec]++;
    else
      frameTimes.Add(frameMillisec, 1);

    frameNum++;

    int[] indices = new int[5];

    indices[0] = 0;
    indices[1] = frameNum / 4;
    indices[2] = frameNum / 2;
    indices[3] = indices[1] + indices[2];
    indices[4] = (int)(frameNum * 0.99);

    string text = "0: " + frameTimes.ElementAt(0).Key + "\n";

    int indicesIndex = 1;
    int frameCounter = 0;
    int thirdQuartValue = 1;
    foreach (var kvp in frameTimes)
    {
      frameCounter += kvp.Value;
      while (frameCounter >= indices[indicesIndex])
      {
        text += indicesIndex + ": " + kvp.Key + "\n";
        if (indicesIndex == 3)
          thirdQuartValue = kvp.Key;
        indicesIndex++;
        if (indicesIndex >= indices.Length)
          break;
      }
      if (indicesIndex >= indices.Length)
        break;
    }

    float score = (overheadScore * 1000 + (float)Screen.width * Screen.height) / (float)thirdQuartValue;

    text += "score: " + Mathf.Round(score / 100);
    text += "\nfps: " + Mathf.Round(1 / Time.unscaledDeltaTime);

    if (FrameTimingManager.IsFeatureEnabled())
    {
      FrameTiming[] timingInfos = new FrameTiming[1];
      FrameTimingManager.GetLatestTimings(1, timingInfos);
      text += "\ngpu: " + System.Math.Round(timingInfos[0].gpuFrameTime) + "ms";
      text += "\ncpu: " + System.Math.Round(timingInfos[0].cpuFrameTime) + "ms";
    }

    text += "\nWindow: " + Screen.width + "x" + Screen.height;
    text += "\nRes: " + Screen.currentResolution;
    text += "\nFullScreen: " + Screen.fullScreen + " @ " + System.Enum.GetName(typeof(FullScreenMode), Screen.fullScreenMode);

    PlayerPlane plane = FindObjectOfType<PlayerPlane>();
    if (plane)
    {
      Vector3 pos = plane.transform.position;
      text += "\npos: " + Mathf.Round(pos.x) + "," + Mathf.Round(pos.y) + "," + Mathf.Round(pos.z);
      Rigidbody r = plane.GetComponent<Rigidbody>();
      if (r)
      {
        text += "\nvelocity: " + Mathf.Round(r.velocity.magnitude);
      }
      var gForceCalculator = reqGForceCalculatorBridge.instance;
      if (gForceCalculator)
      {
        text += "\ngforce: " + Mathf.Round(gForceCalculator.GetGForce() * 10) / 10;
      }
    }


    reqText.text = text;

    if (Keyboard.current.fKey.wasPressedThisFrame) reqText.enabled = !reqText.enabled;
  }
}

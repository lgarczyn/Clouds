using UnityEngine;

/// <summary>
/// Provides and updates the transform allowing to move from solar system space to local space
/// Has a timescale allowing changes in seasons
/// </summary>
public class OrbitController : MonoBehaviour
{
  public JupiterSpace frame;

  public double timeScale = 1;

  void Awake()
  {
    frame = new JupiterSpace();
    frame.UpdatePos(0);
  }

  void UpdatePos(double time)
  {
    frame.UpdatePos(time * timeScale);
  }

  void Update()
  {
    UpdatePos(Time.timeAsDouble);
  }

  void FixedUpdate()
  {
    UpdatePos(Time.fixedTimeAsDouble);
  }
}

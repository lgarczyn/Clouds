using UnityEngine;

[System.Serializable]
public class PIDController1D : PIDController<float>
{
  protected override float Zero() { return 0f; }
  protected override float Add(float a, float b) { return a + b; }
  protected override float Scale(float a, float b) { return a * b; }
  protected override float ClampMagnitude(float a, float max) { return Mathf.Clamp(a, -max, max); }
}

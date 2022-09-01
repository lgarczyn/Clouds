using UnityEngine;

[System.Serializable]
public class PIDController3D : PIDController<Vector3>
{
  protected override Vector3 Zero() { return Vector3.zero; }
  protected override Vector3 Add(Vector3 a, Vector3 b) { return a + b; }
  protected override Vector3 Scale(Vector3 a, float b) { return a * b; }
  protected override Vector3 ClampMagnitude(Vector3 a, float max) { return Vector3.ClampMagnitude(a, max); }
}

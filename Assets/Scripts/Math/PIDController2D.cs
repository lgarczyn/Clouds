using UnityEngine;

[System.Serializable]
public class PIDController2D : PIDController<Vector2>
{
  protected override Vector2 Zero() { return Vector2.zero; }
  protected override Vector2 Add(Vector2 a, Vector2 b) { return a + b; }
  protected override Vector2 Scale(Vector2 a, float b) { return a * b; }
  protected override Vector2 ClampMagnitude(Vector2 a, float max) { return Vector2.ClampMagnitude(a, max); }
}

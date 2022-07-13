using UnityEngine;
using System;

[Serializable]
public class InertialFrame : FrameOfReference
{
  Vector3D velocity;

  public InertialFrame(TransformD transform)
  {
    this.transform = transform;
    this.velocity = Vector3D.zero;
  }

  public InertialFrame(TransformD transform, Vector3D velocity)
  {
    this.transform = transform;
    this.velocity = velocity;
  }

  public void UpdatePos(double deltaTime)
  {
    this.transform = new TransformD(
      this.transform.position + velocity * deltaTime,
      this.transform.rotation,
      this.transform.scale
    );
  }
}

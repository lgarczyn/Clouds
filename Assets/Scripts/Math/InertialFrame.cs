using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertialFrame : Frame
{
  Vector3D velocity;

  public InertialFrame(TransformD transform)
  {
    this.parameters = transform;
    this.velocity = Vector3D.zero;
  }

  public InertialFrame(TransformD transform, Vector3D velocity)
  {
    this.parameters = transform;
    this.velocity = velocity;
  }

  public void UpdatePos(double deltaTime)
  {
    this.parameters = new TransformD(
      this.parameters.position + velocity * deltaTime,
      this.parameters.rotation,
      this.parameters.scale
    );
  }
}

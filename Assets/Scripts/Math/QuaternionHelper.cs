using UnityEngine;

/*
Copyright 2016 Max Kaufmann (max.kaufmann@gmail.com)
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

public static class QuaternionHelper
{

  public static Quaternion AngVelToDeriv(this Quaternion Current, Vector3 AngVel)
  {
    var Spin = new Quaternion(AngVel.x, AngVel.y, AngVel.z, 0f);
    var Result = Spin * Current;
    return new Quaternion(0.5f * Result.x, 0.5f * Result.y, 0.5f * Result.z, 0.5f * Result.w);
  }

  public static Vector3 DerivToAngVel(this Quaternion Current, Quaternion Deriv)
  {
    var Result = Deriv * Quaternion.Inverse(Current);
    return new Vector3(2f * Result.x, 2f * Result.y, 2f * Result.z);
  }

  public static Quaternion IntegrateRotation(this Quaternion Rotation, Vector3 AngularVelocity, float DeltaTime)
  {
    if (DeltaTime < Mathf.Epsilon) return Rotation;
    var Deriv = AngVelToDeriv(Rotation, AngularVelocity);
    var Pred = new Vector4(
        Rotation.x + Deriv.x * DeltaTime,
        Rotation.y + Deriv.y * DeltaTime,
        Rotation.z + Deriv.z * DeltaTime,
        Rotation.w + Deriv.w * DeltaTime
    ).normalized;
    return new Quaternion(Pred.x, Pred.y, Pred.z, Pred.w);
  }

  public static Quaternion SmoothDamp(
    this Quaternion rot,
    Quaternion target,
    ref Quaternion deriv,
    float time,
    float maxSpeed = float.PositiveInfinity,
    float deltaTime = float.NaN
  )
  {
    if (float.IsNaN(deltaTime)) deltaTime = Time.deltaTime;
    if (deltaTime < Mathf.Epsilon) return rot;
    // account for double-cover
    var Dot = Quaternion.Dot(rot, target);
    var Multi = Dot > 0f ? 1f : -1f;
    target.x *= Multi;
    target.y *= Multi;
    target.z *= Multi;
    target.w *= Multi;
    // smooth damp (nlerp approx)
    var Result = new Vector4(
      Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time, maxSpeed, deltaTime),
      Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time, maxSpeed, deltaTime),
      Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time, maxSpeed, deltaTime),
      Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time, maxSpeed, deltaTime)
    ).normalized;

    // ensure deriv is tangent
    var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
    deriv.x -= derivError.x;
    deriv.y -= derivError.y;
    deriv.z -= derivError.z;
    deriv.w -= derivError.w;

    return new Quaternion(Result.x, Result.y, Result.z, Result.w);
  }

  public static Quaternion SmoothDampSimple(
    this Quaternion rot,
    Quaternion target,
    ref float velocity,
    float smoothTime,
    float maxSpeed = float.PositiveInfinity,
    float deltaTime = float.NaN
  )
  {
    if (float.IsNaN(deltaTime)) deltaTime = Time.deltaTime;
    if (deltaTime < Mathf.Epsilon) return rot;

    float angle = Quaternion.Angle(rot, target);

    float step = Mathf.SmoothDampAngle(0, angle, ref velocity, smoothTime, maxSpeed, deltaTime);

    return Quaternion.RotateTowards(rot, target, step);
  }
}
using UnityEngine;

public class Frame
{
  protected TransformD parameters;

  public TransformD toLocalCoord(TransformD coord)
  {
    var globalPos = coord.position;
    var globalRot = coord.rotation;
    var globalScale = coord.scale;

    var inverseRot = QuaternionD.Inverse(parameters.rotation);

    var localPos = inverseRot * (globalPos - parameters.position) / parameters.scale;
    var localRot = inverseRot * globalRot;
    var localScale = globalScale / parameters.scale;

    return new TransformD(localPos, localRot, localScale);
  }

  public TransformD toGlobalCoord(TransformD coord)
  {
    var localPos = coord.position;
    var localRot = coord.rotation;
    var localScale = coord.scale;

    var globalPos = (parameters.rotation * localPos) * parameters.scale + parameters.position;
    var globalRot = parameters.rotation * localRot;
    var globalScale = localScale * parameters.scale;

    return new TransformD(globalPos, globalRot, globalScale);
  }

  public Frame(TransformD parameters = new TransformD())
  {
    this.parameters = parameters;
  }
}
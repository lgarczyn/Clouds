using UnityEngine;

public class FrameOfReference
{
  protected TransformD transform;

  public TransformD toLocal(TransformD coord)
  {
    var globalPos = coord.position;
    var globalRot = coord.rotation;
    var globalScale = coord.scale;

    var inverseRot = QuaternionD.Inverse(transform.rotation);

    var localPos = inverseRot * (globalPos - transform.position) / transform.scale;
    var localRot = inverseRot * globalRot;
    var localScale = globalScale / transform.scale;

    return new TransformD(localPos, localRot, localScale);
  }

  public TransformD toGlobalCoord(TransformD coord)
  {
    var localPos = coord.position;
    var localRot = coord.rotation;
    var localScale = coord.scale;

    var globalPos = (transform.rotation * localPos) * transform.scale + transform.position;
    var globalRot = transform.rotation * localRot;
    var globalScale = localScale * transform.scale;

    return new TransformD(globalPos, globalRot, globalScale);
  }

  public QuaternionD toLocalRot(QuaternionD rot)
  {
    // TODO: minimize and test
    return toLocal(new TransformD(Vector3D.zero, rot)).rotation;
  }
  public Vector3D toLocalPos(Vector3D pos)
  {
    // TODO: minimize and test
    return toLocal(new TransformD(pos)).position;
  }
  public QuaternionD toGlobalRot(QuaternionD rot)
  {
    // TODO: minimize and test
    return toGlobalCoord(new TransformD(Vector3D.zero, rot)).rotation;
  }
  public Vector3D toGlobalPos(Vector3D pos)
  {
    // TODO: minimize and test
    return toGlobalCoord(new TransformD(pos)).position;
  }

  public TransformD fromLocalTransform(Transform transform)
  {
    return toGlobalCoord(new TransformD(transform.position, transform.rotation));
  }

  public FrameOfReference(TransformD transform = new TransformD())
  {
    this.transform = transform;
  }
}
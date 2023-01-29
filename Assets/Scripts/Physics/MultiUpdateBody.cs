using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class MultiUpdateBody : MultiUpdateObject
{
  protected Vector3 previousPosition;
  protected Vector3 nextPosition;
  protected Vector3 interpolatedPosition {
    get {
      return Vector3.Lerp(previousPosition, nextPosition, (float)frameRatio);
    }
  }

  protected Quaternion previousRotation;
  protected Quaternion nextRotation;
  protected Quaternion interpolatedRotation {
    get {
      return Quaternion.Slerp(previousRotation, nextRotation, (float)frameRatio);
    }
  }

  [RequiredComponent][SerializeField] protected Rigidbody reqRigidbody; 

  override protected void BeforeUpdates() {
    base.BeforeUpdates();
    previousPosition = nextPosition;
    nextPosition = reqRigidbody.position;
  }

  override protected void ResetMultiUpdate(double time) {
    base.ResetMultiUpdate(time);
    nextPosition = previousPosition = reqRigidbody.position;
    nextRotation = previousRotation = reqRigidbody.rotation;
  }
}

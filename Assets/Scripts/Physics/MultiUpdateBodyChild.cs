using UnityEngine;

public class MultiUpdateBodyChild : MultiUpdateObject
{
  protected Rigidbody parentBodyRef;

  protected Rigidbody ParentBody {
    get {
      if (parentBodyRef) return parentBodyRef;
      parentBodyRef = GetComponentInParent<Rigidbody>();
      if (!parentBodyRef) throw new System.Exception("MultiUpdateBodyChild is not descended from a rigidbody");
      return parentBodyRef;
    }
  }

  protected Vector3 previousLocalPosition;
  protected Vector3 nextLocalPosition;
  protected Vector3 interpolatedLocalPosition {
    get {
      return Vector3.Lerp(previousLocalPosition, nextLocalPosition, frameRatio);
    }
  }

  protected Quaternion previousLocalRotation;
  protected Quaternion nextLocalRotation;
  protected Quaternion interpolatedLocalRotation {
    get {
      return Quaternion.Slerp(previousLocalRotation, nextLocalRotation, frameRatio);
    }
  }

  protected Vector3 previousParentPosition;
  protected Vector3 nextParentPosition;
  protected Vector3 interpolatedParentPosition {
    get {
      return Vector3.Lerp(previousParentPosition, nextParentPosition, frameRatio);
    }
  }

  protected Quaternion previousParentRotation;
  protected Quaternion nextParentRotation;
  protected Quaternion interpolatedParentRotation {
    get {
      return Quaternion.Slerp(previousParentRotation, nextParentRotation, frameRatio);
    }
  }

  protected Vector3 interpolatedPosition {
    get {
      return interpolatedParentPosition + interpolatedParentRotation * interpolatedLocalPosition;
    }
  }

  protected Quaternion previousRotation;
  protected Quaternion nextRotation;
  protected Quaternion interpolatedRotation {
    get {
      return interpolatedParentRotation * interpolatedLocalRotation;
    }
  }

  override protected void BeforeUpdates() {
    previousParentPosition = nextParentPosition;
    nextParentPosition = ParentBody.position;

    previousParentRotation = nextParentRotation;
    nextParentRotation = ParentBody.rotation;

    previousLocalPosition = nextLocalPosition;
    nextLocalPosition = ParentBody.transform.InverseTransformPoint(transform.position);

    previousLocalRotation = nextLocalRotation;
    nextLocalRotation = Quaternion.Inverse(nextParentRotation) * transform.rotation;

    if (ParentBody.rotation != ParentBody.transform.rotation) Debug.Log("rot fuckup:" + Quaternion.Angle(ParentBody.rotation, ParentBody.transform.rotation));
    if (ParentBody.position != ParentBody.transform.position) Debug.Log("pos fuckup:" + Vector3.Distance(ParentBody.position, ParentBody.transform.position));
  }

  override protected void ResetMultiUpdate(float time) {
    base.ResetMultiUpdate(time);
    nextParentPosition = previousParentPosition = ParentBody.position;
    nextParentRotation = previousParentRotation = ParentBody.rotation;
    nextLocalPosition = previousLocalPosition = ParentBody.transform.InverseTransformPoint(transform.position);
    nextLocalRotation = previousLocalRotation = Quaternion.Inverse(ParentBody.transform.rotation) * transform.rotation;
  }
}

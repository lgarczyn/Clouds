using UnityEngine;

public abstract class MultiUpdateBodyChild : MultiUpdateObject
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
  protected Vector3 interpolatedLocalPosition
  {
    get {
      return Vector3.Lerp(previousLocalPosition, nextLocalPosition, (float)frameRatio);
    }
  }

  protected Quaternion previousLocalRotation;
  protected Quaternion nextLocalRotation;
  protected Quaternion interpolatedLocalRotation
  {
    get {
      return Quaternion.Slerp(previousLocalRotation, nextLocalRotation, (float)frameRatio);
    }
  }

  protected Matrix4x4 interpolatedLocalMatrix
  {
    get
    {
      return Matrix4x4.Translate(interpolatedLocalPosition)     
             * Matrix4x4.Rotate(interpolatedLocalRotation)
             * Matrix4x4.Scale(transform.localScale);
    }
  }

  protected Vector3 previousParentPosition;
  protected Vector3 nextParentPosition;
  protected Vector3 interpolatedParentPosition
  {
    get
    {
      return Vector3.Lerp(previousParentPosition, nextParentPosition, (float)frameRatio);
    }
  }

  protected Quaternion previousParentRotation;
  protected Quaternion nextParentRotation;
  protected Quaternion interpolatedParentRotation
  {
    get
    {
      return Quaternion.Slerp(previousParentRotation, nextParentRotation, (float)frameRatio);
    }
  }

  protected Vector3 interpolatedPosition
  {
    get
    {
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

  protected Matrix4x4 interpolatedParentMatrix
  {
    get
    {
      return Matrix4x4.Translate(interpolatedParentPosition)
             * Matrix4x4.Rotate(interpolatedParentRotation)
             * Matrix4x4.Scale(ParentBody.transform.lossyScale);
    }
  }

  protected Matrix4x4 interpolatedMatrix
  {
    get
    {
      return interpolatedParentMatrix * interpolatedLocalMatrix;
    }
  }

  override protected void BeforeUpdates() {
    base.BeforeUpdates();

    previousParentPosition = nextParentPosition;
    nextParentPosition = ParentBody.position;

    previousParentRotation = nextParentRotation;
    nextParentRotation = ParentBody.rotation;

    previousLocalPosition = nextLocalPosition;
    nextLocalPosition = ParentBody.transform.InverseTransformPoint(transform.position);

    previousLocalRotation = nextLocalRotation;
    nextLocalRotation = Quaternion.Inverse(nextParentRotation) * transform.rotation;
  }

  override protected void ResetMultiUpdate(double time) {
    base.ResetMultiUpdate(time);
    nextParentPosition = previousParentPosition = ParentBody.position;
    nextParentRotation = previousParentRotation = ParentBody.rotation;
    nextLocalPosition = previousLocalPosition = ParentBody.transform.InverseTransformPoint(transform.position);
    nextLocalRotation = previousLocalRotation = Quaternion.Inverse(ParentBody.transform.rotation) * transform.rotation;
  }
}

using UnityAtoms.BaseAtoms;
using UnityEngine;

public interface ITarget
{
  public bool IsVisible(Vector3 seekerPosition);
  public Vector3 Position { get; }
  public Vector3 Velocity { get; }
}

public class Target : MonoBehaviour, ITarget
{
  [SerializeField] float invisibilityThreshold = 5f;

  [SerializeField] float alwaysVisibleRange = 60f;

  [SerializeField] bool forceInvisibility = false;

  [SerializeField] FloatReference playerDensity;

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  public bool IsVisible(Vector3 seekerPosition)
  {
    bool forceVisible = Vector3.Distance(seekerPosition, transform.position) < alwaysVisibleRange;
    bool shouldBeVisible = playerDensity < invisibilityThreshold;
    return (shouldBeVisible || forceVisible) && forceInvisibility == false;
  }

  public Vector3 Position => reqRigidbody.position;

  public Vector3 Velocity => reqRigidbody.velocity;
}
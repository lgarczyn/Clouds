using UnityEngine;

public interface ITarget
{
  public bool IsVisible(Vector3 position);
  public Vector3 position { get; }
  public Vector3 velocity { get; }
}

public class Target : MonoBehaviour, ITarget
{
  public float invisibilityThreshold = 5f;

  public bool forceInvisibility = false;

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  [SerializeField][RequiredComponent] ResourceCalculatorBridge reqResourceCalculatorBridge;

  private bool isVisible = false;

  void Update()
  {
    var resourceCalculator = reqResourceCalculatorBridge.instance;
    isVisible = resourceCalculator.GetDensity() < invisibilityThreshold;
  }

  public bool IsVisible(Vector3 position)
  {
    return isVisible && forceInvisibility == false;
  }

  public Vector3 position
  {
    get
    {
      return reqRigidbody.position;
    }
  }

  public Vector3 velocity
  {
    get
    {
      return reqRigidbody.velocity;
    }
  }
}
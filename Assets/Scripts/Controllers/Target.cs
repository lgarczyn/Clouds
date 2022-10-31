using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ResourceCalculatorBridge))]
public class Target : MonoBehaviour
{
  public float invisibilityThreshold = 5f;

  public bool forceInvisibility = false;

  private bool isVisible = false;

  void Update()
  {
    var resourceCalculator = GetComponent<ResourceCalculatorBridge>().instance;
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
      return GetComponent<Rigidbody>().position;
    }
  }

  public Vector3 velocity
  {
    get
    {
      return GetComponent<Rigidbody>().velocity;
    }
  }
}
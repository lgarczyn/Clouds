using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
  public ResourceCalculator resourceCalculator;
  public float invisibilityThreshold = 5f;

  public bool forceInvisibility = false;

  private bool isVisible = false;

  void Update()
  {
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
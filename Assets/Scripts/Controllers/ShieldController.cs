using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class ShieldController : MonoBehaviour, IDamageReceiver
{
  public PlaneEntity plane;
  public AnimationCurve shieldCoverageVsTime;
  public AnimationCurve shieldContrastVsTime;

  float timeSinceImpact = float.PositiveInfinity;

  public bool alwaysTest;

  Material material;

  public MeshRenderer meshRenderer
  {
    get
    {
      return GetComponent<MeshRenderer>();
    }
  }

  new public Collider collider
  {
    get
    {
      return GetComponent<Collider>();
    }
  }

  void Start()
  {
    meshRenderer.enabled = false;
    material = meshRenderer.material;
    meshRenderer.material = material;
  }

  public float Damage(DamageInfo info)
  {
    // Reset shield animation
    timeSinceImpact = 0;

    // Calculate the relative position of the hit
    Vector3 relativePos = info.position - transform.position;
    // Rotate the shield animation towards the impact point
    transform.rotation = Quaternion.LookRotation(relativePos, Vector3.up);

    // Spend a damage amount of shield
    if (plane.TrySpendShield(info.damage))
      return plane.shield;

    // If not enough shield, break the shield
    plane.TrySpendShield(plane.shield);
    collider.enabled = false;
    meshRenderer.enabled = false;
    return 0;
  }

  void FixedUpdate()
  {
    if (plane.shield > 0)
    {
      collider.enabled = true;
    }
  }

  void Update()
  {
    // Get the animation final key
    float lastAnimationValue = shieldCoverageVsTime.keys.Last().time;

    // If animation is over, end animation
    // If testing, start animation again
    if (timeSinceImpact > lastAnimationValue)
    {
      meshRenderer.enabled = false;

      if (alwaysTest)
      {
        DamageInfo info = new DamageInfo();
        info.position = transform.position + Random.onUnitSphere;
        Damage(info);
      }
    }
    else
    {
      // Display shield and set animated values
      meshRenderer.enabled = true;

      material.SetFloat("_Coverage", shieldCoverageVsTime.Evaluate(timeSinceImpact));
      material.SetFloat("_Contrast", shieldContrastVsTime.Evaluate(timeSinceImpact));
    }
    // Update animation value
    timeSinceImpact += Time.deltaTime;
  }
}

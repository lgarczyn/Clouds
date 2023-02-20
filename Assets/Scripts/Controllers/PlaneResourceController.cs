using UnityAtoms.BaseAtoms;
using UnityEngine;

[RequireComponent(typeof(PlaneEntity))]
public class PlaneResourceController : MonoBehaviour
{
  [RequiredComponent][SerializeField] PlaneEntity plane;
  public AnimationCurve energyVsDensity;
  public FloatReference playerDensity;

  void FixedUpdate()
  {
    plane.RefuelEnergy(energyVsDensity.Evaluate(playerDensity) * Time.fixedDeltaTime);
  }
}

using UnityEngine;

public interface IEvasionTrigger {
  void TriggerEvasion();
}

public class MissileEvasionTriggerController : MonoBehaviour, IEvasionTrigger
{
  public Missile parent;

  public float distance;

  public float duration;

  private void CheckCollider(Collider other)
  {
    if (other.GetComponent<IDamageDealer>() == null) return;
    TriggerEvasion();
  }

  // TODO: consider sending relative target, so missile can do averages/sum of temp targets and avoid confusion
  public void TriggerEvasion() {
    parent.SetTempTarget(transform.position + Random.onUnitSphere * distance, duration);
  }

  private void OnTriggerEnter(Collider other)
  {
    CheckCollider(other);
  }

  private void OnTriggerLeave(Collider other)
  {
    CheckCollider(other);
  }
}

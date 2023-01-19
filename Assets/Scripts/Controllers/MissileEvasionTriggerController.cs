using UnityEngine;

public class MissileEvasionTriggerController : MonoBehaviour
{
  public Missile parent;

  public float distance;

  public float duration;

  private void CheckCollider(Collider other)
  {
    if (other.GetComponent<IDamageDealer>() == null) return;
    TriggerEvasion();
  }

  public void TriggerEvasion() {
    Debug.Log("Triggered");
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

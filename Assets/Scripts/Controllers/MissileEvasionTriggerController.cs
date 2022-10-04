using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileEvasionTriggerController : MonoBehaviour
{
  public Missile parent;

  public float distance;

  public float duration;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.U))
    {
      parent.SetTempTarget(transform.position + Random.onUnitSphere * distance, duration);
    }
  }

  private void CheckCollider(Collider other)
  {
    if (other.GetComponent<IDamageDealer>() == null) return;

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

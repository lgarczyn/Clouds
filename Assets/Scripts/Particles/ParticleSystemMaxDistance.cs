using UnityEngine;

[RequireComponent(typeof(ParticleSystemRenderer))]
public class ParticleSystemMaxDistance : MonoBehaviour
{
  public float maxDistance = 10000;

  void Update()
  {
    Transform target = Camera.main.transform;
    float sqrMaxDistance = maxDistance * maxDistance;
    float sqrDistance = Vector3.SqrMagnitude(target.position - transform.position);

    if (sqrDistance > sqrMaxDistance)
    {
      GetComponent<ParticleSystemRenderer>().enabled = false;
      this.enabled = false;
    }
  }
}

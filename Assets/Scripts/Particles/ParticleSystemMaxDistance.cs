using UnityEngine;

public class ParticleSystemMaxDistance : MonoBehaviour
{
  public float maxDistance = 10000;

  [SerializeField][RequiredComponent] ParticleSystemRenderer reqParticleSystemRenderer;

  void Update()
  {
    Transform target = Camera.main.transform;
    float sqrMaxDistance = maxDistance * maxDistance;
    float sqrDistance = Vector3.SqrMagnitude(target.position - transform.position);

    if (sqrDistance > sqrMaxDistance)
    {
      reqParticleSystemRenderer.enabled = false;
      this.enabled = false;
    }
  }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneFiringController : MonoBehaviour
{
  public float bps = 10;
  public float spread = 0.1f;
  public BulletController bulletPrefab;

  [SerializeField]
  [HideInInspector]
  double lastShotTimestamp = 0;

  void FixedUpdate()
  {

    if (Input.GetMouseButton(0))
    {
      if (lastShotTimestamp + 1f / bps < Time.fixedTimeAsDouble)
      {
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        Vector3 dir = (Camera.main.transform.forward
          + Random.insideUnitSphere * spread).normalized;

        var bulletGO = GameObject.Instantiate(bulletPrefab.gameObject,
          rigidbody.position, Quaternion.identity,
          transform.parent
          );
        bulletGO.GetComponent<BulletController>().Init(rigidbody, dir);
        lastShotTimestamp = Time.fixedTimeAsDouble;
      }
    }
  }
}

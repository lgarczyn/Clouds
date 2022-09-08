using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MissileFiringController : MonoBehaviour
{
  public float bps = 10;
  public float spread = 0.1f;
  public BulletController bulletPrefab;
  public float bulletSpeed = 1000f;
  public float fireCone = 90f;
  public float range = 100f;

  [SerializeField]
  [HideInInspector]
  float reloadTime = 0;

  public Rigidbody target;

  void Start()
  {
    reloadTime = 0;
  }

  void FixedUpdate()
  {
    reloadTime -= Time.fixedTime;
    reloadTime = Mathf.Max(reloadTime, 0f);

    if (reloadTime > 0f) return;

    Rigidbody r = GetComponent<Rigidbody>();
    float distance = Vector3.Distance(target.position, r.position);

    if (distance > range) return;

    Vector3 assumedPos = target.position;
    Vector3 dir = (assumedPos - r.position).normalized;

    if (Vector3.Angle(dir, r.rotation * Vector3.forward) > fireCone) return;

    Vector3 actualDir = (dir + Random.insideUnitSphere * spread).normalized;

    var bulletGO = GameObject.Instantiate(bulletPrefab.gameObject,
        r.position, Quaternion.identity,
        transform.parent
        );
    bulletGO.GetComponent<BulletController>().Init(r, actualDir);

    reloadTime = 1 / bps * Random.Range(0.9f, 1.1f);
  }
}

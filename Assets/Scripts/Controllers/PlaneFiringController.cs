using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneFiringController : MonoBehaviour
{
  public float rps = 10;
  public float spread = 0.1f;
  public BulletController bulletPrefab;
  public WeaponAudio audioPlayer;

  [SerializeField]
  [HideInInspector]
  double lastShotTimestamp = 0;
  bool firing = false;

  void FixedUpdate()
  {

    if (lastShotTimestamp + 1f / rps < Time.fixedTimeAsDouble)
    {
      if (Input.GetMouseButton(0))
      {
        if (firing == false && audioPlayer) audioPlayer.StartFire(rps);

        firing = true;

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
      else
      {
        if (firing == true && audioPlayer) audioPlayer.EndFire();
        firing = false;
      }
    }
  }
}

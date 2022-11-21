using UnityEngine;
using UnityEngine.InputSystem;

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

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  void FixedUpdate()
  {

    if (lastShotTimestamp + 1f / rps < Time.fixedTimeAsDouble)
    {
      if (Mouse.current.leftButton.isPressed)
      {
        if (firing == false && audioPlayer) audioPlayer.StartFire(rps);

        firing = true;

        Vector3 dir = (Camera.main.transform.forward
          + Random.insideUnitSphere * spread).normalized;

        var bulletGO = GameObject.Instantiate(bulletPrefab.gameObject,
          reqRigidbody.position, Quaternion.identity,
          transform.parent
          );
        bulletGO.GetComponent<BulletController>().Init(reqRigidbody, dir);
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

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

  public void OnFire(InputAction.CallbackContext context)
  {
    firing = context.ReadValueAsButton();

    if (!audioPlayer) return;

    if (firing && context.performed) audioPlayer.StartFire(rps);
    if (!firing) audioPlayer.EndFire();
  }

  void FixedUpdate()
  {
    if (firing && lastShotTimestamp + 1f / rps < Time.fixedTimeAsDouble)
    {
      Vector3 dir = (Camera.main.transform.forward
        + Random.insideUnitSphere * spread).normalized;

      var bulletGO = GameObject.Instantiate(bulletPrefab.gameObject,
        reqRigidbody.position, Quaternion.identity,
        transform.parent
        );
      bulletGO.GetComponent<BulletController>().Init(reqRigidbody, dir);
      lastShotTimestamp = Time.fixedTimeAsDouble;
    }
  }
}

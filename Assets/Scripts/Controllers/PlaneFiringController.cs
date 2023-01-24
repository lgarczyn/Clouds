using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManagerBridge))]
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

  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

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

      Rigidbody rb = reqPlayerManagerBridge.playerRigidbody;

      var bulletGO = GameObject.Instantiate(bulletPrefab.gameObject,
        rb.position, Quaternion.identity,
        transform.parent
        );
      bulletGO.GetComponent<BulletController>().Init(rb, dir);
      lastShotTimestamp = Time.fixedTimeAsDouble;
    }
  }
}

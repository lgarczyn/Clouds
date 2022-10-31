using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerManagerBridge))]
public class FaunaController : MonoBehaviour
{
  public Animation[] animations;

  public Transform model;
  public float depopDistance = 1000f;
  public float maxRepopDistance = 500f;
  public float minRepopDistance = 200f;
  public float scaleRatioRange = 1.2f;
  public float scaleBase = 1f;
  public float animationSpeedBase = 1f;

  private float scale;
  private float lastRepop;

  void Start()
  {
    Repop();
  }

  void FixedUpdate()
  {
    // Try to repop
    if (ShouldRepop()) Repop();

    // Scale into full size over a second, to avoid popping into view
    float scaleAnim = Mathf.Clamp((Time.time - lastRepop), 0.01f, 1);

    this.transform.localScale = Vector3.one * (scaleAnim * scale);
  }

  // Move the whale in a hollow cylinder around the player 
  void Repop()
  {

    lastRepop = Time.time;

    SetRandomPos();
    SetRandomScale();
    GetComponentInChildren<Animation>().Play();

    OnRepop();
  }

  void SetRandomPos()
  {
    Transform player = GetComponent<PlayerManagerBridge>().transform;

    Vector3 playerPos = player.position;

    Vector3 randomPos = Random.onUnitSphere;

    randomPos *= Random.Range(minRepopDistance, maxRepopDistance);

    Vector3 newPos = playerPos + randomPos;

    GetComponent<Rigidbody>().position = newPos;

  }

  void SetRandomScale()
  {
    scale = scaleBase * Random.Range(1f / scaleRatioRange, scaleRatioRange);

    model.localScale = Vector3.one * scale;

    foreach (Animation animation in animations)
    {
      foreach (AnimationState state in animation)
      {
        state.speed = animationSpeedBase / Mathf.Sqrt(scale);
      }
    }
  }

  protected virtual void OnRepop() { }

  bool ShouldRepop()
  {
    Transform player = GetComponent<PlayerManagerBridge>().transform;

    Rigidbody rigidbody = GetComponent<Rigidbody>();

    Vector2 playerPos = new Vector2(player.position.x, player.position.z);
    Vector2 whalePos = new Vector2(rigidbody.position.x, rigidbody.position.z);

    return Vector2.Distance(playerPos, whalePos) > depopDistance;
  }
}

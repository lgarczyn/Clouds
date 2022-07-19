using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FaunaController : MonoBehaviour
{
  public Transform container;
  public Transform player;

  public Animation[] animations;

  public Transform model;
  public float depopDistance = 1000f;
  public float maxRepopDistance = 500f;
  public float minRepopDistance = 200f;
  public float containerHeightRepopRange = 0.8f;
  public float scaleRatioRange = 1.2f;
  public float scaleBase = 1f;
  public float animationSpeedBase = 1f;

  public float minHeight
  {
    get
    {
      return container.position.y - container.localScale.y / 2f;
    }
  }
  public float maxHeight
  {
    get
    {
      return container.position.y + container.localScale.y / 2f;
    }
  }

  void Start()
  {
    Repop();

    float scale = scaleBase * Random.Range(1f / scaleRatioRange, scaleRatioRange);

    model.localScale = Vector3.one * scale;

    foreach (Animation animation in animations)
    {
      foreach (AnimationState state in animation)
      {
        state.speed /= Mathf.Sqrt(scale);
      }
    }
  }

  void FixedUpdate()
  {
    // Either repop
    if (ShouldRepop()) Repop();
  }

  // Move the whale in a hollow cylinder around the player 
  void Repop()
  {
    Vector3 playerPos = new Vector3(player.position.x, 0f, player.position.z);

    float randomRadius = Random.Range(minRepopDistance, maxRepopDistance);
    float randomAngle = Random.Range(-Mathf.PI, Mathf.PI);
    float randomHeight = Random.Range(minHeight, maxHeight) * containerHeightRepopRange;


    Vector3 randomPos = new Vector3(
        Mathf.Cos(randomAngle) * randomRadius,
        randomHeight,
        Mathf.Sin(randomAngle) * randomRadius
    );

    Vector3 newPos = playerPos + randomPos;

    GetComponent<Rigidbody>().position = newPos;

    OnRepop(newPos);
  }

  protected virtual void OnRepop(Vector3 newPos)
  {

  }

  bool ShouldRepop()
  {
    Rigidbody rigidbody = GetComponent<Rigidbody>();

    Vector2 playerPos = new Vector2(player.position.x, player.position.z);
    Vector2 whalePos = new Vector2(rigidbody.position.x, rigidbody.position.z);

    return Vector2.Distance(playerPos, whalePos) > depopDistance;
  }
}

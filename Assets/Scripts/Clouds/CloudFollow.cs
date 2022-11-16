using UnityEngine;

public class CloudFollow : MonoBehaviour
{
  public bool increaseOnPlay = true;

  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManagerBridge;

  void Start()
  {
    if (increaseOnPlay)
    {
      transform.localScale = new Vector3(
          transform.localScale.x * 1000,
          transform.localScale.y,
          transform.localScale.z * 1000
      );
      increaseOnPlay = false;
    }
  }

  void LateUpdate()
  {
    var player = reqPlayerManagerBridge.instance.transform;

    Vector3 playerPos = player.position;
    playerPos.y = 0;
    transform.position = playerPos;
  }
}

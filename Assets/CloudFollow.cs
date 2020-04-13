using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudFollow : MonoBehaviour
{
    public Transform player;
    public bool increaseOnPlay = true;

    void Start() {
        if (increaseOnPlay) {
            transform.localScale = new Vector3(
                transform.localScale.x * 10,
                transform.localScale.y,
                transform.localScale.z * 10
            );
        }
    }

    void LateUpdate()
    {
        if (player)
        {
            Vector3 playerPos = player.position;
            playerPos.y = 0;
            transform.position = playerPos;
        }
    }
}

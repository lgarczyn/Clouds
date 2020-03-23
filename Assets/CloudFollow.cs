using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudFollow : MonoBehaviour
{
    public Transform player;
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

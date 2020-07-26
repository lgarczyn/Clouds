using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanktonController : MonoBehaviour
{
    new public Transform camera;
    void LateUpdate()
    {
        transform.position = camera.position;
    }
}

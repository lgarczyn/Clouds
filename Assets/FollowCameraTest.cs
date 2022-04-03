using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraTest : MonoBehaviour
{
    public Camera target;

    public Vector3 fix;

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.localRotation =  Quaternion.Euler(fix) * target.transform.localRotation;
    }
}

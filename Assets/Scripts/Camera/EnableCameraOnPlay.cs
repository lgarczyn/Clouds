using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCameraOnPlay : MonoBehaviour
{

  [SerializeField][RequiredComponent] Camera reqCamera;

  void Start()
  {
    reqCamera.enabled = true;
  }
}

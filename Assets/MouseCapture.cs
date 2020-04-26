using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCapture : MonoBehaviour
{
    void Update()
    {
        if (Application.platform == RuntimePlatform.LinuxEditor || Application.isEditor == false)
        {
            if (Input.GetMouseButtonDown(0))
                Cursor.lockState = CursorLockMode.Locked;
            if (Input.GetKeyDown(KeyCode.Escape))
                Cursor.lockState = CursorLockMode.None;
        }
    }
}

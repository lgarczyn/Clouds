using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleTutorial : MonoBehaviour
{
    public GameObject target;

    void Update() {
        if (Keyboard.current.hKey.wasPressedThisFrame)
            target.SetActive(!target.activeSelf);
    }
}

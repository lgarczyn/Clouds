using UnityEngine;

public class ToggleTutorial : MonoBehaviour
{
    public KeyCode key;
    public GameObject target;

    void Update() {
        if (Input.GetKeyDown(key))
            target.SetActive(!target.activeSelf);
    }
}

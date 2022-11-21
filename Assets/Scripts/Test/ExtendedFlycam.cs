using UnityEngine;
using UnityEngine.InputSystem;

public class ExtendedFlycam : MonoBehaviour
{
    /*
        FEATURES
            WASD/Arrows:    Movement
                      Q:    Climb
                      E:    Drop
                          Shift:    Move faster
                        Control:    Move slower
                            End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
        */

    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        var delta = Mouse.current.delta.ReadValue();
        rotationX += delta.x * cameraSensitivity;
        rotationY += delta.y * cameraSensitivity;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        float horizontal = 0f
          + (Keyboard.current.qKey.isPressed ? -1f : 0f)
          + (Keyboard.current.dKey.isPressed ? -1f : 0f);

        float vertical = 0f
          + (Keyboard.current.wKey.isPressed ? -1f : 0f)
          + (Keyboard.current.sKey.isPressed ? -1f : 0f);

        if (Keyboard.current.shiftKey.isPressed)
        {
            transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * vertical * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * horizontal * Time.deltaTime;
        }
        else if (Keyboard.current.ctrlKey.isPressed)
        {
            transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * vertical * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * horizontal * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * vertical * Time.deltaTime;
            transform.position += transform.right * normalMoveSpeed * horizontal * Time.deltaTime;
        }


        if (Keyboard.current.qKey.isPressed) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
        if (Keyboard.current.eKey.isPressed) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
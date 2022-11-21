using UnityEngine;
using UnityEngine.InputSystem;

public class RandomizeCloudColors : MonoBehaviour
{
  public Light sun;

  [SerializeField][RequiredComponent] CloudMaster reqCloudMaster;

  void Update()
  {
    if (Keyboard.current.numpad1Key.wasPressedThisFrame) sun.color = Random.ColorHSV();
    if (Keyboard.current.numpad2Key.wasPressedThisFrame) reqCloudMaster.colA = Random.ColorHSV();
    if (Keyboard.current.numpad3Key.wasPressedThisFrame) reqCloudMaster.colB = Random.ColorHSV();
    if (Keyboard.current.numpad4Key.wasPressedThisFrame) reqCloudMaster.colC = Random.ColorHSV();

    if (Keyboard.current.numpad0Key.wasPressedThisFrame)
    {
      sun.color = Random.ColorHSV();
      reqCloudMaster.colA = Random.ColorHSV();
      reqCloudMaster.colB = Random.ColorHSV();
      reqCloudMaster.colC = Random.ColorHSV();
    }
    reqCloudMaster.ForceUpdate();
  }
}

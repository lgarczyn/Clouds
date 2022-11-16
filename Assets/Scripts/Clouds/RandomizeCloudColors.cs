using UnityEngine;

public class RandomizeCloudColors : MonoBehaviour
{
  public Light sun;

  [SerializeField][RequiredComponent] CloudMaster reqCloudMaster;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Alpha1)) sun.color = Random.ColorHSV();
    if (Input.GetKeyDown(KeyCode.Alpha2)) reqCloudMaster.colA = Random.ColorHSV();
    if (Input.GetKeyDown(KeyCode.Alpha3)) reqCloudMaster.colB = Random.ColorHSV();
    if (Input.GetKeyDown(KeyCode.Alpha4)) reqCloudMaster.colC = Random.ColorHSV();

    if (Input.GetKeyDown(KeyCode.Alpha0))
    {
      sun.color = Random.ColorHSV();
      reqCloudMaster.colA = Random.ColorHSV();
      reqCloudMaster.colB = Random.ColorHSV();
      reqCloudMaster.colC = Random.ColorHSV();
    }
    reqCloudMaster.ForceUpdate();
  }
}

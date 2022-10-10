using UnityEngine;

public class RandomizeCloudColors : MonoBehaviour
{
  public Light sun;


  void Update()
  {
    var clouds = GetComponent<CloudMaster>();

    if (Input.GetKeyDown(KeyCode.Alpha1)) sun.color = Random.ColorHSV();
    if (Input.GetKeyDown(KeyCode.Alpha2)) clouds.colA = Random.ColorHSV();
    if (Input.GetKeyDown(KeyCode.Alpha3)) clouds.colB = Random.ColorHSV();
    if (Input.GetKeyDown(KeyCode.Alpha4)) clouds.colC = Random.ColorHSV();

    if (Input.GetKeyDown(KeyCode.Alpha0))
    {
      sun.color = Random.ColorHSV();
      clouds.colA = Random.ColorHSV();
      clouds.colB = Random.ColorHSV();
      clouds.colC = Random.ColorHSV();
    }
    clouds.ForceUpdate();
  }
}

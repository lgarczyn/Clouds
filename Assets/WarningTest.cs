using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WarningManagerBridge))]
public class WarningTest : MonoBehaviour
{
  [Range(0, 1)]
  public float chance;

  public List<WarningType> warnings;
  // Update is called once per frame
  void Update()
  {
    if (Random.Range(0f, 1f) < chance && warnings.Count > 0)
    {
      var type = warnings[Random.Range(0, warnings.Count)];
      GetComponent<WarningManagerBridge>().instance.SendWarning(type);
    }
  }
}

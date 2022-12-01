using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DeathControllerBridge : ManagerBridge<DeathController>
{
  public void KillPlane() => instance.KillPlane();
}

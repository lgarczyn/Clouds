using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class WinManager : Manager<WinManager>
{
  public GameObject reachHigherUI;
  public GameObject canWinUI;
  public GameObject[] disableOnThruster;
  public GameObject[] activateOnThrusters;
  public PlaneEntity plane;
  public MouseFlightController mouseFlightController;

  public PlaneThrustController thrustController;

  public PlayerPlane planeController;

  public Key keyToWin;

  public float minHeightToActivateThrust = 700;

  public float minHeightToWin = 3000;

  bool winning = false;

  public void SetWinnable(bool value)
  {
    if (winning) return;

    winning = true;

    StartCoroutine(WinCoroutine());
  }

  IEnumerator WinCoroutine()
  {
    reachHigherUI.SetActive(true);

    yield return new WaitWhile(() => plane.transform.position.y < minHeightToActivateThrust);

    reachHigherUI.SetActive(false);
    canWinUI.SetActive(true);

    yield return new WaitWhile(() => !Keyboard.current[keyToWin].wasPressedThisFrame);

    canWinUI.SetActive(false);

    foreach (var go in disableOnThruster)
    {
      go.SetActive(false);
    }

    foreach (var go in activateOnThrusters)
    {
      go.SetActive(true);
    }
    thrustController.boostMultiplier = 1f;
    planeController.turnTorque = Vector3.zero;
    
    while (plane.transform.position.magnitude < minHeightToWin)
    {
      thrustController.baseThrust *= Mathf.Pow(2, Time.deltaTime);
      yield return null;
    }
    
    PlaythroughData.instance.timeSinceGameStart = Time.timeSinceLevelLoad;
    SceneManager.LoadScene("Credits");
  }
}

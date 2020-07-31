using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaneWinController : MonoBehaviour
{
    public GameObject matterFullUI;
    public GameObject canWinUI;
    public GameObject[] disableOnThruster;
    public GameObject[] activateOnThrusters;
    public PlaneEntity plane;

    public MFlight.Demo.Plane flightController;

    public KeyCode keyToWin;

    public float minHeightToActivateThrust = 700;

    public float minHeightToWin = 3000;

    bool isFull = false;
    bool thrustersActivated = false;
 
    public void SetMatterFull(bool value) {
        isFull = value;
    }

    void Update() {

        bool canWin = transform.position.y > minHeightToActivateThrust;

        matterFullUI.SetActive(isFull && !canWin);
        canWinUI.SetActive(isFull && canWin);

        if (isFull && canWin) {
            if (Input.GetKeyDown(keyToWin)) {
                
                foreach (var go in disableOnThruster)
                {
                    go.SetActive(false);
                }
                
                foreach (var go in activateOnThrusters)
                {
                    go.SetActive(true);
                }

                flightController.thrust *= 20;

                thrustersActivated = true;
            }
        }

        if (thrustersActivated && transform.position.y > minHeightToWin) {
            PlaythroughData.instance.timeSinceGameStart = Time.timeSinceLevelLoad;
            SceneManager.LoadScene("Credits");
        }
    }
}

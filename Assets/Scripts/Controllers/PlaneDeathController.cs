using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaneDeathController : MonoBehaviour
{
    public Rigidbody planeRigidbody;
    public MFlight.Demo.Plane planeScript;

    public GameObject[] toDisable;

    Vector3 prevVelocity;

    public void FixedUpdate() {
        prevVelocity = planeRigidbody.velocity;
    }

    public void KillPlane() {

        foreach (var go in toDisable)
        {
            go.SetActive(false);
        }

        planeRigidbody.drag = 0f;
        planeRigidbody.detectCollisions = false;
        planeRigidbody.velocity = prevVelocity;
        planeScript.thrust = 0f;

        StartCoroutine(RespawnCoroutine());
    }

    public IEnumerator RespawnCoroutine() {
        
        yield return new WaitForSeconds(5);

        SceneManager.LoadScene(0);
    }
}

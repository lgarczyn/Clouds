using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaneDeathController : MonoBehaviour
{
  public Rigidbody planeRigidbody;
  public PlayerPlane planeScript;

  public GameObject[] toDisable;

  public float gravityMultiplier = 3f;

  Vector3 prevVelocity;

  public bool isDying = false;

  public bool canDie = false;

  public void FixedUpdate()
  {
    prevVelocity = planeRigidbody.velocity;

    if (isDying)
    {
      // Add more gravity during death anim
      planeRigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }
  }

  public void KillPlane()
  {
    if (!canDie) return;

    isDying = true;

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

  public IEnumerator RespawnCoroutine()
  {

    yield return new WaitForSeconds(5);

    isDying = false;

    PlaythroughData.instance.timeSinceGameStart = Time.timeSinceLevelLoad;

    SceneManager.LoadScene("GameOver");
  }
}

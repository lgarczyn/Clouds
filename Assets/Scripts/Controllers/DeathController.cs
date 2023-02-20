using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DeathController : Manager<DeathController>
{
  public Rigidbody planeRigidbody;
  public PlayerPlane planeScript;

  public UnityEvent onDeath;

  public float gravityMultiplier = 3f;

  Vector3 prevVelocity;

  public bool isDying = false;

  public bool canDie = false;

  public bool kill = false;

  public void FixedUpdate()
  {
    prevVelocity = planeRigidbody.velocity;

    if (isDying)
    {
      // Add more gravity during death anim
      planeRigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }
    if (kill)
    {
      kill = false;
      KillPlane();
    }
  }

  public void KillPlane()
  {
    if (!canDie) return;

    isDying = true;

    onDeath.Invoke();

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

    SceneManager.LoadScene("GameOver");

    PlaythroughData.instance.timeSinceGameStart = Time.timeSinceLevelLoad;
  }
}

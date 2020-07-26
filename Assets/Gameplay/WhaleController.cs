using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WhaleController : MonoBehaviour
{
    new Rigidbody rigidbody;
    Vector3 velocity = Vector3.forward;
    public float diveCounter = 0f;

    
    public Transform container;
    public Transform player;

    public Transform model;
    public float depopDistance = 1000f;
    public float maxRepopDistance = 500f;
    public float minRepopDistance = 200f;
    public float containerHeightRepopRange = 0.8f;
    public float dirChangeSpeed = 0.1f;
    public float speed = 0.2f;
    public float rotationSpeed = 0.1f;
    public float diveDuration = 2f;
    public float diveSpeed = 0.1f;
    public float scaleRatioRange = 1.2f;

    public float minHeight { get {
        return container.position.y - container.localScale.y / 2f;
    }}
    public float maxHeight { get {
        return container.position.y + container.localScale.y / 2f;
    }}

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
        velocity = Random.insideUnitSphere * speed;
        Repop();
        rigidbody.rotation = Quaternion.LookRotation(velocity, Vector3.up);

        model.localScale *= Random.Range(1f / scaleRatioRange, scaleRatioRange);
    }

    void FixedUpdate()
    {
        // Apply random change to velocity
        Vector3 velocityChange = Random.insideUnitSphere * dirChangeSpeed;
        // Add drag
        velocity *= velocity.sqrMagnitude > 1f ? 0.9f : 1.1f;
        // Calculate new velocity plus the dive bias
        velocity += (velocityChange + GetDiveVelocity()) * Time.fixedDeltaTime;
        
        // Either repop or move normally
        if (ShouldRepop())
            Repop();
        else {
            // Move forward
            rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime * speed);
            // Rotate forward
            rigidbody.MoveRotation(
                Quaternion.Slerp(
                    rigidbody.rotation,
                    Quaternion.LookRotation(velocity, Vector3.up),
                    Time.fixedDeltaTime * rotationSpeed));
        }
    }

    // Move the whale in a hollow cylinder around the player 
    void Repop() {
        Vector3 playerPos = new Vector3(player.position.x, 0f, player.position.z);

        float randomRadius = Random.Range(minRepopDistance, maxRepopDistance);
        float randomAngle = Random.Range(-Mathf.PI, Mathf.PI);
        float randomHeight = Random.Range(minHeight, maxHeight) * containerHeightRepopRange;


        Vector3 randomPos = new Vector3(
            Mathf.Cos(randomAngle) * randomRadius,
            randomHeight,
            Mathf.Sin(randomAngle) * randomRadius
        );

        rigidbody.position = playerPos + randomPos;
    }

    // Calculate velocity bias to stay in container
    Vector3 GetDiveVelocity() {
        // If outside of bounds, set a pool of velocity to be spread over diveDuration
        float newCounter =
            rigidbody.position.y > maxHeight ? -1f:
            rigidbody.position.y < minHeight ? 1f:
            0f;
        
        newCounter *= diveDuration / Time.fixedDeltaTime;

        // If the previous pool is closer to 0, replace it
        if (Mathf.Abs(newCounter) > Mathf.Abs(diveCounter))
            diveCounter = newCounter;

        Vector3 returnVelocity = Vector3.up * Mathf.Clamp(diveCounter, -1f, 1f) * diveSpeed; 

        // If the pool is more than tick time, reduce it
        if (Mathf.Abs(diveCounter) > 1f)
            diveCounter -= Mathf.Sign(diveCounter);
        else
            diveCounter = 0f;

        return returnVelocity;
    }

    bool ShouldRepop() {
        Vector2 playerPos = new Vector2(player.position.x, player.position.z);
        Vector2 whalePos = new Vector2(rigidbody.position.x, rigidbody.position.z);

        return Vector2.Distance(playerPos, whalePos) > depopDistance;
    }
}

using UnityEngine;

/// <summary>
/// A component capable of receiving damage
/// </summary>
public interface ICollisionReceiver
{
  /// <summary>
  /// Called by CollisionManager when a collider on the same go is hit
  /// If no ICollisionReceiver is found on the collider,
  /// CollisionManager will look on the rigidbody
  /// </summary>
  /// <param name="info"></param>
  void Collide(Collision info);
}
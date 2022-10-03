using UnityEngine;

public struct DamageInfo
{
  /// <summary>
  /// The raw damage of the attack
  /// </summary>
  public float damage;
  /// <summary>
  /// True of single hit, false if DPS
  /// </summary>
  public bool oneOff;
  /// <summary>
  /// The contact point between the projectile and the target
  /// </summary>
  public Vector3 position;
  /// <summary>
  /// The normal vector of the collision
  /// </summary>
  public Vector3 normal;
  /// <summary>
  /// The velocity of the projectile minus the velocity of the target
  /// </summary>
  public Vector3 relativeVelocity;
}

/// <summary>
/// A component capable of receiving damage
/// </summary>
public interface IDamageReceiver
{
  /// <summary>
  /// Applies damage to the target
  /// </summary>
  /// <param name="info"> Struct containing all damage source info </param>
  /// <returns>Remaining life of the target, negative if damage was overkill</returns>
  float Damage(DamageInfo info);
}
using UnityEngine;

/// <summary>
/// A component capable of dealing damage on contact
/// </summary>
public interface IDamageDealer
{
  /// <summary>
  /// Get the amount of damage on collision
  /// </summary>
  float GetDamageHit();

  /// <summary>
  /// Get the amount of each frame of contact
  /// </summary>
  float GetDamageFrame();
}
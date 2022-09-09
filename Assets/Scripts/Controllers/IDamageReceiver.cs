/// <summary>
/// A component capable of receiving damage
/// </summary>
public interface IDamageReceiver
{
  /// <summary>
  /// Applies damage to the target
  /// </summary>
  /// <param name="damage"> The amount of damage </param>
  /// <param name="onOff"> True if the damage is a single event, false if continuous </param>
  /// <returns>Remaining life of the target, negative if damage was overkill</returns>
  float Damage(float damage, bool oneOff = true);
}
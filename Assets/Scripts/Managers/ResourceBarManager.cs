using UnityEngine;

public interface IResourceBarManager : IManager<IResourceBarManager>
{
  public void SetHealth(float value);
  public void SetShield(float value);
  public void SetEnergy(float value);
}

public class ResourceBarManager : Manager<ResourceBarManager>
{
  public ResourceBar health;
  public ResourceBar shield;
  public ResourceBar energy;

  public void SetHealth(float value)
  {
    this.health.SetValue(value);
  }

  public void SetShield(float value)
  {
    this.shield.SetValue(value);
  }

  public void SetEnergy(float value)
  {
    this.energy.SetValue(value);
  }
}

public class ResourceBarManagerBridge : ManagerBridge<ResourceBarManager>
{
  public void SetHealth(float value)
  {
    this.tryInstance?.SetHealth(value);
  }

  public void SetShield(float value)
  {
    this.tryInstance?.SetShield(value);
  }

  public void SetEnergy(float value)
  {
    this.tryInstance?.SetEnergy(value);
  }
}

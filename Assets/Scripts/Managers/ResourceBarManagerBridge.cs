public class ResourceBarManagerBridge : ManagerBridge<ResourceBarManager>
{
  public void SetHealth(float value)
  {
    this.instance.SetHealth(value);
  }

  public void SetShield(float value)
  {
    this.instance.SetShield(value);
  }

  public void SetEnergy(float value)
  {
    this.instance.SetEnergy(value);
  }
}

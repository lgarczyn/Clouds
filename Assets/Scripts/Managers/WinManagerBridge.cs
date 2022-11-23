public class WinManagerBridge : ManagerBridge<WinManager>
{
  public void SetWinnable(bool value)
  {
    instance.SetWinnable(value);
  }
}

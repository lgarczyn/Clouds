
/// <summary>
/// Interface implemented by all managers and manager-like interfaces
/// Inheriting from this is only required for dependency injections of managers
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IManager<T>
where T : class, IManager<T>
{
  public bool isActiveAndEnabled { get; }
}
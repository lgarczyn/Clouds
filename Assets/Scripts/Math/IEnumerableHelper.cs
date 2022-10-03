using System.Collections.Generic;

public static class IEnumerableHelper
{
  public static void Do<T>(this IEnumerable<T> list, System.Action<T> action)
  {
    foreach (T item in list)
    {
      action(item);
    }
  }
  public static IEnumerable<T> DoLazy<T>(this IEnumerable<T> list, System.Action<T> action)
  {
    foreach (T item in list)
    {
      action(item);
      yield return item;
    }
  }

  public static void Consume<T>(this IEnumerable<T> list, System.Action<T> action)
  {
    foreach (T item in list) { }
  }
}
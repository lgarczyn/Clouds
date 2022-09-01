using System.Collections.Generic;

public static class IEnumerableHelper
{
  public static IEnumerable<T> Do<T>(this IEnumerable<T> list, System.Action<T> action)
  {
    foreach (T item in list)
    {
      action(item);
      yield return item;
    }
  }
}